using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using QID.DataAccess;
using System.Threading;
using System.IO;
using Excel;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;


namespace QidWorkerRole.UploadMasters.Collection
{
    class InvoiceCollection
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
        public Boolean UpdateInvoiceCollection(DataSet dataSetFileData)
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
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "CollectionUploadFile", out uploadFilePath))
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
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
        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableCollectionExcelData = new DataTable("dataTableCollectionExcelData");

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
                dataTableCollectionExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableCollectionExcelData);

                foreach (DataColumn dataColumn in dataTableCollectionExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableCollectionExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating RouteControlsMasterType DataTable

                DataTable dataTableCollectionType = new DataTable("dataTableCollectionExcelData");
                dataTableCollectionType.Columns.Add("InvoiceIndex", System.Type.GetType("System.Int32"));
                dataTableCollectionType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                dataTableCollectionType.Columns.Add("InvoiceNumber", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("InvoiceType", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("CollectedAmount", System.Type.GetType("System.Decimal"));
                dataTableCollectionType.Columns.Add("TAXAmount", System.Type.GetType("System.Decimal"));
                dataTableCollectionType.Columns.Add("VATAmount", System.Type.GetType("System.Decimal"));
                dataTableCollectionType.Columns.Add("PaymentCurrency", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("CurrencyRate", System.Type.GetType("System.Decimal"));
                dataTableCollectionType.Columns.Add("PaymentType", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("ChequeDdNo", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("ChequeDate", System.Type.GetType("System.DateTime"));
                dataTableCollectionType.Columns.Add("BankName", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("DepositDate", System.Type.GetType("System.DateTime"));
                dataTableCollectionType.Columns.Add("TIN#", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("ORNo", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("PPRemarks", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("TDSPercent", System.Type.GetType("System.Decimal"));
                dataTableCollectionType.Columns.Add("ReferenceNoofpayment", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("PaidBy", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                dataTableCollectionType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                dataTableCollectionType.Columns.Add("validationErrorDetailsCollection", System.Type.GetType("System.String"));


                #endregion



                string validationErrorDetailsCollection = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;

                for (int i = 0; i < dataTableCollectionExcelData.Rows.Count; i++)
                {
                    validationErrorDetailsCollection = string.Empty;
                    tempDecimalValue = 0;

                    #region Create row for CollectionType Data Table

                    DataRow dataRowCollectionType = dataTableCollectionType.NewRow();

                    #region 

                    dataRowCollectionType["InvoiceIndex"] = i + 1;

                    #endregion RouteControlsIndex

                    #region SerialNumber INT NULL

                    dataRowCollectionType["SerialNumber"] = DBNull.Value;

                    #endregion SerialNumber

                    #region InvoiceNumber VARCHAR(30) NULL

                    if (columnNames.Contains("invoice number"))
                    {
                        if (dataTableCollectionExcelData.Rows[i]["Invoice Number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invoice Number is required ;";
                        }
                        else
                        {
                            dataRowCollectionType["InvoiceNumber"] = dataTableCollectionExcelData.Rows[i]["Invoice Number"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion InvoiceNumber

                    #region InvoiceType VARCHAR(200) NULL
                    string[] array1 = { "Agent", "Walk-In", "Proforma", "Destination", "Delivery", "Interline", "MiscInvoice" };
                    if (columnNames.Contains("invoice type"))
                    {
                        if (dataTableCollectionExcelData.Rows[i]["Invoice Type"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invoice Type is required ;";
                        }
                        else
                        {
                            if (!Array.Exists(array1, element => element == dataTableCollectionExcelData.Rows[i]["Invoice Type"].ToString().Trim()))
                            {
                                validationErrorDetailsCollection = validationErrorDetailsCollection + " InvoiceType in excel is invalid ";

                            }
                            else
                            {
                                dataRowCollectionType["InvoiceType"] = dataTableCollectionExcelData.Rows[i]["Invoice Type"].ToString().Trim().Trim(',');

                            }

                        }
                    }

                    #endregion

                    #region Collected Amount NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["Collected Amount"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["CollectedAmount"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCollectionExcelData.Rows[i]["Collected Amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCollectionType["CollectedAmount"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCollectionType["CollectedAmount"] = tempDecimalValue;
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Collected Amount;";
                        }
                    }

                    #endregion 

                    #region Tax Amount 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["TAX Amount"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["TAXAmount"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCollectionExcelData.Rows[i]["TAX Amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCollectionType["TAXAmount"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCollectionType["TAXAmount"] = tempDecimalValue;
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Collected Amount;";
                        }
                    }

                    #endregion

                    #region VAT Amount 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["VAT Amount"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["VATAmount"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCollectionExcelData.Rows[i]["VAT Amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCollectionType["VATAmount"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCollectionType["VATAmount"] = tempDecimalValue;
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Collected Amount;";
                        }
                    }

                    #endregion
                    #region Currency VARCHAR(5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["Payment Currency"].ToString().Trim().Trim(',')))
                    {
                        validationErrorDetailsCollection = validationErrorDetailsCollection + " Payment Currency is required;";
                    }
                    else
                    {
                        if (dataTableCollectionExcelData.Rows[i]["Payment Currency"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationErrorDetailsCollection = validationErrorDetailsCollection + " Currency is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowCollectionType["PaymentCurrency"] = dataTableCollectionExcelData.Rows[i]["Payment Currency"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Currency

                    #region Currency Rate 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["Currency Rate"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["CurrencyRate"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCollectionExcelData.Rows[i]["Currency Rate"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCollectionType["CurrencyRate"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCollectionType["CurrencyRate"] = tempDecimalValue;
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Currency Rate;";
                        }
                    }

                    #endregion

                    #region Payment Type
                    string[] array = { "CASH", "CHEQUE", "DEPOSITE", "ADVANCE", "CREDIT ACCOUNT", "OTHERS", "RTGS", "CARD", "DD", "PO/GBL", "VERIFONE", "NEFT", "IMPS", "PHONE PAY", "PAYTM" };

                    if (columnNames.Contains("payment type"))
                    {
                        if (dataTableCollectionExcelData.Rows[i]["Payment Type"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Payment Type is required ;";
                        }
                        else
                        {
                            if (!Array.Exists(array, element => element == Convert.ToString(dataTableCollectionExcelData.Rows[i]["Payment Type"]).ToString().ToUpper().Trim()))
                            {
                                validationErrorDetailsCollection = validationErrorDetailsCollection + " PaymentType in excel is invalid ";

                            }
                            else
                            {
                                dataRowCollectionType["PaymentType"] = dataTableCollectionExcelData.Rows[i]["Payment Type"].ToString().Trim().Trim(',');

                            }

                        }
                    }
                    #endregion 

                    #region Cheque/DD/RTGS/CARD# VARCHAR(50) NULL

                    if (columnNames.Contains("cheque/dd/rtgs/card#"))
                    {
                        if (!dataTableCollectionExcelData.Rows[i]["Cheque/DD/RTGS/CARD#"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowCollectionType["ChequeDdNo"] = dataTableCollectionExcelData.Rows[i]["Cheque/DD/RTGS/CARD#"].ToString().Trim().Trim(',');
                        }
                        else
                        {
                            if (dataRowCollectionType["PaymentType"].ToString().ToUpper().Trim() == "DD" && dataRowCollectionType["ChequeDdNo"].ToString() == "")
                            {
                                validationErrorDetailsCollection = validationErrorDetailsCollection + " DD No. is required ;";
                            }
                            if (dataRowCollectionType["PaymentType"].ToString().ToUpper().Trim() == "CHEQUE" && dataRowCollectionType["ChequeDdNo"].ToString() == "")
                            {
                                validationErrorDetailsCollection = validationErrorDetailsCollection + " CHEQUE No. is required ;";
                            }
                            if (dataRowCollectionType["PaymentType"].ToString().ToUpper().Trim() == "RTGS" && dataRowCollectionType["ChequeDdNo"].ToString() == "")
                            {
                                validationErrorDetailsCollection = validationErrorDetailsCollection + " RTGS No. is required ;";
                            }
                            dataRowCollectionType["ChequeDdNo"] = DBNull.Value;
                        }
                    }
                    else
                    {
                        validationErrorDetailsCollection = validationErrorDetailsCollection + "Cheque/DD/RTGS/CARD# column not found;";
                    }


                    #endregion

                    #region 
                    if (dataRowCollectionType["PaymentType"].ToString().ToUpper().Trim() == "CHEQUE" && dataTableCollectionExcelData.Rows[i]["Cheque Date"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationErrorDetailsCollection = validationErrorDetailsCollection + "Cheque Date is Manadatory when Payment type is  Cheque;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowCollectionType["ChequeDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableCollectionExcelData.Rows[i]["Cheque Date"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableCollectionExcelData.Rows[i]["Cheque Date"].ToString().Trim(), out tempDate))
                                {
                                    dataRowCollectionType["ChequeDate"] = tempDate;
                                }
                                else if (string.IsNullOrEmpty(dataTableCollectionExcelData.Rows[i]["Cheque Date"].ToString().Trim()))
                                {
                                    dataRowCollectionType["ChequeDate"] = DBNull.Value;
                                }
                                else
                                {

                                    validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Cheque Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {

                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Cheque Date;";
                        }
                    }

                    #endregion 

                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["Bank Name"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["BankName"] = DBNull.Value;
                    }
                    else
                    {

                        dataRowCollectionType["BankName"] = dataTableCollectionExcelData.Rows[i]["Bank Name"].ToString().Trim().ToUpper().Trim(',');

                    }

                    if (dataTableCollectionExcelData.Rows[i]["Deposit Date"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationErrorDetailsCollection = validationErrorDetailsCollection + "Deposite Date is Manadatory ;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowCollectionType["DepositDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableCollectionExcelData.Rows[i]["Deposit Date"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableCollectionExcelData.Rows[i]["Deposit Date"].ToString().Trim(), out tempDate))
                                {
                                    dataRowCollectionType["DepositDate"] = tempDate;
                                }
                                else
                                {

                                    validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Deposite Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {

                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid Deposite Date;";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["TIN#"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["TIN#"] = DBNull.Value;
                    }
                    else
                    {

                        dataRowCollectionType["TIN#"] = dataTableCollectionExcelData.Rows[i]["TIN#"].ToString().Trim().ToUpper().Trim(',');

                    }

                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["OR No."].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["ORNo"] = DBNull.Value;
                    }
                    else
                    {

                        dataRowCollectionType["ORNo"] = dataTableCollectionExcelData.Rows[i]["OR No."].ToString().Trim().ToUpper().Trim(',');

                    }
                    if (dataRowCollectionType["ORNo"].ToString().Trim() == "")
                    {

                        if (decimal.Parse(dataRowCollectionType["CollectedAmount"].ToString().Trim().Trim(',')) < 0)
                        {
                            dataRowCollectionType["ORNo"] = ValidateORNo("CR", 0, "SKAdmin");
                        }
                        else
                        {
                            dataRowCollectionType["ORNo"] = ValidateORNo("OR", 0, "SKAdmin");
                        }

                    }
                    #region Remarks VARCHAR(500) NULL

                    if (columnNames.Contains("pp remarks"))
                    {
                        if (dataTableCollectionExcelData.Rows[i]["PP Remarks"].ToString().Trim().Trim(',').Length > 500)
                        {
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Remarks is more than 500 Chars;";
                        }
                        else
                        {
                            dataRowCollectionType["PPRemarks"] = dataTableCollectionExcelData.Rows[i]["PP Remarks"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion Remarks

                    #region TDSPercent [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCollectionExcelData.Rows[i]["TDS(%)"].ToString().Trim().Trim(',')))
                    {
                        dataRowCollectionType["TDSPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCollectionExcelData.Rows[i]["TDS(%)"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCollectionType["TDSPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCollectionType["TDSPercent"] = tempDecimalValue;
                            validationErrorDetailsCollection = validationErrorDetailsCollection + "Invalid TDS %;";
                        }
                    }
                    if (columnNames.Contains("reference no of payment"))
                    {
                        if (dataTableCollectionExcelData.Rows[i]["Reference No of payment"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowCollectionType["ReferenceNoofpayment"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowCollectionType["ReferenceNoofpayment"] = dataTableCollectionExcelData.Rows[i]["Reference No of payment"].ToString().Trim().Trim(',');
                        }
                    }
                    if (columnNames.Contains("paid by"))
                    {
                        if (dataTableCollectionExcelData.Rows[i]["Paid By"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowCollectionType["PaidBy"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowCollectionType["PaidBy"] = dataTableCollectionExcelData.Rows[i]["Paid By"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion
                    #region UpdatedOn [datetime] NULL

                    dataRowCollectionType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region UpdatedBy [varchar] (30) NULL

                    dataRowCollectionType["UpdatedBy"] = string.Empty;

                    #endregion UpdatedBy
                    dataRowCollectionType["validationErrorDetailsCollection"] = validationErrorDetailsCollection;
                    dataTableCollectionType.Rows.Add(dataRowCollectionType);
                    #endregion Create row for CollectionType Data Table

                }


                // Database Call to Validate & Insert Route Controls Master
                string errorInSp = string.Empty;
                ValidateAndInsertCollectionMaster(srNotblMasterUploadSummaryLog, dataTableCollectionType, errorInSp);
               
                return true;

            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableCollectionExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Route Controls Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="dataTableRateLineType"> Route Controls Master Table Type </param>
        /// <param name="dataTableRateLineParamType"> Route Config Params Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public DataSet ValidateAndInsertCollectionMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableCollectionType,
                                                                                                string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {   new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@CollectionType", dataTableCollectionType),

                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("USPInvoiceLevelCollectionUpload", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return dataSetResult;
            }
        }



        public String ValidateORNo(string strRotationId, int intYear, string strUserName)
        {
            DataSet res = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {   new SqlParameter("@strRotationId",strRotationId),
                                                                      new SqlParameter("@intYear", intYear),
                                                                      new SqlParameter("@strUserName", strUserName),
                                                                      new SqlParameter("@strOutput", ParameterDirection.Output)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                res = sQLServer.SelectRecords("spGenerateRotationNoNew", sqlParameters);
                string s = res.Tables[0].Rows[0][0].ToString();
                return s;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return "";
            }
        }

    }
}

