using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;


namespace QidWorkerRole
{
    public class RapidException
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<RapidException> _logger;
        private readonly balRapidInterfaceForCebu _balRapidInterfaceForCebu;
        private readonly AppConfig _appConfig;   

        #region Constructor
        public RapidException(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<RapidException> logger,
            balRapidInterfaceForCebu balRapidInterfaceForCebu,
            AppConfig appConfig)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _balRapidInterfaceForCebu = balRapidInterfaceForCebu;
            _appConfig = appConfig;
        }
        #endregion
        public async Task RapidExceptionCEBU()
        {try
        {
            
                //clsLog.WriteLogAzure("Calling start RapidExceptionCEBU");
                _logger.LogInformation("Calling start RapidExceptionCEBU");
    
                //String TimeZone = System.Configuration.ConfigurationManager.AppSettings["UTCORLOCALTIME"].ToString();
                string TimeZone = _appConfig.Miscellaneous.UTCORLOCALTIME;

                DateTime ExecutedOn = DateTime.Now;
                DateTime FromDate = DateTime.Now;
                DateTime ToDate = DateTime.Now;
    
                if (TimeZone.ToUpper() == "UTC")
                {
                    // UTC Time 
                    ExecutedOn = DateTime.Now;
                    FromDate = DateTime.Now;
                    ToDate = DateTime.Now;
                }
                else
                {
                    ExecutedOn = DateTime.Now;
                    FromDate = ExecutedOn;
                    ToDate = ExecutedOn;
    
                }
                //balRapidInterfaceForCebu objBAL = new balRapidInterfaceForCebu();
    
                #region MissingAWBFlown Data
    
                DataSet dsRapidException = new DataSet();
                dsRapidException = await _balRapidInterfaceForCebu.GetMissingAWBFlownDetails(Convert.ToDateTime(ExecutedOn), FromDate, ToDate);
                if (dsRapidException != null & dsRapidException.Tables.Count > 0)
                {
                    //clsLog.WriteLogAzure("GetMissingAWBFlownDetails Process start SK to RAPID Interface..");
                    _logger.LogInformation("GetMissingAWBFlownDetails Process start SK to RAPID Interface..");
    
                    DataTable dtAWB = dsRapidException.Tables[0];
                    DataTable dtFlown = dsRapidException.Tables[1];
                    //string toId = GetConfigurationValue("ToEmailIDForRapidException");
                    //string fromID = GetConfigurationValue("msgService_OutEmailId");
                    
                    string toId = ConfigCache.Get("ToEmailIDForRapidException");
                    string fromID = ConfigCache.Get("msgService_OutEmailId");
    
                    string ToEmailID = string.Empty;
                    string strSubject = "SK to RAPID Interface: Missing AWBs from: " + DateTime.Today.ToString("MMddyyyy");
    
                    if (!string.IsNullOrEmpty(toId))
                    {
                        ToEmailID = toId;
                    }
                    string body = string.Empty;
    
                    if ((dtFlown.Rows.Count > 0 || dtAWB.Rows.Count > 0)&& ToEmailID.Length>1 )
                    {
                        //clsLog.WriteLogAzure("GetMissingAWBFlownDetails Process start SK to RAPID Interface..");
                        _logger.LogInformation("GetMissingAWBFlownDetails Process start SK to RAPID Interface..");
    
                        body += "<p style='padding: 3px;font-family:Calibri;'><b/>1.AWB</p>";
                        body += "<table  border=1 style='border-collapse:collapse;padding: 5px;font-family:Calibri;width:600px'>";
                        body += "<tr style='padding: 3px'><td style='padding: 3px'> <b/>AWB Number </td><td style='padding: 3px'> <b/>Date Accepted </td><td style='padding: 3px'> <b/>Origin / Destination </td></tr>";
                        if (dtAWB.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtAWB.Rows)
                            {
                                string awbNumber = dr["AWBNumber"].ToString();
                                string dateAccepted = dr["AcceptedDate"].ToString();
                                string routing = dr["routing"].ToString();
                                body += "<tr style='padding: 3px'>";
                                body += $"<td style='padding: 3px'>{awbNumber}</td>";
                                body += $"<td style='padding: 3px'>{dateAccepted}</td>";
                                body += $"<td style='padding: 3px'>{routing}</td>";
                                body += "</tr>";
                            }
                        }
     
                        body += "<tr style='padding: 3px;height:20px'> <td style='padding: 3px'><br/></td ><td style='padding: 3px'></td ><td style='padding: 3px'></td > </tr>";
                        body += "</table>";
    
                        body += "<p style='padding: 3px;font-family:Calibri;'><b/>2.FLOWN</p>";
                        body += "<table border=1 style='border-collapse:collapse;padding: 5px;font-family:Calibri;width:600px '>";
                        body += "<tr style='padding: 3px'><td style='padding: 3px'><b/> Carrier </td><td style='padding: 3px'><b/> Flight Number </td><td style='padding: 3px'> <b/>Flight Date </td><td style='padding: 3px'> <b/>Origin / Destination </td><td style='padding: 3px'><b/> AWB Number </td></tr>";
                        if (dtFlown.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtFlown.Rows)
                            {
                                string carrier = dr["Carrier"].ToString();
                                string flightno = dr["FlightNo"].ToString();
                                string flightdate = dr["FlightDate"].ToString();
                                string routing = dr["routing"].ToString();
                                string awbNumber = dr["AWBNumber"].ToString();
    
                                body += "<tr >";
                                body += $"<td >{carrier}</td>";
                                body += $"<td >{flightno}</td>";
                                body += $"<td >{flightdate}</td>";
                                body += $"<td >{routing}</td>";
                                body += $"<td >{awbNumber}</td>";
                                body += "</tr>";
                            }
                        }
    
                        body += "<tr style='padding: 3px;height:20px'> <td style='padding: 3px'><br/></td><td style='padding: 3px'></td ><td style='padding: 3px'></td ><td style='padding: 3px'></td ><td style='padding: 3px'></td > </tr>";
                        body += "</table>";
    
                        //clsLog.WriteLogAzure("RAPIDEXCEPTIONREPORT Reports sending");
                        _logger.LogInformation("RAPIDEXCEPTIONREPORT Reports sending");
    
                        await addMsgToOutBox(strSubject, body, fromID, ToEmailID, false, true, "RAPIDEXCEPTIONREPORT");
                        
                        //clsLog.WriteLogAzure("RAPIDEXCEPTIONREPORT Reports Sent");
                        _logger.LogInformation("RAPIDEXCEPTIONREPORT Reports Sent");
                    }
                }
                else
                {
                    //clsLog.WriteLog("No Missing Data found in AWB or Flown:");
                    _logger.LogInformation("No Missing Data found in AWB or Flown:");
                }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            throw;
        }

            #endregion
        }
        //public static string GetConfigurationValue(string Key)
        //{
        //    SqlDataAdapter objDA = null;
        //    DataSet objDs = null;
        //    string FileName = string.Empty;
        //    try
        //    {

        //        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();

        //        string Command = "Exec [dbo].[uspGetTblConfiguration] '" + Key + "'";
        //        objDA = new SqlDataAdapter(Command, connectionString);
        //        objDA.SelectCommand.CommandTimeout = 0;
        //        objDs = new DataSet();
        //        objDA.Fill(objDs);

        //        if (objDs != null && objDs.Tables.Count > 0 && objDs.Tables[0].Rows.Count > 0)
        //        {
        //            FileName = objDs.Tables[0].Rows[0]["values"].ToString();

        //        }
        //        return FileName;

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return "";

        //    }
        //    finally
        //    {
        //        objDA = null;
        //        objDs = null;
        //    }
        //}

        public async Task<bool> addMsgToOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, bool isInternal, bool isHTML, string type)
        {
            bool flag = false;
            try
            {
                //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                //SqlConnection con = new SqlConnection(connectionString);
                //con.Open();
                //SqlCommand cmd = new SqlCommand();

                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "spInsertMsgToOutbox";
                //cmd.Connection = con;
                SqlParameter[] prm = new SqlParameter[] {
                    new SqlParameter("@Subject",subject)
                    ,new SqlParameter("@Body",Msg)
                    ,new SqlParameter("@FromEmailID",FromEmailID)
                    ,new SqlParameter("@ToEmailID",ToEmailID)
                    ,new SqlParameter("@Type",type)
                    ,new SqlParameter("@IsHTML",isHTML)
                };

                //cmd.Parameters.AddRange(prm);
                //cmd.ExecuteNonQuery();
                await _readWriteDao.ExecuteNonQueryAsync("spInsertMsgToOutbox", prm);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in addMsgToOutBox: {ErrorMessage}", ex.Message);
                flag = false;
            }
            return flag;
        }

    }
}
