using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
//using QID.DataAccess; not in used
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole
{
    public class FRPMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FRPMessageProcessor> _logger;

        #region Constructor
        public FRPMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FRPMessageProcessor> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion
        /// <summary>
        /// Method to decode incomming FRP message
        /// </summary>
        /// <param name="strMsg">message string</param>
        /// <param name="fwbinfo">FWB information</param>
        /// <param name="frpinfo">FRP Information</param>
        /// <returns>Return true if message decoded successfully</returns>
        public bool DecodeFRPReceiveMessage(string strMsg, ref MessageData.fwbinfo[] fwbinfo, ref MessageData.frpinfo[] frpinfo, ref string errorMessage)
        {
            bool isDecodeSuccess = true;
            try
            {
                if (strMsg.StartsWith("FRP", StringComparison.OrdinalIgnoreCase))
                {
                    string[] messageContent = strMsg.Split('$');
                    for (int i = 0; i < messageContent.Length; i++)
                    {
                        if (i == 1)
                        {
                            string[] consginfo = messageContent[i].Split('/');
                            fwbinfo[0].airlineprefix = consginfo[0].Substring(0, 3);
                            fwbinfo[0].awbnum = consginfo[0].Substring(4, 8);
                        }
                        ///Other Service Information
                        if (messageContent[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] osi = messageContent[i].Split('/');
                            if (osi.Length > 1)
                            {
                                frpinfo[0].remarks = osi[1];
                            }
                        }
                        ///Authorisation 
                        if (messageContent[i].StartsWith("ATH", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] ath = messageContent[i].Split('/');
                            if (ath.Length > 3)
                            {
                                DateTime dt;
                                if (DateTime.TryParse(ath[1], out dt))
                                {
                                    frpinfo[0].bookingdate = dt;
                                }
                                frpinfo[0].executedat = ath[2];
                                frpinfo[0].user = ath[3];
                            }
                            else
                            {
                                errorMessage = "Invalid message";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                return false;
            }
            return isDecodeSuccess;
        }

        /// <summary>
        /// Method to validate FRP data and save to the database
        /// </summary>
        /// <param name="fwbinfo">FWB information</param>
        /// <param name="frpinfo">FRP information</param>
        /// <returns>Returns true if message saved successfully</returns>
        public async Task<bool> ValidateAndSaveFRPMessage(MessageData.fwbinfo[] fwbinfo, MessageData.frpinfo[] frpinfo)
        {
            bool isSavedSuccess = false;
            DataSet dsResult = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameters = new SqlParameter[]{
                     new SqlParameter("AWBPrefix",fwbinfo[0].airlineprefix)
                    ,new SqlParameter("AWBNumber",fwbinfo[0].awbnum)
                    ,new SqlParameter("Remarks",frpinfo[0].remarks)
                    ,new SqlParameter("AWBDate",frpinfo[0].bookingdate)
                    ,new SqlParameter("UserName",frpinfo[0].user)
                };

                //dsResult = sqlServer.SelectRecords("uspSaveAWBRemarksThroughFRP", sqlParameters);
                dsResult = await _readWriteDao.SelectRecords("uspSaveAWBRemarksThroughFRP", sqlParameters);
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    if (Convert.ToBoolean(dsResult.Tables[0].Rows[0]["Result"].ToString().ToUpper().Trim()))
                    {
                        isSavedSuccess = true;
                    }
                    else
                    {
                        isSavedSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                return false;
            }
            return isSavedSuccess;
        }
    }
}
