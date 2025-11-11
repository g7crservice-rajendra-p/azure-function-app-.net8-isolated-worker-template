using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;

namespace QidWorkerRole
{
    public class DataBaseMessageProcessor
    {
        //SCMExceptionHandlingWorkRole scmexception = new SCMExceptionHandlingWorkRole();
        //GenericFunction genericFunction = new GenericFunction();
        //string msgType = string.Empty;


        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<Cls_BL> _logger;
        private readonly GenericFunction _genericFunction;
        private readonly cls_SCMBL _clsSCMBL;
        public DataBaseMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<Cls_BL> logger,
            GenericFunction genericFunction,
            cls_SCMBL clsSCMBL)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _clsSCMBL = clsSCMBL;
        }


        #region Below Method Used call on AzzureJob
        public void MessageDataBaseProcessed()
        {
            IncomingMessageProcesseandUpdatetheDatabase();
            AutogenearateMessageandsend();
        }
        #endregion

        #region Below Method used to update transaction table from tblinbox
        private async Task IncomingMessageProcesseandUpdatetheDatabase()
        {
            try
            {
                //SQLServer db = new SQLServer();
                DataSet? ds = null;
                string status = "Re-Processed", MessageFrom = string.Empty;

                // bool flag = false;                    
                //ds = db.SelectRecords("spGetMessageForInsert");

                ds = await _readWriteDao.SelectRecords("spGetMessageForInsert");

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
                    {

                        int srno = Convert.ToInt32(ds.Tables[0].Rows[row][0].ToString());
                        string message = ds.Tables[0].Rows[row]["body"].ToString();
                        string strFromID = ds.Tables[0].Rows[row]["FromiD"].ToString();
                        string strStatus = ds.Tables[0].Rows[row]["STATUS"].ToString();

                        try
                        {
                            if (ds.Tables[0].Rows[row]["UpdatedBy"].ToString() != "")
                            {
                                if ((message.Contains("=ORIGIN")))
                                {
                                    int indexOfMessageTag = message.IndexOf("=ORIGIN");
                                    MessageFrom = message.Substring(indexOfMessageTag);
                                    MessageFrom = MessageFrom.Substring(9, 7).ToString();
                                }
                            }
                            else
                                MessageFrom = ds.Tables[0].Rows[row]["UpdatedBy"].ToString();

                        }
                        catch (Exception)
                        {
                            MessageFrom = string.Empty;
                        }

                        if (message.Contains("ZCZC") && message.Contains("NNNN"))
                            message = _genericFunction.ExtractFromString(message, "ZCZC", "NNNN");

                        if (message.Contains("=SMI"))
                            message = _genericFunction.ExtractFromString(message, "=SMI", "");
                        else if (message.Contains("=TEXT"))
                            message = _genericFunction.ExtractFromString(message, "=TEXT", "");
                        message = _genericFunction.ReplaceBlankSpaces(message);

                        string msgType = string.Empty;
                        //cls_SCMBL clscmbl = new cls_SCMBL();
                        string Errmsg = string.Empty;

                        //if (!clscmbl.addBookingFromMsg(genericFunction.RemoveSITAHeader(message), srno, MessageFrom, out msgType, strFromID, strStatus, "", out Errmsg))
                        //{
                        //    status = "Failed";
                        //}

                        bool flag = false;
                        var transformedMsg = _genericFunction.RemoveSITAHeader(message);
                        (flag, msgType, Errmsg) = await _clsSCMBL.addBookingFromMsg(transformedMsg, srno, MessageFrom, strFromID, strStatus, "");

                        if (flag)
                        {
                            status = "Failed";
                        }

                        if (ds.Tables[0].Rows[row]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
                        {
                            status = "Processed";
                        }
                        if (ds.Tables[0].Rows[row]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                        {
                            status = "Re-Processed";
                        }

                        //string[] PName = new string[] { "srno", "status", "body" }; //  In sp body will update to Mssage Type (Cond is handeled in DB)
                        //object[] PValues = new object[] { Convert.ToInt32(ds.Tables[0].Rows[row][0].ToString()), status, msgType };
                        //SqlDbType[] PType = new SqlDbType[] { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar };

                        SqlParameter[] parameters =
                        {
                            new SqlParameter("@srno", SqlDbType.Int) { Value = Convert.ToInt32(ds.Tables[0].Rows[row][0].ToString()) },
                            new SqlParameter("@status", SqlDbType.VarChar) { Value = status },
                            new SqlParameter("@body", SqlDbType.VarChar) { Value = msgType }
                        };

                        //if (!db.ExecuteProcedure("spUpdateMessageStatus", PName, PType, PValues))

                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spUpdateMessageStatus", parameters);


                        if (!dbRes)
                        {
                            clsLog.WriteLogAzure("Error Status Update:" + ds.Tables[0].Rows[row][0].ToString());
                        }
                    }
                }

                //db = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                clsLog.WriteLogAzure("Exception in CheckMessagesforProcessing:", ex);
            }
        }

        #endregion

        #region  AutoGenerateMessage
        private void AutogenearateMessageandsend()
        {
            //#region  Auto Send FSU Message
            //SQLServer dbfusdlvConnection = new SQLServer();
            //DataSet dsdlv = null;
            //dsdlv = dbfusdlvConnection.SelectRecords("GetRecordforMakeFSUMessage");
            //dbfusdlvConnection = null;
            //#endregion
            try
            {

                #region Check FFA for send
                clsLog.WriteLogAzure("FFA Processing");
                FFAMessageProcessor ffmMessage = new FFAMessageProcessor();
                ffmMessage.MakeanSendFFAMessage();

                #endregion

                #region Auto FBL functionality
                string f_AutoFBL;
                //f_AutoFBL = _genericFunction.ReadValueFromDb("AUTOFBL");

                f_AutoFBL = ConfigCache.Get("AUTOFBL");


                if (f_AutoFBL.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    FBLMessageProcessor fblprocessor = new FBLMessageProcessor();
                    fblprocessor.GenerateAutoFBLMessage();
                }
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);

                clsLog.WriteLog("Exception in Auto FBL process:" + ex.Message);
            }
            #endregion
        }
        #endregion
    }
}


