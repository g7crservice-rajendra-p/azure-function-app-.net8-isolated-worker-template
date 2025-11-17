using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Globalization;

namespace QidWorkerRole.UploadMasters.DCM
{
    public class UploadDCM
    {
        //UploadMasterCommon _uploadMasterCommon = new UploadMasterCommon();


        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadDCM> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadDCM(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadDCM> logger,
            UploadMasterCommon uploadMasterCommon
         )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;
        }
        #endregion
        public async Task<bool> DCMUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.RouteControls);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "dcm", out uploadFilePath))
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                return false;
            }
        }
        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableDCMExcelData = new DataTable("dataTableDCMExcelData");

            bool isBinaryReader = false;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".XLS") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTableDCMExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommon.RemoveEmptyRows(dataTableDCMExcelData);

                foreach (DataColumn dataColumn in dataTableDCMExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableDCMExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating RouteControlsMasterType DataTable

                DataTable dataTableDCMType = new DataTable("dataTableDCMExcelData");
                dataTableDCMType.Columns.Add("DCMIndex", System.Type.GetType("System.Int32"));
                dataTableDCMType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                dataTableDCMType.Columns.Add("AgentCode", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Credit", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Debit", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Flt No", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Flt Date", System.Type.GetType("System.DateTime"));
                dataTableDCMType.Columns.Add("Applied Date", System.Type.GetType("System.DateTime"));
                dataTableDCMType.Columns.Add("Origin", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Destination", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Commodity Code", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Description", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Pieces", System.Type.GetType("System.Int32"));
                dataTableDCMType.Columns.Add("GrossWt", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("CW", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("FreightAmount", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("FreightTaxAmt", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("AW", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("IC", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("XSC", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("MY", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("OCCode", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("Other Charges", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("OCTax", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("TotalCharges", System.Type.GetType("System.Decimal"));
                dataTableDCMType.Columns.Add("Remarks", System.Type.GetType("System.String"));
                dataTableDCMType.Columns.Add("validationErrorDetailsDCM", System.Type.GetType("System.String"));


                #endregion



                string validationErrorDetailsDCM = string.Empty;
                DateTime tempDate; string Cr = string.Empty; string Dr = string.Empty;
                decimal tempDecimalValue, GrossWt, CW = 0;
                string[] formats = { "dd-MMM-yyyy" };
                CultureInfo ukCulture = new CultureInfo("en-GB");

                for (int i = 0; i < dataTableDCMExcelData.Rows.Count; i++)
                {
                    validationErrorDetailsDCM = string.Empty;
                    tempDecimalValue = 0;

                    #region Create row for CollectionType Data Table

                    DataRow dataRowDCMType = dataTableDCMType.NewRow();

                    #region 

                    dataRowDCMType["DCMIndex"] = i + 1;

                    #endregion RouteControlsIndex

                    #region SerialNumber INT NULL

                    dataRowDCMType["SerialNumber"] = i + 1;

                    #endregion SerialNumber

                    #region AgentCode VARCHAR(30) NULL

                    if (columnNames.Contains("agent code"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Agent Code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Agent Code is required ;";
                        }
                        else
                        {
                            dataRowDCMType["AgentCode"] = dataTableDCMExcelData.Rows[i]["Agent Code"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion AgentCode

                    #region Credit VARCHAR(1) NULL
                    if (columnNames.Contains("credit"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Credit"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Credit is required ;";
                        }
                        else if (dataTableDCMExcelData.Rows[i]["Credit"].ToString().Trim().Trim(',').Length > 1)
                        {
                            validationErrorDetailsDCM += "Credit must be a single character;";
                        }
                        else
                        {
                            dataRowDCMType["Credit"] = dataTableDCMExcelData.Rows[i]["Credit"].ToString().Trim().Trim(',').Substring(0, 1);
                            Cr = dataTableDCMExcelData.Rows[i]["Credit"].ToString().Trim().Trim(',').Substring(0, 1);
                        }
                    }

                    #endregion

                    #region Debit VARCHAR(1) NULL
                    if (columnNames.Contains("debit"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Debit"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Debit is required ;";
                        }
                        else if (dataTableDCMExcelData.Rows[i]["Debit"].ToString().Trim().Trim(',').Length > 1)
                        {
                            validationErrorDetailsDCM += "Debit must be a single character;";
                        }
                        else
                        {
                            dataRowDCMType["Debit"] = dataTableDCMExcelData.Rows[i]["Debit"].ToString().Trim().Trim(',').Substring(0, 1);
                            Dr = dataTableDCMExcelData.Rows[i]["Debit"].ToString().Trim().Trim(',').Substring(0, 1);
                        }
                    }
                    Cr = Cr?.ToUpper();
                    Dr = Dr?.ToUpper();

                    if ((Cr == "Y" && Dr == "Y") || (Cr == "N" && Dr == "N"))
                    {
                        validationErrorDetailsDCM = validationErrorDetailsDCM + "Credit/Debit should have valid value ;";
                    }

                    #endregion

                    #region Flt No VARCHAR(20) NULL
                    if (columnNames.Contains("flt no."))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Flt No."].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Flt No is required ;";
                        }
                        else
                        {
                            dataRowDCMType["Flt No"] = dataTableDCMExcelData.Rows[i]["Flt No."].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion

                    #region Flt Date VARCHAR(50) NULL
                    if (columnNames.Contains("flt date"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Flt Date"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Flt Date is required ;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowDCMType["Flt Date"] = DateTime.FromOADate(Convert.ToDouble(dataTableDCMExcelData.Rows[i]["Flt Date"].ToString().Trim()));
                                }
                                else
                                {
                                    //string dateString = dataTableDCMExcelData.Rows[i]["Flt Date"].ToString().Trim();

                                    string dateString = DateTime.Parse(dataTableDCMExcelData.Rows[i]["Flt Date"].ToString().Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                                    if (DateTime.TryParseExact(dateString, formats, ukCulture, DateTimeStyles.None, out tempDate))
                                    {
                                        dataRowDCMType["Flt Date"] = tempDate;
                                    }

                                    else
                                    {

                                        validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid Flt Date Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {

                                validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid Flt Date;";
                            }
                        }
                    }
                    #endregion

                    #region Applied Date VARCHAR(50) NULL
                    if (columnNames.Contains("applied date"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Applied Date"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Applied Date is required ;";
                        }
                        else
                        {
                            //string dateString = dataTableDCMExcelData.Rows[i]["Applied Date"].ToString().Trim();

                            string dateString = DateTime.Parse(dataTableDCMExcelData.Rows[i]["Applied Date"].ToString().Trim()).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                            //if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            //{
                            //    dataRowDCMType["Applied Date"] = tempDate;
                            //}


                            if (DateTime.TryParseExact(dateString, formats, ukCulture, DateTimeStyles.None, out tempDate))
                            {
                                dataRowDCMType["Applied Date"] = tempDate;
                            }

                            else if (string.IsNullOrEmpty(dataTableDCMExcelData.Rows[i]["Applied Date"].ToString().Trim()))
                            {
                                dataRowDCMType["Applied Date"] = DBNull.Value;
                            }
                            else
                            {

                                validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid Applied Date;";
                            }
                        }
                    }
                    #endregion

                    #region Origin VARCHAR(10) NULL
                    if (columnNames.Contains("origin"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Origin"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Origin is required ;";
                        }
                        else
                        {
                            dataRowDCMType["Origin"] = dataTableDCMExcelData.Rows[i]["Origin"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region Destination VARCHAR(10) NULL
                    if (columnNames.Contains("destination"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Destination"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Destination is required ;";
                        }
                        else
                        {
                            dataRowDCMType["Destination"] = dataTableDCMExcelData.Rows[i]["Destination"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region Commodity Code VARCHAR(10) NULL
                    if (columnNames.Contains("commodity code"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Commodity Code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Commodity Code is required ;";
                        }
                        else
                        {
                            dataRowDCMType["Commodity Code"] = dataTableDCMExcelData.Rows[i]["Commodity Code"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region Description VARCHAR(50) NULL
                    if (columnNames.Contains("description"))
                    {
                        if (dataTableDCMExcelData.Rows[i]["Description"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Description is required ;";
                        }
                        else
                        {
                            dataRowDCMType["Description"] = dataTableDCMExcelData.Rows[i]["Description"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region Pieces Int32

                    Int32 tempintValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Pieces"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["Pieces"] = tempintValue;
                    }
                    else
                    {
                        if (Int32.TryParse(dataTableDCMExcelData.Rows[i]["Pieces"].ToString().Trim().Trim(','), out tempintValue))
                        {
                            dataRowDCMType["Pieces"] = tempintValue;
                        }
                        else
                        {
                            dataRowDCMType["Pieces"] = tempintValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid Pieces Amount;";
                        }
                    }

                    #endregion

                    #region GrossWt 

                    GrossWt = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Gross weight"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["GrossWt"] = GrossWt;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["Gross weight"].ToString().Trim().Trim(','), out GrossWt))
                        {
                            dataRowDCMType["GrossWt"] = GrossWt;
                        }
                        else
                        {
                            dataRowDCMType["GrossWt"] = GrossWt;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid GrossWt;";
                        }
                    }

                    #endregion

                    #region CW 

                    CW = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["CW"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["CW"] = CW;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["CW"].ToString().Trim().Trim(','), out CW))
                        {
                            dataRowDCMType["CW"] = CW;
                        }
                        else
                        {
                            dataRowDCMType["CW"] = CW;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid CW;";
                        }
                    }

                    #endregion

                    if (GrossWt > CW)
                    {
                        validationErrorDetailsDCM += "GrossWt can not be greater than CW;";
                    }

                    #region FreightAmount 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Freight Amount"].ToString().Trim().Trim(',')))
                    {
                        //dataRowDCMType["FreightAmount"] = tempDecimalValue;
                        validationErrorDetailsDCM += "FreightAmount Can not be blank;";
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["Freight Amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            {
                                dataRowDCMType["FreightAmount"] = "-" + tempDecimalValue;
                            }
                            else
                            {
                                dataRowDCMType["FreightAmount"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["FreightAmount"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid FreightAmount;";
                        }
                    }

                    #endregion

                    #region FreightTaxAmt 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Freight Tax Amount"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["FreightTaxAmt"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["Freight Tax Amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            {
                                dataRowDCMType["FreightTaxAmt"] = "-" + tempDecimalValue;
                            }
                            else
                            {
                                dataRowDCMType["FreightTaxAmt"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["FreightTaxAmt"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid FreightTaxAmt;";
                        }
                    }

                    #endregion

                    #region AW 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["AW"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["AW"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["AW"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            {
                                dataRowDCMType["AW"] = "-" + tempDecimalValue;
                            }
                            else
                            {
                                dataRowDCMType["AW"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["AW"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid AW;";
                        }
                    }

                    #endregion

                    #region IC 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["IC"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["IC"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["IC"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            {
                                dataRowDCMType["IC"] = "-" + tempDecimalValue;
                            }
                            else
                            {
                                dataRowDCMType["IC"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["IC"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid IC;";
                        }
                    }

                    #endregion

                    #region XSC 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["XSC"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["XSC"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["XSC"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            {
                                dataRowDCMType["XSC"] = "-" + tempDecimalValue;
                            }
                            else
                            {
                                dataRowDCMType["XSC"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["XSC"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid XSC;";
                        }
                    }

                    #endregion

                    #region MY 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["MY"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["MY"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["MY"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            {
                                dataRowDCMType["MY"] = "-" + tempDecimalValue;
                            }
                            else
                            {
                                dataRowDCMType["MY"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["MY"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid MY;";
                        }
                    }

                    #endregion

                    #region OCCode VARCHAR(100) NULL

                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["OC Code"].ToString().Trim().Trim(',')))
                    {
                        validationErrorDetailsDCM = validationErrorDetailsDCM + "OCCode is required;";
                    }
                    else
                    {

                        dataRowDCMType["OCCode"] = dataTableDCMExcelData.Rows[i]["OC Code"].ToString().Trim().ToUpper().Trim(',');

                    }

                    #endregion OCCode

                    #region OtherCharges VARCHAR(500) NULL

                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Other Charges"].ToString().Trim().Trim(',')))
                    {
                        validationErrorDetailsDCM = validationErrorDetailsDCM + "OtherCharges is required;";
                    }
                    else
                    {
                        if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                        {
                            dataRowDCMType["Other Charges"] = "-" + dataTableDCMExcelData.Rows[i]["Other Charges"].ToString().Trim().ToUpper().Trim(',');

                        }
                        else
                        {
                            dataRowDCMType["Other Charges"] = dataTableDCMExcelData.Rows[i]["Other Charges"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion OtherCharges

                    #region OCTax 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["OC Tax"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["OCTax"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["OC Tax"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            { dataRowDCMType["OCTax"] = "-" + tempDecimalValue; }
                            else
                            {
                                dataRowDCMType["OCTax"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["OCTax"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid OCTax;";
                        }
                    }

                    #endregion

                    #region TotalCharges 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Total Charges"].ToString().Trim().Trim(',')))
                    {
                        dataRowDCMType["TotalCharges"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableDCMExcelData.Rows[i]["Total Charges"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            if (dataTableDCMExcelData.Rows[i]["Credit"].ToString() == "Y")
                            { dataRowDCMType["TotalCharges"] = "-" + tempDecimalValue; }
                            else
                            {
                                dataRowDCMType["TotalCharges"] = tempDecimalValue;
                            }
                        }
                        else
                        {
                            dataRowDCMType["TotalCharges"] = tempDecimalValue;
                            validationErrorDetailsDCM = validationErrorDetailsDCM + "Invalid TotalCharges;";
                        }
                    }

                    #endregion

                    #region Remarks VARCHAR(100) NULL

                    if (string.IsNullOrWhiteSpace(dataTableDCMExcelData.Rows[i]["Remark"].ToString().Trim().Trim(',')))
                    {
                        validationErrorDetailsDCM = validationErrorDetailsDCM + "Remarks is required;";
                    }
                    else
                    {

                        dataRowDCMType["Remarks"] = dataTableDCMExcelData.Rows[i]["Remark"].ToString().Trim().ToUpper().Trim(',');

                    }

                    #endregion Remarks

                    dataRowDCMType["validationErrorDetailsDCM"] = validationErrorDetailsDCM;
                    dataTableDCMType.Rows.Add(dataRowDCMType);
                    #endregion Create row for CollectionType Data Table

                }


                // Database Call to Validate & Insert Route Controls Master
                string errorInSp = string.Empty;
                await ValidateAndInsertDCMMaster(srNotblMasterUploadSummaryLog, dataTableDCMType, errorInSp);

                return true;

            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableDCMExcelData = null;
            }
        }

        public async Task<DataSet?> ValidateAndInsertDCMMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableDCMType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@DCMType", dataTableDCMType),
                    new SqlParameter("@Error", errorInSp)
                 ];

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("USPDCMUpload", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("USPDCMUpload", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return dataSetResult;
            }
        }


    }
}
