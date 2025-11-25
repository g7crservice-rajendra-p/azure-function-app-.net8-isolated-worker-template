using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.Text;
using WinSCP;

namespace QidWorkerRole
{
    public class RapidInterfaceMethods
    {
        static Dictionary<string, string> objDictionary = null;
        static Dictionary<string, string> objUploadDictionary = null;

        //static string BlobKey = String.Empty;
        //static string BlobName = String.Empty;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //static void Main()
        //{
        //    try
        //    {
        //        UpdateRapidDetails();
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("----------------------------------------------------------------------------------------------------------------------");
        //        clsLog.WriteLogAzure("Exception::" + ex.Message);

        //    }
        //    finally
        //    {
        //        objDictionary.Clear();
        //        objUploadDictionary.Clear();
        //        objDictionary = null;
        //        objUploadDictionary = null;
        //    }
        //}

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<RapidInterfaceMethods> _logger;
        private readonly AppConfig _appConfig;
        private readonly balRapidInterfaceForCebu _balRapidInterfaceForCebu;
        private readonly balRapidInterface _balRapidInterface;
        public RapidInterfaceMethods(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<RapidInterfaceMethods> logger,
            AppConfig appConfig,
            balRapidInterfaceForCebu balRapidInterfaceForCebu,
            balRapidInterface balRapidInterface
         )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _appConfig = appConfig;
            _balRapidInterfaceForCebu = balRapidInterfaceForCebu;
            _balRapidInterface = balRapidInterface;
        }

        public async Task UpdateRapidDetailsForCebu()
        {
            try
            {
                //String TimeZone = ConfigurationManager.AppSettings["UTCORLOCALTIME"].ToString();

                //string TimeZone = _appConfig.Miscellaneous.UTCORLOCALTIME;
                string TimeZone = ConfigCache.Get("UTCORLOCALTIME");

                DateTime ExecutedOn = DateTime.Now;
                DateTime FromDate = DateTime.Now;
                DateTime ToDate = DateTime.Now;

                if (TimeZone.ToUpper() == "UTC")
                {
                    // UTC Time 
                    ExecutedOn = DateTime.Now.AddDays(+1);
                    FromDate = DateTime.Now.AddDays(0);
                    ToDate = DateTime.Now.AddDays(0);
                }
                else
                {
                    ExecutedOn = DateTime.Now.AddDays(0);
                    FromDate = ExecutedOn.AddDays(-1);
                    ToDate = ExecutedOn.AddDays(-1);

                }
                // clsLog.WriteLogAzure("------------------------- doProcess() Started ---------------------------------------------------------------------");
                _logger.LogInformation("------------------------- doProcess() Started ---------------------------------------------------------------------");
                objDictionary = new Dictionary<string, string>();
                objUploadDictionary = new Dictionary<string, string>();

                //string FilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string flnFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string salesFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string ccaFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();

                string FilePath = _appConfig.Miscellaneous.XMLFilePath;
                string flnFilePath = _appConfig.Miscellaneous.XMLFilePath;
                string salesFilePath = _appConfig.Miscellaneous.XMLFilePath;
                string ccaFilePath = _appConfig.Miscellaneous.XMLFilePath;

                //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                string XML = string.Empty;

                //balRapidInterfaceForCebu objBAL = new balRapidInterfaceForCebu();

                // clsLog.WriteLogAzure("----------------------------------------------------------------------------------------------------------------------");
                // clsLog.WriteLogAzure("Schedular run on ::" + System.DateTime.Now);
                _logger.LogInformation("----------------------------------------------------------------------------------------------------------------------");
                _logger.LogInformation("Schedular run on :: {0}", System.DateTime.Now);
                objDictionary.Add("File Names ", "Status");
                #region "AWB RAPID File"

                try
                {
                    string filenameCTM = string.Empty;
                    DataSet? dsRpdIntrfcAWBFileName = new DataSet();
                    DataSet dsAWBData = new DataSet();

                    StringBuilder sbAWB = new StringBuilder();
                    string filenameAWBData = string.Empty;

                    dsRpdIntrfcAWBFileName = await _balRapidInterfaceForCebu.InsertRapidInterfaceData(Convert.ToDateTime(ExecutedOn)
                                                                , "SKAdmin"
                                                                 , FromDate
                                                                 , ToDate
                                                                , filenameCTM, filenameAWBData);
                    // clsLog.WriteLogAzure("--Data Inserted -" + DateTime.Now.ToString() + ":" + FromDate + ":" + ToDate + ":");
                    _logger.LogInformation("--Data Inserted - {0} : {1} : {2} :", DateTime.Now.ToString(), FromDate, ToDate);
                    if (dsRpdIntrfcAWBFileName != null) //dsFileName != null
                    {
                        // clsLog.WriteLogAzure("- dsRpdIntrfcAWBFileName is not null -");
                        _logger.LogInformation("- dsRpdIntrfcAWBFileName is not null -");
                        if (dsRpdIntrfcAWBFileName.Tables.Count > 0)
                        {
                            {
                                DataRow drAWBFileName = dsRpdIntrfcAWBFileName.Tables[0].Rows[0];

                                if (dsRpdIntrfcAWBFileName.Tables[0].Columns.Contains("StatusMsg"))
                                {
                                    if (Convert.ToString(dsRpdIntrfcAWBFileName.Tables[0].Rows[0]["StatusMsg"]).ToUpper() == "FILESALREADYGENERATED")
                                        return;
                                }

                                if (drAWBFileName != null)
                                {
                                    if (!string.IsNullOrEmpty(drAWBFileName["FileName"].ToString()))
                                    {
                                        filenameAWBData = drAWBFileName["FileName"].ToString();
                                        dsAWBData = await _balRapidInterfaceForCebu.GetRapidInterfaceData(filenameAWBData);
                                        if (dsAWBData != null && dsAWBData.Tables.Count > 0)
                                        {
                                            var result = new StringBuilder();
                                            //Header Row
                                            foreach (DataRow row in dsAWBData.Tables[0].Rows) // Select each Row
                                            {
                                                for (int i = 0; i < dsAWBData.Tables[0].Columns.Count; i++)// Write Each coloumn in a Row
                                                {
                                                    result.Append(row[i].ToString());
                                                    result.Append(i == dsAWBData.Tables[0].Columns.Count - 1 ? "\n" : "");
                                                }
                                                result.AppendLine();
                                            }
                                            //[START]AWb, RateLine and OT
                                            foreach (DataRow row in dsAWBData.Tables[1].Rows) // Select each AWB Row
                                            {
                                                //AWB
                                                for (int i = 0; i < dsAWBData.Tables[1].Columns.Count; i++)// Write Each coloumn in a AWB Row
                                                {
                                                    result.Append(row[i].ToString());
                                                    result.Append(i == dsAWBData.Tables[1].Columns.Count - 1 ? "\n" : "");
                                                }
                                                result.AppendLine();

                                                //RateLine
                                                DataRow[] drRateline = dsAWBData.Tables[2].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString() + " AND IssuingCarrierCode=" + "'" + row["IssuingCarrierCode"].ToString() + "'");
                                                foreach (DataRow rowRT in drRateline) // Select each RT Row
                                                {
                                                    for (int i = 0; i < dsAWBData.Tables[2].Columns.Count; i++)// Write Each coloumn in a RT Row
                                                    {
                                                        result.Append(rowRT[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[2].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();
                                                }

                                                //Other Charges
                                                DataRow[] drOtherCharges = dsAWBData.Tables[3].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString() + " AND IssuingCarrierCode=" + "'" + row["IssuingCarrierCode"].ToString() + "'");
                                                foreach (DataRow rowOT in drOtherCharges) // Select each OT Row
                                                {
                                                    for (int i = 0; i < dsAWBData.Tables[3].Columns.Count; i++)// Write Each coloumn in a OT Row
                                                    {
                                                        result.Append(rowOT[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[3].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();
                                                }

                                                //Other Charges
                                                DataRow[] drTaxDetails = dsAWBData.Tables[4].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString() + " AND IssuingCarrierCode=" + "'" + row["IssuingCarrierCode"].ToString() + "'");
                                                foreach (DataRow rowOT in drTaxDetails) // Select each OT Row
                                                {
                                                    for (int i = 0; i < dsAWBData.Tables[4].Columns.Count; i++)// Write Each coloumn in a TX Row
                                                    {
                                                        result.Append(rowOT[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[4].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();
                                                }
                                            }

                                            //[END]AWb, RateLine and OT
                                            //Trailer Row
                                            foreach (DataRow row in dsAWBData.Tables[5].Rows) // Select each Row
                                            {
                                                for (int i = 0; i < dsAWBData.Tables[5].Columns.Count; i++)// Write Each coloumn in a Row
                                                {
                                                    result.Append(row[i].ToString());
                                                    result.Append(i == dsAWBData.Tables[5].Columns.Count - 1 ? "\n" : "");
                                                }
                                                result.AppendLine();
                                            }


                                            sbAWB = result;//EncodeBilling.GettingBillingCASSRecords(dsAWBData);
                                            if (sbAWB != null)
                                            {
                                                #region Upload file to Blob
                                                try
                                                {
                                                    if (dsAWBData != null && dsAWBData.Tables.Count > 0 && dsAWBData.Tables[0].Rows.Count > 0)
                                                    {
                                                        string FileName = filenameAWBData.ToString() + ".txt";
                                                        FilePath = FilePath + @"awb\";
                                                        if (!Directory.Exists(FilePath))
                                                        {
                                                            Directory.CreateDirectory(FilePath);
                                                        }
                                                        if (File.Exists(FilePath + FileName))
                                                            File.Delete(FilePath + FileName);

                                                        File.WriteAllText(FilePath + FileName, sbAWB.ToString());
                                                        objDictionary.Add(FileName, "Success");
                                                        objUploadDictionary.Add(FileName, "");
                                                    }
                                                    else
                                                    {
                                                        objDictionary.Add(filenameAWBData + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                                        objUploadDictionary.Add(filenameAWBData + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                                    }
                                                }
                                                catch
                                                {
                                                    objDictionary.Add("sksap.domcollection" + ExecutedOn.ToString("yyyyMMdd") + ".txt", "Failed");
                                                    objUploadDictionary.Add("sksap.domcollection" + ExecutedOn.ToString("yyyyMMdd") + ".txt", "Failed");
                                                }

                                                #endregion

                                            }
                                            else
                                            {
                                                return;
                                            }


                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }//!string.IsNullOrEmpty(dr["FileName"].ToString())

                                }//if (dr != null)

                            }// [END] dsFileName.Tables[0] != null
                        }

                    } //[END]//if dsFileName != null
                    else
                    {
                        // clsLog.WriteLogAzure("--Else Run -");
                        _logger.LogWarning("--Else Run -");
                        //lblStatus.Text = "Rapid AWB generation failed!";
                        //lblStatus.ForeColor = Color.Red;
                        //return;

                    }
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                #region "Flown Rapid File"
                try
                {
                    DataSet? dsFlownFileName = new DataSet();
                    DataSet? dsFlownData = new DataSet();
                    StringBuilder sbFlown = new StringBuilder();
                    string filenameFlown = "";

                    #region INSERT Flown DATA
                    dsFlownFileName = await _balRapidInterfaceForCebu.InsertRapidFlownTransaction(Convert.ToDateTime(ExecutedOn)
                                                                , "SKAdmin"
                                                                 , FromDate
                                                                 , ToDate
                                                              );



                    if (dsFlownFileName != null)
                    {
                        if (dsFlownFileName.Tables.Count > 0)
                        {
                            if (dsFlownFileName.Tables[0] != null)
                            {
                                DataRow dr = dsFlownFileName.Tables[0].Rows[0];

                                if (dr != null)
                                {
                                    if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
                                    {
                                        filenameFlown = dr["FileName"].ToString();
                                    }
                                    else
                                    {

                                        return;
                                    }
                                }//if (dr != null)
                            }// [END] dsFileName.Tables[0] != null
                        }
                    } //[END]//if dsFileName != null
                    else
                    {


                        return;

                    }
                    #endregion

                    #region GET Flown DATA

                    dsFlownData = await _balRapidInterfaceForCebu.GetRapidFlownTransaction(filenameFlown);
                    if (dsFlownData != null && dsFlownData.Tables.Count > 0)
                    {
                        //DataTable table = ds.Tables[0];// You data table values;
                        var result = new StringBuilder();
                        foreach (DataTable table in dsFlownData.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    result.Append(row[i].ToString());
                                    result.Append(i == table.Columns.Count - 1 ? "\n" : "");
                                }
                                result.AppendLine();
                            }
                        }
                        sbFlown = result;//EncodeBilling.GettingBillingCASSRecords(ds);
                        if (sbFlown != null)
                        {
                            #region Upload file to Blob
                            try
                            {
                                if (dsFlownData != null && dsFlownData.Tables.Count > 0 && dsFlownData.Tables[0].Rows.Count > 0)
                                {
                                    string FileName = filenameFlown.ToString() + ".txt";
                                    flnFilePath = flnFilePath + @"fln\";
                                    if (!Directory.Exists(flnFilePath))
                                    {
                                        Directory.CreateDirectory(flnFilePath);
                                    }
                                    if (File.Exists(flnFilePath + FileName))
                                        File.Delete(flnFilePath + FileName);

                                    File.WriteAllText(flnFilePath + FileName, sbFlown.ToString());
                                    objDictionary.Add(FileName, "Success");
                                    objUploadDictionary.Add(FileName, "");
                                }
                                else
                                {
                                    objDictionary.Add(filenameFlown + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                    objUploadDictionary.Add(filenameFlown + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                }
                            }
                            catch (Exception exc)
                            {
                                // clsLog.WriteLogAzure(exc);
                                _logger.LogError(exc, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                objDictionary.Add("sksap.domcollection" + ExecutedOn.ToString("yyyyMMdd") + ".txt", "Failed");
                                objUploadDictionary.Add("sksap.domcollection" + ExecutedOn.ToString("yyyyMMdd") + ".txt", "Failed");
                            }

                            #endregion
                        }
                    }
                    else
                    {

                        //return;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                #region"Export Sales Rapid file"

                DataSet? dsExportData = new DataSet();
                DataSet? dsExportFileName = new DataSet();
                StringBuilder sbExportFile = new StringBuilder();
                string filenameExport = "";
                try
                {
                    #region INSERT Flown DATA

                    dsExportFileName = await _balRapidInterfaceForCebu.InsertRapidExportSales(Convert.ToDateTime(ExecutedOn)
                                                           , "SKAdmin"
                                                            , FromDate
                                                            , ToDate
                                                       );

                    if (dsExportFileName != null)
                    {
                        if (dsExportFileName.Tables.Count > 0)
                        {
                            if (dsExportFileName.Tables[0] != null)
                            {
                                DataRow dr = dsExportFileName.Tables[0].Rows[0];

                                if (dr != null)
                                {
                                    if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
                                    {
                                        filenameExport = dr["FileName"].ToString();
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }//if (dr != null)
                            }// [END] dsFileName.Tables[0] != null
                        }
                    } //[END]//if dsFileName != null
                    else
                    {
                        return;
                    }
                    #endregion

                    #region GET Flown DATA

                    dsExportData = await _balRapidInterfaceForCebu.GetRapidExportSalesTransaction(filenameExport);
                    if (dsExportData != null && dsExportData.Tables.Count > 0)
                    {
                        //DataTable table = ds.Tables[0];// You data table values;
                        var result = new StringBuilder();
                        foreach (DataTable table in dsExportData.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    result.Append(row[i].ToString());
                                    result.Append(i == table.Columns.Count - 1 ? "\n" : "");
                                }
                                result.AppendLine();
                            }
                        }

                        sbExportFile = result;//EncodeBilling.GettingBillingCASSRecords(ds);

                        if (sbExportFile != null)
                        {
                            #region Upload file to Blob


                            try
                            {
                                if (dsExportData != null && dsExportData.Tables.Count > 0 && dsExportData.Tables[0].Rows.Count > 0)
                                {
                                    string FileName = filenameExport.ToString() + ".txt";
                                    salesFilePath = salesFilePath + @"sales\";
                                    if (!Directory.Exists(salesFilePath))
                                    {
                                        Directory.CreateDirectory(salesFilePath);
                                    }
                                    if (File.Exists(salesFilePath + FileName))
                                        File.Delete(salesFilePath + FileName);

                                    File.WriteAllText(salesFilePath + FileName, sbExportFile.ToString());
                                    objDictionary.Add(FileName, "Success");
                                    objUploadDictionary.Add(FileName, "");
                                }
                                else
                                {
                                    objDictionary.Add(filenameExport + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                    objUploadDictionary.Add(filenameExport + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                }
                            }
                            catch (Exception ex)
                            {
                                //clsLog.WriteLogAzure(ex);
                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                objDictionary.Add("sksap.domcollection" + ExecutedOn.ToString("yyyyMMdd") + ".txt", "Failed");
                                objUploadDictionary.Add("sksap.domcollection" + ExecutedOn.ToString("yyyyMMdd") + ".txt", "Failed");
                            }

                            #endregion

                        }
                        else
                        {

                            //return;
                        }
                    }
                    else
                    {

                        //return;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                // clsLog.WriteLogAzure("------------------------------------------End DoProcess() -----------------------------------------------------------------------" + System.DateTime.Now);
                _logger.LogInformation("------------------------------------------End DoProcess() ----------------------------------------------------------------------- {0}", System.DateTime.Now);


                #region Send Email Notification
                try
                {
                    string agentEmail = string.Empty;

                    string AgentCode = string.Empty;

                    string strSubject = "Rapid Files Upload Status Dated: " + DateTime.Today.ToString("MM-dd-yyyy");

                    //string toId = GetConfigurationValue("ToEmailIDForRapid");
                    //string fromID = GetConfigurationValue("msgService_OutEmailId");

                    string toId = ConfigCache.Get("ToEmailIDForRapid");
                    string fromID = ConfigCache.Get("msgService_OutEmailId");

                    String ToEmailID = string.Empty;
                    if (!string.IsNullOrEmpty(toId))
                    {
                        ToEmailID = toId;
                    }


                    uploadfiles();
                    String body = string.Empty;

                    body += "Dear Customer," + "<br><br> Following is today's " + DateTime.Today.ToString("MM-dd-yyyy") + " RAPID file Generation Status Report. <br>";
                    body += "<br><br>";
                    body += "<table><tr><td>File Names </td><td></td><td>Generation Status</td><td>Upload Status</td></tr>";
                    var query = (from l1 in objUploadDictionary.AsEnumerable()
                                 join l2 in objDictionary.AsEnumerable()
                                 on l1.Key
                                 equals l2.Key
                                 select new { Key = l1.Key, Val = string.Format("{0},{1}", l2.Value, l1.Value) }).ToList();

                    foreach (var _temp in query)
                    {
                        try
                        {
                            string[] str = _temp.Val.Split(',');
                            body += "<tr><td>" + _temp.Key + "</td>"
                                + string.Format("<td></td><td>{0}</td><td>{1}</td></tr>", str[0], str[1]);
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }

                    //foreach (KeyValuePair<string, string> pair in objDictionary)
                    //{
                    //    body += "<tr><td>" + pair.Key + "</td><td>" + pair.Value + "</td><td></td></tr>";

                    //}
                    body += "</table>";
                    body += "<br><br>Thank You," + "<br> SmartKargo Team <br><br>";
                    body += "<br><br>Note: This is a system generated email. Please do not reply. If you were an unintended recipient kindly delete this email.";

                    await addMsgToOutBox(strSubject, body, fromID, ToEmailID, false, true, "RAPID");

                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion
            }
            catch (Exception ex)
            {

                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        public async Task UpdateRapidDetails()
        {
            try
            {
                // clsLog.WriteLogAzure("After UpdateULDStock 1111");
                _logger.LogInformation("After UpdateULDStock 1111");
                //String TimeZone = ConfigurationManager.AppSettings["UTCORLOCALTIME"].ToString();

                //string TimeZone = _appConfig.Miscellaneous.UTCORLOCALTIME;
                string TimeZone = ConfigCache.Get("UTCORLOCALTIME");

                // clsLog.WriteLogAzure("After UpdateULDStock 2222 :  " + TimeZone);
                _logger.LogInformation("After UpdateULDStock 2222 :  {0}", TimeZone);
                DateTime ExecutedOn = DateTime.Now;
                DateTime FromDate = DateTime.Now;
                DateTime ToDate = DateTime.Now;

                if (TimeZone.ToUpper() == "UTC")
                {
                    // UTC Time 
                    ExecutedOn = DateTime.Now.AddDays(0);
                    FromDate = DateTime.Now.AddDays(0);
                    ToDate = DateTime.Now.AddDays(0);
                }
                else
                {
                    ExecutedOn = DateTime.Now.AddDays(0);
                    FromDate = ExecutedOn.AddDays(0);
                    ToDate = ExecutedOn.AddDays(0);
                    //ExecutedOn = DateTime.Now.AddDays(0);
                    //FromDate = ExecutedOn.AddDays(0);
                    //ToDate = ExecutedOn.AddDays(0);


                }
                // clsLog.WriteLogAzure("------------------------- doProcess() Started ---------------------------------------------------------------------");
                _logger.LogInformation("------------------------- doProcess() Started ---------------------------------------------------------------------");
                objDictionary = new Dictionary<string, string>();
                objUploadDictionary = new Dictionary<string, string>();

                //string FilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string flnFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string salesFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string CTMFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();
                //string CCAPXFilePath = ConfigurationManager.AppSettings["XMLFilePath"].ToString();

                string FilePath = _appConfig.Miscellaneous.XMLFilePath;
                string flnFilePath = _appConfig.Miscellaneous.XMLFilePath;
                string salesFilePath = _appConfig.Miscellaneous.XMLFilePath;
                string CTMFilePath = _appConfig.Miscellaneous.XMLFilePath;
                string CCAPXFilePath = _appConfig.Miscellaneous.XMLFilePath;

                //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();

                string XML = string.Empty;

                //balRapidInterface objBAL = new balRapidInterface();

                // clsLog.WriteLogAzure("----------------------------------------------------------------------------------------------------------------------");
                // clsLog.WriteLogAzure("Schedular run on ::" + System.DateTime.Now);
                _logger.LogInformation("----------------------------------------------------------------------------------------------------------------------");
                _logger.LogInformation("Schedular run on ::" + System.DateTime.Now);
                objDictionary.Add("File Names ", "Status");
                #region "AWB RAPID File"

                try
                {
                    // string filenameCTM = string.Empty;
                    DataSet? dsRpdIntrfcAWBFileName = new DataSet();
                    DataSet? dsAWBData = new DataSet();

                    StringBuilder sbAWB = new StringBuilder();
                    string filenameAWBData = string.Empty;

                    dsRpdIntrfcAWBFileName = await _balRapidInterface.InsertRapidInterfaceData(Convert.ToDateTime(ExecutedOn)
                                                                , "SKAdmin"
                                                                 , FromDate
                                                                 , ToDate
                                                                , "", filenameAWBData);
                    // clsLog.WriteLogAzure("--Data Inserted -" + DateTime.Now.ToString() + ":" + FromDate + ":" + ToDate + ":");
                    _logger.LogInformation("--Data Inserted - {0} : {1} : {2}", DateTime.Now.ToString(), FromDate, ToDate);
                    if (dsRpdIntrfcAWBFileName != null) //dsFileName != null
                    {
                        // clsLog.WriteLogAzure("-AWB File write Processing Started -");
                        _logger.LogInformation("-AWB File write Processing Started -");
                        if (dsRpdIntrfcAWBFileName.Tables.Count > 0)
                        {
                            {
                                DataRow drAWBFileName = dsRpdIntrfcAWBFileName.Tables[0].Rows[0];
                                if (dsRpdIntrfcAWBFileName.Tables[0].Columns.Contains("StatusMsg"))
                                {
                                    if (Convert.ToString(dsRpdIntrfcAWBFileName.Tables[0].Rows[0]["StatusMsg"]).ToUpper() == "FILESALREADYGENERATED")
                                    {
                                        // clsLog.WriteLogAzure("- FILESALREADYGENERATED ON :- " + ExecutedOn.ToString("ddMMMyymmss"));
                                        _logger.LogInformation("- FILESALREADYGENERATED ON :- {0}", ExecutedOn.ToString("ddMMMyymmss"));
                                        return;
                                    }
                                }

                                if (drAWBFileName != null)
                                {
                                    if (!string.IsNullOrEmpty(drAWBFileName["FileName"].ToString()))
                                    {
                                        filenameAWBData = drAWBFileName["FileName"].ToString();
                                        dsAWBData = await _balRapidInterface.GetRapidInterfaceData(filenameAWBData);
                                        if (dsAWBData != null && dsAWBData.Tables.Count > 0)
                                        {
                                            if (Convert.ToInt64(dsAWBData.Tables[5].Rows[0]["TotalAWBs"]).ToString() == "0")
                                            {
                                                objDictionary.Add(filenameAWBData.ToString() + ".txt", "Success");
                                                objUploadDictionary.Add(filenameAWBData.ToString() + ".txt", "" + "0");
                                            }
                                            else
                                            {
                                                var result = new StringBuilder();
                                                //Header Row
                                                foreach (DataRow row in dsAWBData.Tables[0].Rows) // Select each Row
                                                {
                                                    for (int i = 0; i < dsAWBData.Tables[0].Columns.Count; i++)// Write Each coloumn in a Row
                                                    {
                                                        result.Append(row[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[0].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();
                                                }
                                                //[START]AWb, RateLine and OT
                                                foreach (DataRow row in dsAWBData.Tables[1].Rows) // Select each AWB Row
                                                {
                                                    //AWB
                                                    for (int i = 0; i < dsAWBData.Tables[1].Columns.Count; i++)// Write Each coloumn in a AWB Row
                                                    {
                                                        result.Append(row[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[1].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();

                                                    //RateLine
                                                    DataRow[] drRateline = dsAWBData.Tables[2].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString() + " AND IssuingCarrierCode=" + "'" + row["IssuingCarrierCode"].ToString() + "'");
                                                    foreach (DataRow rowRT in drRateline) // Select each RT Row
                                                    {
                                                        for (int i = 0; i < dsAWBData.Tables[2].Columns.Count; i++)// Write Each coloumn in a RT Row
                                                        {
                                                            result.Append(rowRT[i].ToString());
                                                            result.Append(i == dsAWBData.Tables[2].Columns.Count - 1 ? "\n" : "");
                                                        }
                                                        result.AppendLine();
                                                    }

                                                    //Other Charges
                                                    DataRow[] drOtherCharges = dsAWBData.Tables[3].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString() + " AND IssuingCarrierCode=" + "'" + row["IssuingCarrierCode"].ToString() + "'");
                                                    foreach (DataRow rowOT in drOtherCharges) // Select each OT Row
                                                    {
                                                        for (int i = 0; i < dsAWBData.Tables[3].Columns.Count; i++)// Write Each coloumn in a OT Row
                                                        {
                                                            result.Append(rowOT[i].ToString());
                                                            result.Append(i == dsAWBData.Tables[3].Columns.Count - 1 ? "\n" : "");
                                                        }
                                                        result.AppendLine();
                                                    }

                                                    //Other Charges
                                                    DataRow[] drTaxDetails = dsAWBData.Tables[4].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString() + " AND IssuingCarrierCode=" + "'" + row["IssuingCarrierCode"].ToString() + "'");
                                                    foreach (DataRow rowOT in drTaxDetails) // Select each OT Row
                                                    {
                                                        for (int i = 0; i < dsAWBData.Tables[4].Columns.Count; i++)// Write Each coloumn in a TX Row
                                                        {
                                                            result.Append(rowOT[i].ToString());
                                                            result.Append(i == dsAWBData.Tables[4].Columns.Count - 1 ? "\n" : "");
                                                        }
                                                        result.AppendLine();
                                                    }
                                                }

                                                //[END]AWb, RateLine and OT
                                                //Trailer Row
                                                foreach (DataRow row in dsAWBData.Tables[5].Rows) // Select each Row
                                                {
                                                    for (int i = 0; i < dsAWBData.Tables[5].Columns.Count; i++)// Write Each coloumn in a Row
                                                    {
                                                        result.Append(row[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[5].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();
                                                }


                                                sbAWB = result;//EncodeBilling.GettingBillingCASSRecords(dsAWBData);
                                                if (sbAWB != null)
                                                {
                                                    #region Upload file to Blob
                                                    try
                                                    {
                                                        if (dsAWBData != null && dsAWBData.Tables.Count > 0 && dsAWBData.Tables[0].Rows.Count > 0)
                                                        {
                                                            string FileName = filenameAWBData.ToString() + ".txt";
                                                            FilePath = FilePath + @"awb\";
                                                            if (!Directory.Exists(FilePath))
                                                            {
                                                                Directory.CreateDirectory(FilePath);
                                                            }
                                                            if (File.Exists(FilePath + FileName))
                                                                File.Delete(FilePath + FileName);

                                                            File.WriteAllText(FilePath + FileName, sbAWB.ToString());
                                                            await addMsgToOutBox(FileName, sbAWB.ToString(), "", "SFTP", false, true, "RAPID");
                                                            objDictionary.Add(FileName, "Success");
                                                            objUploadDictionary.Add(FileName, "" + Convert.ToInt64(dsAWBData.Tables[5].Rows[0]["TotalAWBs"]).ToString());
                                                            // clsLog.WriteLogAzure("--AWB file write Process Completed -");
                                                            _logger.LogInformation("--AWB file write Process Completed -");
                                                        }
                                                        else
                                                        {
                                                            objDictionary.Add(filenameAWBData + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                                            objUploadDictionary.Add(filenameAWBData + ExecutedOn.ToString("yyyyMMdd") + ".txt", "No Data");
                                                        }
                                                    }
                                                    catch (Exception exc)
                                                    {
                                                        objDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                                        objUploadDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                                        // clsLog.WriteLogAzure(exc);
                                                        _logger.LogError(exc, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                    }

                                                    #endregion

                                                }
                                                else
                                                {
                                                    return;
                                                }
                                            }

                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }//!string.IsNullOrEmpty(dr["FileName"].ToString())

                                }//if (dr != null)

                            }// [END] dsFileName.Tables[0] != null
                        }
                        else
                        {
                            objDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                            objUploadDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                            // clsLog.WriteLogAzure("if (dsRpdIntrfcAWBFileName");
                            _logger.LogInformation("if (dsRpdIntrfcAWBFileName");
                        }

                    } //[END]//if dsFileName != null
                    else
                    {
                        objDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                        objUploadDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");

                        // clsLog.WriteLogAzure("--Else Run -");
                        _logger.LogInformation("--Else Run -");
                        //lblStatus.Text = "Rapid AWB generation failed!";
                        //lblStatus.ForeColor = Color.Red;
                        //return;

                    }


                }
                catch (Exception ex)
                {
                    objDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                    objUploadDictionary.Add("RAPID_AWB" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");

                    // clsLog.WriteLogAzure(ex.Message);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                #region "Flown Rapid File"
                try
                {

                    DataSet? dsFlownFileName = new DataSet();
                    DataSet? dsFlownData = new DataSet();
                    StringBuilder sbFlown = new StringBuilder();
                    string filenameFlown = "";

                    #region INSERT Flown DATA
                    // clsLog.WriteLogAzure("-Flown File write Processing Started -");
                    _logger.LogInformation("-Flown File write Processing Started -");
                    dsFlownFileName = await _balRapidInterface.InsertRapidFlownTransaction(Convert.ToDateTime(ExecutedOn)
                                                                , "SKAdmin"
                                                                 , FromDate
                                                                 , ToDate
                                                              );



                    if (dsFlownFileName != null)
                    {
                        if (dsFlownFileName.Tables.Count > 0)
                        {
                            if (dsFlownFileName.Tables[0] != null)
                            {
                                DataRow dr = dsFlownFileName.Tables[0].Rows[0];

                                if (dr != null)
                                {
                                    if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
                                    {
                                        filenameFlown = dr["FileName"].ToString();
                                    }
                                    else
                                    {

                                        return;
                                    }
                                }//if (dr != null)
                            }// [END] dsFileName.Tables[0] != null
                        }
                    } //[END]//if dsFileName != null
                    else
                    {

                        objDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                        objUploadDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                        //return;

                    }
                    #endregion

                    #region GET Flown DATA
                    if (filenameFlown != null && filenameFlown != "")
                    {
                        dsFlownData = await _balRapidInterface.GetRapidFlownTransaction(filenameFlown);
                        if (dsFlownData != null && dsFlownData.Tables.Count > 0)
                        {
                            //DataTable table = ds.Tables[0];// You data table values;
                            var result = new StringBuilder();
                            foreach (DataTable table in dsFlownData.Tables)
                            {
                                foreach (DataRow row in table.Rows)
                                {
                                    for (int i = 0; i < table.Columns.Count; i++)
                                    {
                                        result.Append(row[i].ToString());
                                        result.Append(i == table.Columns.Count - 1 ? "\n" : "");
                                    }
                                    result.AppendLine();
                                }
                            }
                            sbFlown = result;//EncodeBilling.GettingBillingCASSRecords(ds);
                            if (sbFlown != null)
                            {
                                #region Upload file to Blob
                                try
                                {
                                    if (dsFlownData != null && dsFlownData.Tables.Count > 0 && dsFlownData.Tables[0].Rows.Count > 0)
                                    {
                                        string FileName = filenameFlown.ToString() + ".txt";
                                        flnFilePath = flnFilePath + @"fln\";
                                        if (!Directory.Exists(flnFilePath))
                                        {
                                            Directory.CreateDirectory(flnFilePath);
                                        }
                                        if (File.Exists(flnFilePath + FileName))
                                            File.Delete(flnFilePath + FileName);

                                        File.WriteAllText(flnFilePath + FileName, sbFlown.ToString());
                                        await addMsgToOutBox(FileName, sbFlown.ToString(), "", "SFTP", false, true, "RAPID");
                                        objDictionary.Add(FileName, "Success");
                                        objUploadDictionary.Add(FileName, "" + Convert.ToInt64(dsFlownData.Tables[2].Rows[0]["TotalAWBs"]).ToString());
                                        // clsLog.WriteLogAzure("--Flown file write Process Completed -");
                                        _logger.LogInformation("--Flown file write Process Completed -");
                                    }
                                    else
                                    {
                                        objDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                        objUploadDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                    }
                                }
                                catch (Exception exc)
                                {


                                    objDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                    objUploadDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                    // clsLog.WriteLogAzure(exc);
                                    _logger.LogError(exc, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }

                                #endregion
                            }
                        }
                        else
                        {

                            //return;
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    objDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                    objUploadDictionary.Add("RAPID_FLN" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");

                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                #region "CTM Rapid Interface"
                DataSet? dsCTMData = new DataSet();
                DataSet? dsCTMFileName = new DataSet();

                StringBuilder sbCTM = new StringBuilder();
                string filenameCTM = "";

                try
                {

                    // clsLog.WriteLogAzure("--CTM file write Process Started -");
                    _logger.LogInformation("--CTM file write Process Started -");
                    #region Insert CTM DATA
                    dsCTMFileName = await _balRapidInterface.InsertRapidCTMTransaction(Convert.ToDateTime(ExecutedOn)
                                                                , "SKAdmin"
                                                                 , FromDate
                                                                 , ToDate
                                                              );



                    if (dsCTMFileName != null) //dsFileName != null
                    {
                        if (dsCTMFileName.Tables.Count > 0)
                        {
                            if (dsCTMFileName.Tables[0] != null)
                            {
                                DataRow dr = dsCTMFileName.Tables[0].Rows[0];

                                if (dr != null)
                                {
                                    if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
                                    {

                                        filenameCTM = dr["FileName"].ToString();

                                    }//!string.IsNullOrEmpty(dr["FileName"].ToString())
                                    else
                                    {
                                        objDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                        objUploadDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                        return;
                                    }
                                }//if (dr != null)
                            }// [END] dsFileName.Tables[0] != null
                        }
                    } //[END]//if dsFileName != null
                    else
                    {
                        objDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                        objUploadDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");

                        return;

                    }
                    #endregion

                    #region GET CTM DATA
                    dsCTMData = await _balRapidInterface.GetRapidCTMTransaction(filenameCTM);
                    if (dsCTMData != null && dsCTMData.Tables.Count > 0)
                    {
                        var result = new StringBuilder();
                        foreach (DataTable table in dsCTMData.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    result.Append(row[i].ToString());
                                    result.Append(i == table.Columns.Count - 1 ? "\n" : "");
                                }
                                result.AppendLine();
                            }
                        }

                        sbCTM = result;//EncodeBilling.GettingBillingCASSRecords(ds);

                        if (sbCTM != null)
                        {
                            #region Upload file to Blob
                            try
                            {
                                if (dsCTMData != null && dsCTMData.Tables.Count > 0 && dsCTMData.Tables[0].Rows.Count > 0)
                                {
                                    string FileName = filenameCTM.ToString() + ".txt";
                                    CTMFilePath = CTMFilePath + @"CTM\";
                                    if (!Directory.Exists(CTMFilePath))
                                    {
                                        Directory.CreateDirectory(CTMFilePath);
                                    }
                                    if (File.Exists(CTMFilePath + FileName))
                                        File.Delete(CTMFilePath + FileName);

                                    File.WriteAllText(CTMFilePath + FileName, sbCTM.ToString());
                                    await addMsgToOutBox(FileName, sbCTM.ToString(), "", "SFTP", false, true, "RAPID");
                                    objDictionary.Add(FileName, "Success");
                                    objUploadDictionary.Add(FileName, "" + Convert.ToInt64(dsCTMData.Tables[2].Rows[0]["TotalAWBs"]).ToString());
                                    // clsLog.WriteLogAzure("--CTM file write Process Completed -");
                                    _logger.LogInformation("--CTM file write Process Completed -");
                                }
                                else
                                {
                                    objDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                    objUploadDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                }
                            }
                            catch (Exception exc)
                            {
                                objDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                objUploadDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                // clsLog.WriteLogAzure(exc);
                                _logger.LogError(exc, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            }

                            #endregion
                        }


                    }
                    else
                    {
                        objDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                        objUploadDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                        return;
                    }




                    #endregion

                }
                catch (Exception ex)
                {
                    objDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                    objUploadDictionary.Add("RAPID_CTM" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                finally
                {
                    if (dsCTMData != null)
                        dsCTMData.Dispose();

                }
                #endregion

                #region "CCA PX Rapid Interface"
                DataSet? dsCCAPXData = new DataSet();
                DataSet? dsCCAPXFileName = new DataSet();

                StringBuilder sbCCAPX = new StringBuilder();
                string filenameCCAPX = "";

                try
                {


                    #region Insert CTM DATA
                    // clsLog.WriteLogAzure("--CCA For PX file write Process Started -");
                    _logger.LogInformation("--CCA For PX file write Process Started -");
                    dsCCAPXFileName = await _balRapidInterface.InsertRapidCCAPXTransaction(Convert.ToDateTime(ExecutedOn)
                                                                , "SKAdmin"
                                                                 , FromDate
                                                                 , ToDate
                                                              );



                    if (dsCCAPXFileName != null) //dsFileName != null
                    {
                        if (dsCCAPXFileName.Tables.Count > 0)
                        {
                            if (dsCCAPXFileName.Tables[0] != null)
                            {
                                DataRow dr = dsCCAPXFileName.Tables[0].Rows[0];

                                if (dr != null)
                                {
                                    if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
                                    {

                                        filenameCCAPX = dr["FileName"].ToString();

                                    }//!string.IsNullOrEmpty(dr["FileName"].ToString())
                                    else
                                    {
                                        objDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                        objUploadDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                        return;
                                    }
                                }//if (dr != null)
                            }// [END] dsFileName.Tables[0] != null
                        }
                    } //[END]//if dsFileName != null
                    else
                    {
                        objDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                        objUploadDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");

                        return;

                    }
                    #endregion

                    #region GET CCA PX DATA
                    dsCCAPXData = await _balRapidInterface.GetRapidCCAPXTransaction(filenameCCAPX);
                    if (dsCCAPXData != null && dsCCAPXData.Tables.Count > 0)
                    {
                        var result = new StringBuilder();
                        foreach (DataTable table in dsCCAPXData.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    result.Append(row[i].ToString());
                                    result.Append(i == table.Columns.Count - 1 ? "\n" : "");
                                }
                                result.AppendLine();
                            }
                        }

                        sbCCAPX = result;//EncodeBilling.GettingBillingCASSRecords(ds);

                        if (sbCCAPX != null)
                        {
                            #region Upload file to Blob
                            try
                            {
                                if (dsCCAPXData != null && dsCCAPXData.Tables.Count > 0 && dsCCAPXData.Tables[0].Rows.Count > 0)
                                {
                                    string FileName = filenameCCAPX.ToString() + ".txt";
                                    CCAPXFilePath = CCAPXFilePath + @"CCAPX\";
                                    if (!Directory.Exists(CCAPXFilePath))
                                    {
                                        Directory.CreateDirectory(CCAPXFilePath);
                                    }
                                    if (File.Exists(CCAPXFilePath + FileName))
                                        File.Delete(CCAPXFilePath + FileName);

                                    File.WriteAllText(CCAPXFilePath + FileName, sbCCAPX.ToString());
                                    await addMsgToOutBox(FileName, sbCCAPX.ToString(), "", "SFTP", false, true, "RAPID");
                                    objDictionary.Add(FileName, "Success");
                                    objUploadDictionary.Add(FileName, "" + Convert.ToInt64(dsCCAPXData.Tables[5].Rows[0]["TotalAWBs"]).ToString());
                                    // clsLog.WriteLogAzure("--CCA for PX file write Process Completed -");
                                    _logger.LogInformation("--CCA for PX file write Process Completed -");
                                }
                                else
                                {
                                    objDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                    objUploadDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                }
                            }
                            catch (Exception exec)
                            {
                                objDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                                objUploadDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                                // clsLog.WriteLogAzure(exec);
                                _logger.LogError(exec, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            }

                            #endregion
                        }


                    }
                    else
                    {
                        objDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                        objUploadDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");

                        return;
                    }


                    #endregion

                }
                catch (Exception ex)
                {
                    objDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "FAILED");
                    objUploadDictionary.Add("RAPID_CCA" + ExecutedOn.ToString("ddMMMyymmss") + ".txt", "0");
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                finally
                {
                    if (dsCCAPXData != null)
                        dsCCAPXData.Dispose();

                }
                #endregion

                // clsLog.WriteLogAzure("------------------------------------------End DoProcess() -----------------------------------------------------------------------" + System.DateTime.Now);
                _logger.LogInformation("------------------------------------------End DoProcess() ----------------------------------------------------------------------- {0}", System.DateTime.Now);
                await SaveRapidStatus("SaveRapidLog");

                //#region Send Email Notification
                //try
                //{
                //    string agentEmail = string.Empty;

                //    string AgentCode = string.Empty;

                //    string strSubject = "Rapid Files Upload Status Dated: " + DateTime.Today.ToString("MM-dd-yyyy");

                //    string toId = GetConfigurationValue("ToEmailIDForRapid");
                //    string fromID = GetConfigurationValue("msgService_OutEmailId");

                //    String ToEmailID = string.Empty;
                //    if (!string.IsNullOrEmpty(toId))
                //    {
                //        ToEmailID = toId;
                //    }


                //    // uploadfiles(); ---------remove after testing
                //    String body = string.Empty;

                //    body += "Dear Customer," + "<br><br> Following is today's " + DateTime.Today.ToString("MM-dd-yyyy") + " RAPID file Generation Status Report. <br>";
                //    body += "<br><br>";
                //    body += "<table><tr><td><b>File Names</b></td><td></td><td><b>Send Status</b></td><td><b>Count of AWB</b></td></tr>";
                //    var query = (from l1 in objUploadDictionary.AsEnumerable()
                //                 join l2 in objDictionary.AsEnumerable()
                //                 on l1.Key
                //                 equals l2.Key
                //                 select new { Key = l1.Key, Val = string.Format("{0},{1}", l2.Value, l1.Value) }).ToList();

                //    foreach (var _temp in query)
                //    {
                //        try
                //        {
                //            string[] str = _temp.Val.Split(',');
                //            body += "<tr><td>" + _temp.Key + "</td>"
                //                + string.Format("<td></td><td>{0}</td><td>{1}</td></tr>", str[0], str[1]);
                //        }
                //        catch (Exception EX)
                //        { clsLog.WriteLogAzure(EX); }
                //    }

                //    //foreach (KeyValuePair<string, string> pair in objDictionary)
                //    //{
                //    //    body += "<tr><td>" + pair.Key + "</td><td>" + pair.Value + "</td><td></td></tr>";

                //    //}
                //    body += "</table>";
                //    body += "<br><br>Thank You," + "<br> SmartKargo Team <br><br>";
                //    body += "<br><br>Note: This is a system generated email. Please do not reply. If you were an unintended recipient kindly delete this email.";

                //    addMsgToOutBox(strSubject, body, fromID, ToEmailID, false, true, "RAPID");

                //}
                //catch (Exception ex)
                //{
                //    //clsLog.WriteLogAzure(ex);
                //}
                //#endregion
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex.Message);
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

        }

        /*Not in use*/
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
        //        //clsLog.WriteLogAzure(ex);
        //        return "";

        //    }
        //    finally
        //    {
        //        objDA = null;
        //        objDs = null;
        //    }
        //}

        public void uploadfiles()
        {
            TransferOperationResult transferResult = null;
            try
            {

                //@ConfigurationManager.AppSettings["XMLFilePath"]
                var xMLFilePath = _appConfig.Miscellaneous.XMLFilePath;

                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    //HostName = ConfigurationManager.AppSettings["HostName"].ToString(),
                    //UserName = ConfigurationManager.AppSettings["UserName"].ToString(),
                    //Password = ConfigurationManager.AppSettings["Password"].ToString(),
                    //SshHostKeyFingerprint = ConfigurationManager.AppSettings["SshHostKeyFingerprint"].ToString()

                    HostName = _appConfig.Sftp.SftpHostName,
                    UserName = _appConfig.Sftp.SftpUserName,
                    Password = _appConfig.Sftp.SftpPassword,
                    SshHostKeyFingerprint = _appConfig.Sftp.SftpSshHostKeyFingerprint
                };
                using (Session session = new Session())
                {
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
                    //transferResult = session.PutFiles(@ConfigurationManager.AppSettings["XMLFilePath"].ToString() + "*", @ConfigurationManager.AppSettings["SFTPFolderPath"].ToString(), true, transferOptions);
                    transferResult = session.PutFiles(@xMLFilePath + "*", @xMLFilePath, true, transferOptions);

                    if (transferResult.IsSuccess)
                    {
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {
                            try
                            {
                                bool value = objUploadDictionary.ContainsKey(Path.GetFileName(transfer.FileName));
                                if (value)
                                    objUploadDictionary[Path.GetFileName(transfer.FileName)] = "Success";

                                // clsLog.WriteLogAzure("Rapid File trasferd::" + transfer.FileName + "\r\n");
                                _logger.LogInformation("Rapid File trasferd:: {0} \r\n", transfer.FileName);
                            }
                            catch (Exception ex)
                            { //clsLog.WriteLogAzure(ex);
                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            }
                        }
                    }
                    else
                    {
                        // clsLog.WriteLogAzure("Rapid File trasferd::" + "failed \r\n");
                        _logger.LogWarning("Rapid File trasferd:: failed \r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Exception::" + ex.Message);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    try
                    {
                        objUploadDictionary[Path.GetFileName(transfer.FileName)] = "failed";

                        // clsLog.WriteLogAzure("Rapid File trasferd::" + transfer.FileName + "failed \r\n");
                        _logger.LogWarning("Rapid File trasferd:: {0} failed \r\n", transfer.FileName);
                    }
                    catch (Exception ex1)
                    {
                        // clsLog.WriteLogAzure("Exception::" + ex1.Message);
                        _logger.LogError(ex1, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
            }
        }

        /*Not in use*/
        //public static bool UploadBlob(Stream stream, string fileName)
        //{
        //    try
        //    {
        //        string containerName = "blobstorage";

        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());// "NUro8/C7+kMqtwOwLbe6agUvA83s+8xSTBqrkMwSjPP6MAxVkdtsLDGjyfyEqQIPv6JHEEf5F5s4a+DFPsSQfg==");
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
        //        blobContainer.CreateIfNotExist();
        //        CloudBlob blob = blobContainer.GetBlobReference(fileName);
        //        blob.Properties.ContentType = "";
        //        blob.UploadFromStream(stream);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        return false;
        //    }
        //}
        /*Not in use*/
        //private static string getStorageKey()
        //{
        //    try
        //    {
        //        if (String.IsNullOrEmpty(BlobKey))
        //        {

        //            BlobKey = GetMasterConfiguration("BlobStorageKey");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //    return BlobKey;
        //}

        /*Not in use*/
        //private static string getStorageName()
        //{
        //    try
        //    {
        //        if (String.IsNullOrEmpty(BlobName))
        //        {

        //            BlobName = GetMasterConfiguration("BlobStorageName");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //    return BlobName;
        //}

        /*Not in use*/
        //public static string GetMasterConfiguration(string Parameter)
        //{
        //    string ParameterValue = string.Empty;

        //    balRapidInterface da = new balRapidInterface();
        //    string[] QName = new string[] { "PType" };
        //    object[] QValues = new object[] { Parameter };
        //    SqlDbType[] QType = new SqlDbType[] { SqlDbType.VarChar };
        //    ParameterValue = da.GetStringByProcedure("spGetSystemParameter", QName, QValues, QType);
        //    if (ParameterValue == null)
        //        ParameterValue = "";
        //    da = null;
        //    QName = null;
        //    QValues = null;
        //    QType = null;

        //    return ParameterValue;
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

                SqlParameter[] prm = [
                    new SqlParameter("@Subject",subject)
                    ,new SqlParameter("@Body",Msg)
                    ,new SqlParameter("@FromEmailID",FromEmailID)
                    ,new SqlParameter("@ToEmailID",ToEmailID)
                    ,new SqlParameter("@Type",type)
                    ,new SqlParameter("@IsHTML",isHTML)
                ];

                return await _readWriteDao.ExecuteNonQueryAsync("spInsertMsgToOutbox", prm);

                //cmd.Parameters.AddRange(prm);
                //cmd.ExecuteNonQuery();
                //    string procedure = "spInsertMsgToOutbox";

                //    string CarrierCode = string.Empty;


                //    string[] paramname = new string[] { "Subject",
                //                                    "Body",
                //                                    "FromEmailID",
                //                                    "ToEmailID",
                //                                    "IsHTML",
                //                                    "isInternal",
                //                                    "Type",
                //                                    "CreatedOn",
                //"CarrierCode"};

                //    object[] paramvalue = new object[] {subject,
                //                                    Msg,
                //                                    FromEmailID,
                //                                    ToEmailID,
                //                                    isHTML,
                //                                    isInternal,
                //                                    type,
                //                                    System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //CarrierCode};

                //    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.Bit,
                //                                         SqlDbType.Bit,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.DateTime,
                //SqlDbType.VarChar};

                //    flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        public async Task SaveRapidStatus(string SaveSendFlag)
        {
            SqlParameter[] sqlParameters = new SqlParameter[] { };
            //SQLServer sqlServer = new SQLServer();
            DataSet? dsReturn = new DataSet();

            try
            {
                DataTable dtRapidStatus = new DataTable();
                dtRapidStatus.Columns.AddRange(new DataColumn[6] {
                            new DataColumn("Rno", typeof(int)),
                            new DataColumn("FileName", typeof(string)),
                            new DataColumn("FileSendStatus", typeof(string)),
                            new DataColumn("SFTPSendStatus", typeof(string)),
                            new DataColumn("Count", typeof(string)),
                            new DataColumn("CreatedDate", typeof(DateTime)) });

                if (SaveSendFlag == "SaveRapidLog")
                {
                    var query = (from l1 in objUploadDictionary.AsEnumerable()
                                 join l2 in objDictionary.AsEnumerable()
                                 on l1.Key
                                 equals l2.Key
                                 select new { Key = l1.Key, Val = string.Format("{0},{1}", l2.Value, l1.Value) }).ToList();

                    int Rno = 1;
                    foreach (var _temp in query)
                    {
                        string[] str = _temp.Val.Split(',');
                        dtRapidStatus.Rows.Add(Rno, _temp.Key, str[0], str[0], str[1], System.DateTime.Now);
                        Rno++;
                    }
                    sqlParameters = new SqlParameter[] { new SqlParameter("@RapidFileUploadLog", dtRapidStatus), new SqlParameter("@Flag", 1) };

                    //dsReturn = sqlServer.SelectRecords("UspSendAleart_SFTP_RAPID", sqlParameters);

                    dsReturn = await _readWriteDao.SelectRecords("UspSendAleart_SFTP_RAPID", sqlParameters);
                    if (dsReturn != null)
                    {
                        // clsLog.WriteLogAzure("Sucess-SaveRapidLog_Ds ::" + System.DateTime.Now);
                        _logger.LogInformation("Sucess-SaveRapidLog_Ds :: {0}", System.DateTime.Now);

                        if (dsReturn.Tables.Count > 0 && dsReturn.Tables[0].Rows.Count > 0)
                        {
                            // clsLog.WriteLogAzure("Sucess-SaveRapidLog_Dt ::" + System.DateTime.Now);
                            _logger.LogInformation("Sucess-SaveRapidLog_Dt :: {0}", System.DateTime.Now);
                        }
                        else
                        {
                            // clsLog.WriteLogAzure("Failed-SaveRapidLog_Dt ::" + System.DateTime.Now);
                            _logger.LogWarning("Failed-SaveRapidLog_Dt :: {0}", System.DateTime.Now);
                        }
                    }
                    else
                    {
                        // clsLog.WriteLogAzure("Failed-SaveRapidLog_Ds ::" + System.DateTime.Now);
                        _logger.LogWarning("Failed-SaveRapidLog_Ds :: {0}", System.DateTime.Now);
                    }
                }

                if (SaveSendFlag == "SendRapidAleart")
                {
                    sqlParameters = new SqlParameter[] { new SqlParameter("@RapidFileUploadLog", dtRapidStatus), new SqlParameter("@Flag", 2) };

                    //dsReturn = sqlServer.SelectRecords("UspSendAleart_SFTP_RAPID", sqlParameters);
                    dsReturn = await _readWriteDao.SelectRecords("UspSendAleart_SFTP_RAPID", sqlParameters);
                    if (dsReturn != null)
                    {
                        // clsLog.WriteLogAzure("Sucess-SendRapidAleart_Ds ::" + System.DateTime.Now);
                        _logger.LogInformation("Sucess-SendRapidAleart_Ds :: {0}", System.DateTime.Now);
                        if (dsReturn.Tables.Count > 0 && dsReturn.Tables[0].Rows.Count > 0)
                        {
                            if (Convert.ToString(dsReturn.Tables[0].Rows[0][0]).ToUpper() != "FILESALREADYGENERATED")
                            {
                                // clsLog.WriteLogAzure("Sucess-FILESGENERATED ::" + System.DateTime.Now);
                                _logger.LogInformation("Sucess-FILESGENERATED :: {0}", System.DateTime.Now);

                                string agentEmail = string.Empty; string AgentCode = string.Empty; String body = string.Empty; String ToEmailID = string.Empty;
                                DataTable dt = new DataTable();
                                string strSubject = "Rapid Files Upload Status Dated: " + DateTime.Today.ToString("MM-dd-yyyy");

                                //string toId = GetConfigurationValue("ToEmailIDForRapid");
                                //string fromID = GetConfigurationValue("msgService_OutEmailId");

                                string toId = ConfigCache.Get("ToEmailIDForRapid");
                                string fromID = ConfigCache.Get("msgService_OutEmailId");

                                if (!string.IsNullOrEmpty(toId))
                                {
                                    ToEmailID = toId;
                                }
                                dt = dsReturn.Tables[0];

                                body += "Dear Customer," + "<br><br> Following is today's " + DateTime.Today.ToString("MM-dd-yyyy") + " RAPID file Generation Status Report. <br>";
                                body += "<br><br>";
                                body += "<table><tr style='font-weight: bold;'><td style='border:1px solid black;'>FILE NAMES </td><td style='border:1px solid black;'>GENERATION STATUS</td><td style='border:1px solid black;'>UPLOAD STATUS</td><td style='border:1px solid black;'>COUNT OF AWB</td></tr>";

                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    body += "<tr><td style='border:1px solid black;'>" + dt.Rows[i]["FileName"].ToString() + " </td><td style='border:1px solid black;'>" + dt.Rows[i]["FileSendStatus"].ToString() + "</td><td style='border:1px solid black;'>" + dt.Rows[i]["SFTPSendStatus"].ToString() + "</td><td style='border:1px solid black;'>" + dt.Rows[i]["Count"].ToString() + "</td></tr>";
                                }

                                body += "</table>";
                                body += "<br><br>Thank You," + "<br> SmartKargo Team <br><br>";
                                body += "<br><br>Note: This is a system generated email. Please do not reply. If you were an unintended recipient kindly delete this email.";

                                await addMsgToOutBox(strSubject, body, fromID, ToEmailID, false, true, "RAPID");
                            }
                        }
                        else
                        {
                            // clsLog.WriteLogAzure("Failed-SendRapidAleart_Ds ::" + System.DateTime.Now);
                            _logger.LogWarning("Failed-SendRapidAleart_Ds :: {0}", System.DateTime.Now);
                        }
                    }
                    else
                    {
                        // clsLog.WriteLogAzure("Failed-SendRapidAleart_Ds ::" + System.DateTime.Now);
                        _logger.LogWarning("Failed-SendRapidAleart_Ds :: {0}", System.DateTime.Now);
                    }
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Failed SaveRapidSendStatus" + ex.Message + System.DateTime.Now);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name} {System.DateTime.Now}");
            }
        }
    }
}
