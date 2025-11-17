using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.Vendor
{
    public class UploadVendorMaster
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadVendorMaster> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadVendorMaster(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadVendorMaster> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;
        }
        #endregion

        /// <summary>
        /// Method to Uplaod Cost Line Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<bool> VendorMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.CostMaster);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "VendorMasterUploadFile", out uploadFilePath))
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
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
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Message: " + ex.Message + " \n StackTrace: " + ex.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to Process Cost Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Cost Master Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableVendorMasterExcelData = new DataTable();

            bool isBinaryReader = false;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTableVendorMasterExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommon.RemoveEmptyRows(dataTableVendorMasterExcelData);

                foreach (DataColumn dataColumn in dataTableVendorMasterExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                #region Creating CostType DataTable

                DataTable VendorType = new DataTable("VendorMaster");

                VendorType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                VendorType.Columns.Add("Vendor_code", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Iata_acc_code", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Vendor_name", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Dba", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Valid_from", System.Type.GetType("System.DateTime"));
                VendorType.Columns.Add("Valid_to", System.Type.GetType("System.DateTime"));
                VendorType.Columns.Add("Participation_type", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Country", System.Type.GetType("System.String"));
                VendorType.Columns.Add("State", System.Type.GetType("System.String"));
                VendorType.Columns.Add("City", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Address1", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Address2", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Phone_no", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Zip_code", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Email", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Contact_person", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Mobile_number", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Fax", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Tin", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Is_active", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_address_same_as_mailing", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_address1", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_address2", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_city", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_state", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_zip_code", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_country", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_contact_person", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Billing_phone_no", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Remarks", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Station", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Sita_address", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Type_of_charges", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Currency_code", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Bank_details", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Email_address", System.Type.GetType("System.String"));
                VendorType.Columns.Add("Eoricode", System.Type.GetType("System.String"));
                VendorType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));
                

                #endregion

                string validationDetails = string.Empty;
                
                string uLDType = string.Empty;

                for (int i = 0; i < dataTableVendorMasterExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;

                    DataRow dataRowCostType = VendorType.NewRow();

                    #region ReferenceID INT NOT NULL

                    dataRowCostType["ReferenceID"] = i + 1;

                    #endregion ReferenceID

                    #region Vendor_code
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["vendor_code*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Vendor Code is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["vendor_code*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "Vendor Code is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Vendor_code"] = dataTableVendorMasterExcelData.Rows[i]["vendor_code*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion Vendor_code

                    #region Iata_acc_code
                    dataRowCostType["Iata_acc_code"] = dataTableVendorMasterExcelData.Rows[i]["iata_acc_code"].ToString().Trim().Trim(',');
                    #endregion Iata_acc_code

                    #region Vendor_name
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["vendor_name*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Vendor Code is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["vendor_name*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "Vendor Name is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Vendor_name"] = dataTableVendorMasterExcelData.Rows[i]["vendor_name*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion Vendor_name

                    #region Dba
                    dataRowCostType["Dba"] = dataTableVendorMasterExcelData.Rows[i]["dba"].ToString().Trim().Trim(',');
                    #endregion Dba

                    #region Valid_from
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["valid_from*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Valid_from is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["valid_from*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "Valid_from is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Valid_from"] = dataTableVendorMasterExcelData.Rows[i]["valid_from*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion Valid_from

                    #region valid_to
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["valid_to*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " valid_to is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["valid_to*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "valid_to is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Valid_to"] = dataTableVendorMasterExcelData.Rows[i]["valid_to*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion valid_to

                    #region Dba
                    dataRowCostType["Participation_type"] = dataTableVendorMasterExcelData.Rows[i]["participation_type"].ToString().Trim().Trim(',');
                    #endregion Dba

                    #region country
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["country*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " country is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "country is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Country"] = dataTableVendorMasterExcelData.Rows[i]["country*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion country

                    #region state
                    dataRowCostType["State"] = dataTableVendorMasterExcelData.Rows[i]["state"].ToString().Trim().Trim(',');
                    #endregion state

                    #region city
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["city*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " city is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "city is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["City"] = dataTableVendorMasterExcelData.Rows[i]["city*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion country

                    #region address1
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " address1 is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "address1 is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Address1"] = dataTableVendorMasterExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion address1

                    #region address2
                    dataRowCostType["Address2"] = dataTableVendorMasterExcelData.Rows[i]["address2"].ToString().Trim().Trim(',');
                    #endregion address2

                    #region phone_no
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " phone_no is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "phone_no is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Phone_no"] = dataTableVendorMasterExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion phone_no

                    #region zip_code
                    dataRowCostType["Zip_code"] = dataTableVendorMasterExcelData.Rows[i]["zip_code"].ToString().Trim().Trim(',');
                    #endregion zip_code

                    #region email
                    dataRowCostType["Email"] = dataTableVendorMasterExcelData.Rows[i]["email"].ToString().Trim().Trim(',');
                    #endregion email

                    #region contact_person
                    dataRowCostType["Contact_person"] = dataTableVendorMasterExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',');
                    #endregion contact_person

                    #region mobile_number
                    dataRowCostType["Mobile_number"] = dataTableVendorMasterExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',');
                    #endregion mobile_number

                    #region fax
                    dataRowCostType["Fax"] = dataTableVendorMasterExcelData.Rows[i]["fax"].ToString().Trim().Trim(',');
                    #endregion fax

                    #region tin
                    dataRowCostType["Tin"] = dataTableVendorMasterExcelData.Rows[i]["tin"].ToString().Trim().Trim(',');
                    #endregion tin

                    #region is_active
                    if (string.IsNullOrWhiteSpace(dataTableVendorMasterExcelData.Rows[i]["is_active*"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " is_active is required;";
                    }
                    else
                    {
                        if (dataTableVendorMasterExcelData.Rows[i]["is_active*"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "is_active is more than 50 Chars;";
                        }
                        else
                        {
                            if (dataTableVendorMasterExcelData.Rows[i]["is_active*"].ToString().Trim().Trim(',') == "Y")
                            {
                                dataRowCostType["Is_active"] = "1";
                            }
                            else
                            {
                                dataRowCostType["Is_active"] = "0";
                            }
                            
                        }
                    }
                    #endregion is_active

                    #region Billing_address_same_as_mailing
                    if (dataTableVendorMasterExcelData.Rows[i]["billing_address_same_as_mailing"].ToString().Trim().Trim(',') == "Y")
                    {
                        dataRowCostType["Billing_address_same_as_mailing"] = "1";
                    }
                    else
                    {
                        dataRowCostType["Billing_address_same_as_mailing"] = "0";
                    }
                    #endregion Billing_address_same_as_mailing

                    #region Billing_address1
                    dataRowCostType["Billing_address1"] = "1";
                    #endregion Billing_address1

                    #region Billing_address2
                    dataRowCostType["Billing_address2"] = dataTableVendorMasterExcelData.Rows[i]["billing_address2"].ToString().Trim().Trim(',');
                    #endregion Billing_address2

                    #region Billing_city
                    dataRowCostType["Billing_city"] = dataTableVendorMasterExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',');
                    #endregion Billing_city

                    #region Billing_state
                    dataRowCostType["Billing_state"] = dataTableVendorMasterExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',');
                    #endregion Billing_state

                    #region Billing_zip_code
                    dataRowCostType["Billing_zip_code"] = dataTableVendorMasterExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',');
                    #endregion Billing_zip_code

                    #region Billing_country
                    dataRowCostType["Billing_country"] = dataTableVendorMasterExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',');
                    #endregion Billing_country

                    #region Billing_contact_person
                    dataRowCostType["Billing_contact_person"] = dataTableVendorMasterExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',');
                    #endregion Billing_contact_person

                    #region billing_phone_no
                    dataRowCostType["Billing_phone_no"] = dataTableVendorMasterExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',');
                    #endregion billing_phone_no

                    #region remarks
                    dataRowCostType["Remarks"] = dataTableVendorMasterExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',');
                    #endregion remarks

                    #region station
                    dataRowCostType["Station"] = dataTableVendorMasterExcelData.Rows[i]["station"].ToString().Trim().Trim(',');
                    #endregion station

                    #region sita_address
                    dataRowCostType["Sita_address"] = dataTableVendorMasterExcelData.Rows[i]["sita_address"].ToString().Trim().Trim(',');
                    #endregion sita_address

                    #region type_of_charges
                    dataRowCostType["Type_of_charges"] = dataTableVendorMasterExcelData.Rows[i]["type_of_charges"].ToString().Trim().Trim(',');
                    #endregion type_of_charges

                    #region currency_code
                    dataRowCostType["Currency_code"] = dataTableVendorMasterExcelData.Rows[i]["currency_code"].ToString().Trim().Trim(',');
                    #endregion currency_code

                    #region bank_details
                    dataRowCostType["Bank_details"] = dataTableVendorMasterExcelData.Rows[i]["bank_details"].ToString().Trim().Trim(',');
                    #endregion bank_details

                    #region email_address
                    dataRowCostType["Email_address"] = dataTableVendorMasterExcelData.Rows[i]["email_address"].ToString().Trim().Trim(',');
                    #endregion temail_addressin

                    #region eoricode
                    dataRowCostType["Eoricode"] = dataTableVendorMasterExcelData.Rows[i]["eoricode"].ToString().Trim().Trim(',');
                    #endregion eoricode

                    #region ValidationDetails
                    dataRowCostType["ValidationDetails"] = validationDetails;
                    #endregion ValidationDetails

                    VendorType.Rows.Add(dataRowCostType);
                }

                // Database Call to Validate & Insert Cost Line Master
                string errorInSp = string.Empty;
                ValidateAndInsertVendorMaster(srNotblMasterUploadSummaryLog, VendorType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", exception.Message, exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableVendorMasterExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Cost Line Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> tblMasterUploadSummaryLog Primay Key </param>
        /// <param name="dataTableCostType"> CostType DataTable </param>
        /// <param name="dataTableCostSlabsType"> CostSlabsType DataTable </param>
        /// <param name="dataTableCostULDSlabsType"> CostULDSlabsType DataTable </param>
        /// <param name="dataTableCostParamsType"> CostParamsType DataTable </param>
        /// <param name="dataTableCostRemarksType"> CostRemarksType DataTable </param>
        /// <param name="errorInSp"> Stored Procedure Out Parameter </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet?> ValidateAndInsertVendorMaster(int srNotblMasterUploadSummaryLog, DataTable VendorType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog", srNotblMasterUploadSummaryLog),
                                                                    new SqlParameter("@VendorType", VendorType),
                                                                    new SqlParameter("@Error", errorInSp)
                                                                  };



                //SQLServer sQLServer = new SQLServer();
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspUploadVendorMaster", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Message: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", ex.Message, ex.StackTrace);
                return dataSetResult;
            }
        }
    }
}
