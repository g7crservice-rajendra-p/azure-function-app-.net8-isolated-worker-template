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




namespace QidWorkerRole.UploadMasters.CCAUpload
{
    class CCAUploadFile
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
        public Boolean UpdateCCAUpload(DataSet dataSetFileData)
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
                                                              "CCAUploadFile", out uploadFilePath))
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
            DataTable dataTableCCAExcelData = new DataTable("dataTableCCAExcelData");

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
                dataTableCCAExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableCCAExcelData);

                foreach (DataColumn dataColumn in dataTableCCAExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableCCAExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating RouteControlsMasterType DataTable

                DataTable dataTableCCAType = new DataTable("dataTableCCAExcelData");
                dataTableCCAType.Columns.Add("CCAIndex", System.Type.GetType("System.Int32"));
                dataTableCCAType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                dataTableCCAType.Columns.Add("AWBNumber", System.Type.GetType("System.String"));
                dataTableCCAType.Columns.Add("RevisedCorrectWeight", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("CommCode", System.Type.GetType("System.String"));
                dataTableCCAType.Columns.Add("WeightCharges", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("Commission", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("Incentive", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("NetAmount", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("TDSCommission", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("STCommission", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("TotalOtherChargesDueAirport", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("TotalOtherChargesDueAirline", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("AWBFees", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("MCFees", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("MOFees", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("CCAFees", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("DOFees", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("OtherFees", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("TotalPayabletoAirline", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("Tax", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("RatePerKg", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("Total", System.Type.GetType("System.Decimal"));
                dataTableCCAType.Columns.Add("ReasonforCorrection", System.Type.GetType("System.String"));
                dataTableCCAType.Columns.Add("CCANumber", System.Type.GetType("System.String"));
                dataTableCCAType.Columns.Add("Status", System.Type.GetType("System.String"));
                dataTableCCAType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                dataTableCCAType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                dataTableCCAType.Columns.Add("validationErrorDetailsCCA", System.Type.GetType("System.String"));


                #endregion



                string validationErrorDetailsCCA = string.Empty;
                
                decimal tempDecimalValue = 0;

                for (int i = 0; i < dataTableCCAExcelData.Rows.Count; i++)
                {
                    validationErrorDetailsCCA = string.Empty;
                    tempDecimalValue = 0;

                    #region Create row for CollectionType Data Table

                    DataRow dataRowCCAType = dataTableCCAType.NewRow();

                    #region 

                    dataRowCCAType["CCAIndex"] = i + 1;

                    #endregion RouteControlsIndex

                    #region SerialNumber INT NULL

                    dataRowCCAType["SerialNumber"] = DBNull.Value;

                    #endregion SerialNumber

                    #region AWBNumber VARCHAR(30) NULL

                    if (columnNames.Contains("awb number"))
                    {
                        if (dataTableCCAExcelData.Rows[i]["AWB Number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "AWB Number is required ;";
                        }
                        else
                        {
                            if (dataTableCCAExcelData.Rows[i]["AWB Number"].ToString().Trim().Trim(',').Length != 12)
                            {
                                validationErrorDetailsCCA = validationErrorDetailsCCA + " AWBNumber length should not be greater than 12;";
                            }
                            else
                            {
                                dataRowCCAType["AWBNumber"] = dataTableCCAExcelData.Rows[i]["AWB Number"].ToString().Trim().ToUpper().Trim(',');
                            }
                        }
                    }

                    #endregion AWBNumber

                    #region Revised/Correct Weight 
                    tempDecimalValue = 0;
                    if (columnNames.Contains("revised/correct weight"))
                    {
                        if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Revised/Correct Weight"].ToString().Trim().Trim(',')))
                        {
                            dataRowCCAType["RevisedCorrectWeight"] = tempDecimalValue;
                        }
                        else
                        {
                            if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Revised/Correct Weight"].ToString().Trim().Trim(','), out tempDecimalValue))
                            {
                                dataRowCCAType["RevisedCorrectWeight"] = tempDecimalValue;
                            }
                            else
                            {
                                dataRowCCAType["RevisedCorrectWeight"] = tempDecimalValue;
                                validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Revised/Correct Weight;";
                            }
                        }
                    }

                    #endregion

                    #region Comm Code

                    if (dataTableCCAExcelData.Rows[i]["Comm Code"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationErrorDetailsCCA = validationErrorDetailsCCA + "CommodityCode is more than 4000 Chars;";
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Comm Code"].ToString().Trim().Trim(',')))
                        {
                            dataRowCCAType["CommCode"] = DBNull.Value;
                        }
                        else
                        {

                            dataRowCCAType["CommCode"] = dataTableCCAExcelData.Rows[i]["Comm Code"].ToString().Trim().ToUpper().Trim(',');

                        }
                    }

                    #endregion 

                    #region Weight Charges 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Weight Charges"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["WeightCharges"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Weight Charges"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["WeightCharges"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["WeightCharges"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Weight Charges;";
                        }
                    }

                    #endregion

                    #region Commission 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Commission"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["Commission"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Commission"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["Commission"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["Commission"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Commission;";
                        }
                    }

                    #endregion


                    #region Incentive 

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Incentive"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["Incentive"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Incentive"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["Incentive"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["Incentive"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Incentive;";
                        }
                    }

                    #endregion
                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Net/Net Amount"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["NetAmount"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Net/Net Amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["NetAmount"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["NetAmount"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Net/Net Amount;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["TDSCommission"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["TDSCommission"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["TDSCommission"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["TDSCommission"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["TDSCommission"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid TDSCommission;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["STCommission"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["STCommission"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["STCommission"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["STCommission"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["STCommission"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid STCommission;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Total Other Charges Due Airport"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["TotalOtherChargesDueAirport"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Total Other Charges Due Airport"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["TotalOtherChargesDueAirport"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["TotalOtherChargesDueAirport"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Total Other Charges Due Airport;";
                        }
                    }
                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Total Other Charges Due Airline"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["TotalOtherChargesDueAirline"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Total Other Charges Due Airline"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["TotalOtherChargesDueAirline"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["TotalOtherChargesDueAirline"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Total Other Charges Due Airline;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["AWB Fees"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["AWBFees"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["AWB Fees"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["AWBFees"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["AWBFees"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid AWB Fees;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["MC Fees"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["MCFees"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["MC Fees"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["MCFees"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["MCFees"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid MC Fees;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["MO Fees"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["MOFees"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["MO Fees"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["MOFees"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["MOFees"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid MO Fees;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["CCA Fees"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["CCAFees"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["CCA Fees"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["CCAFees"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["CCAFees"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid CCA Fees;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["DO Fees"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["DOFees"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["DO Fees"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["DOFees"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["DOFees"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid DO Fees;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Other Fees"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["OtherFees"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Other Fees"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["OtherFees"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["OtherFees"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Other Fees;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Total Payable to Airline (Ex:VAT)"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["TotalPayabletoAirline"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Total Payable to Airline (Ex:VAT)"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["TotalPayabletoAirline"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["TotalPayabletoAirline"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Total Payable to Airline (Ex:VAT);";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Tax"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["Tax"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Tax"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["Tax"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["Tax"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Tax;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Rate Per Kg"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["RatePerKg"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Rate Per Kg"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["RatePerKg"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["RatePerKg"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Rate Per Kg;";
                        }
                    }

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["Total"].ToString().Trim().Trim(',')))
                    {
                        dataRowCCAType["Total"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCCAExcelData.Rows[i]["Total"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCCAType["Total"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCCAType["Total"] = tempDecimalValue;
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Invalid Total;";
                        }
                    }

                    #region Reason for Correction VARCHAR(500) NULL

                    if (columnNames.Contains("reason for correction"))
                    {
                        if (dataTableCCAExcelData.Rows[i]["Reason for Correction"].ToString().Trim().Trim(',').Length > 500)
                        {
                            validationErrorDetailsCCA = validationErrorDetailsCCA + "Reason for Correction is more than 500 Chars;";
                        }
                        else
                        {
                            dataRowCCAType["ReasonforCorrection"] = dataTableCCAExcelData.Rows[i]["Reason for Correction"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion Reason for Correction

                    if (dataRowCCAType["CCANumber"].ToString().Trim() == "")
                    {

                        if (!string.IsNullOrWhiteSpace(dataTableCCAExcelData.Rows[i]["AWB Number"].ToString().Trim().Trim(',')))
                        {
                            dataRowCCAType["CCANumber"] = GetNextCCAIDNumber("CCA", 0, "SKAdmin");
                        }
                        
                    }

                    dataRowCCAType["Status"] = "New";

                    #region UpdatedOn [datetime] NULL

                    dataRowCCAType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region UpdatedBy [varchar] (30) NULL

                    dataRowCCAType["UpdatedBy"] = string.Empty;

                    #endregion UpdatedBy
                    dataRowCCAType["validationErrorDetailsCCA"] = validationErrorDetailsCCA;
                    dataTableCCAType.Rows.Add(dataRowCCAType);
                    #endregion Create row for CollectionType Data Table

                }

                DataSet dsResult = new DataSet("ds_CCAUpdateResult");
                cls_SCMBL clscmbl = new cls_SCMBL();
                GenericFunction genericFunction = new GenericFunction();
                
                // Database Call to Validate & Insert Route Controls Master
                string errorInSp = string.Empty;
                string Approve, Reject,link;
                string Subject,ToEmail,credittype;
                
                dsResult =ValidateAndInsertCCAMaster(srNotblMasterUploadSummaryLog, dataTableCCAType, errorInSp);
                if (dsResult != null)
                {
                    for (int i = 0; i < dsResult.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToString(dsResult.Tables[0].Rows[i]["CCANo"].ToString()).Contains("CN") == true)
                        {
                            credittype = "Credit";
                        }
                        else if (Convert.ToString(dsResult.Tables[0].Rows[i]["CCANo"].ToString()).Contains("DN") == true)
                        {
                            credittype = "Debit";
                        }
                        else
                        {

                            credittype = "Credit / Debit";
                        }
                        Subject =credittype+ " Notes Request_" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBPrefix"].ToString()) + "-" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBNumber"].ToString()) + "_" + Convert.ToString(dsResult.Tables[0].Rows[i]["InvoiceNumber"].ToString());
                        Approve = "/CCAApprovalStatus.aspx?Approval=Approve&AWBNumber=" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBPrefix"].ToString()) + "-" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBNumber"].ToString()) + "&InvoiceNumber=" + Convert.ToString(dsResult.Tables[0].Rows[i]["InvoiceNumber"].ToString()+"&CCANo="+ Convert.ToString(dsResult.Tables[0].Rows[i]["CCANo"].ToString()));
                        Reject = "/CCAApprovalStatus.aspx?Approval=Reject&AWBNumber=" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBPrefix"].ToString()) + "-" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBNumber"].ToString()) + "&InvoiceNumber=" + Convert.ToString(dsResult.Tables[0].Rows[i]["InvoiceNumber"].ToString() + "&CCANo=" + Convert.ToString(dsResult.Tables[0].Rows[i]["CCANo"].ToString()));
                        link = Convert.ToString(dsResult.Tables[0].Rows[i]["link"].ToString());
                        StringBuilder sb = new StringBuilder();
                        sb.Append("  Dear Approver,<br><br>" + "\n\n  A "+  credittype+" Note approval is requested for AWB with the following details below: \n\n<br><br>");
                        sb.Append("  AWBNo :" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBPrefix"].ToString()) + "-" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBNumber"].ToString())+ "<br>");
                        sb.Append("  Agent Code :" + Convert.ToString(dsResult.Tables[0].Rows[i]["AgentCode"].ToString())+ "<br>");
                        sb.Append("  Agent Name :" + Convert.ToString(dsResult.Tables[0].Rows[i]["AgentName"].ToString())+ "<br>");
                        sb.Append("  Invoice Number :" + Convert.ToString(dsResult.Tables[0].Rows[i]["InvoiceNumber"].ToString())+ "<br>");
                        sb.Append("  Invoice Date :" + Convert.ToString(dsResult.Tables[0].Rows[i]["InvoiceDate"].ToString())+ "<br>");
                        sb.Append("  Date Of AWB Issue :" + Convert.ToString(dsResult.Tables[0].Rows[i]["AWBDate"].ToString()) + "<br><br>");
                        sb.Append("  Origin :" + Convert.ToString(dsResult.Tables[0].Rows[i]["origin"].ToString())+ "<br>");
                        sb.Append("  Destination :" + Convert.ToString(dsResult.Tables[0].Rows[i]["Destination"].ToString())+ "<br>");
                        sb.Append("  Airline Code :" + Convert.ToString(dsResult.Tables[0].Rows[i]["AirlineCode"].ToString())+ "<br>");
                        sb.Append("  CCA Number :" + Convert.ToString(dsResult.Tables[0].Rows[i]["CCANo"].ToString())+ "<br><br>");

                        sb.Append("<table width = \"90%\" border =\"1\" style=\"border-collapse: collapse; \" cellpadding=\"5\">");
                        sb.Append("<tr style = \"font-size: 14px; font-weight: bold;background-color:#fafad2;color:black;text-align:center;\"><th></th><th>Revised / Corrected</th><th>Original / Incorrect</th></tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Weight" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedChargbleWt"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["ChargbleWt"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Currency" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["Currency"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["Currency"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Comm. Code" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["CommodityCodeRev"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["CommodityCodeOrg"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Weight Charges" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedFreightRate"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["FreightRate"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Commission" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedCommission"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["Commission"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Incentive" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedIncentive"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["Incentive"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Net/Net Amount" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedNetAmount"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["NetAmount"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "TDSCommission" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedTDSComm"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["TDSComm"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "STCommission" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedSTComm"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["STComm"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Total Other Charges Due Agent" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedOCDA"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["OCDA"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Total Other Charges Due Airline" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedOCDC"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["OCDC"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Total Payable to Airline (Ex:VAT)" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedTotalPayabletoAirline"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["TotalPayabletoAirline"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Tax" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedServiceTax"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["ServiceTax"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Rate per kg" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedRatePerKg"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RatePerKg"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Total" + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["RevisedTotal"].ToString()) + "</td>");
                        sb.Append("<td>" + Convert.ToString(dsResult.Tables[0].Rows[i]["Total"].ToString()) + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td>" + "Reason For Correction" + "</td>");
                        sb.Append("<td colspan=\"2\" >" + Convert.ToString(dsResult.Tables[0].Rows[i]["ReasonforCorrection"].ToString()) + "</td>");

                        sb.Append("</tr></table><br/> ");

                        sb.Append("\r\n Note:This is a system generated email. Please do not reply. If you were an unintended recipient kindly delete this email.\n\n");
                        sb.Append(" <center>");
                        sb.Append("<table align=\"center\" cellspacing=\"0\" cellpadding=\"0\" width=\"100 % \">");
                        sb.Append(" <tr>");
                        sb.Append("<td align=\"left\" style=\"padding: 10px; \">");
                        sb.Append("<table border=\"0\" class=\"mobile - button\" cellspacing=\"0\" cellpadding=\"0\">");
                        sb.Append("<tr>");
                        sb.Append("<td align=\"left\" bgcolor=\"#91f406\" style=\"background-color: #91f406; margin: auto; max-width: 600px; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; padding: 15px 20px; \" width=\"40%\">");
                        sb.Append("<a href=\""+link+"" + Approve+" \" target=\"_blank\" style=\"16px; font-family: Times New Roman, Times, Georgia, serif; color: #ffffff; font-weight:normal; text-align:center; background-color: #91f406; text-decoration: none; border: none; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; display: inline-block;\">");
                        sb.Append("<span style=\"font - size: 16px; font - family: Times New Roman, Times, Georgia, serif; color: #ffffff; font-weight:normal; line-height:1.5em; text-align:center;\">Approve</span></ a > </td>");
                        sb.Append("<td align=\"center\" style=\" margin: auto; max - width: 50px; -webkit - border - radius: 5px; -moz - border - radius: 5px; border - radius: 5px; padding: 15px 20px; \" width=\"20 % \"> </ td > ");
                        sb.Append("<td align=\"left\" bgcolor=\"#090909\" style=\"background-color:red; margin: auto; max-width: 600px; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; padding: 15px 20px; \" width=\"100%\">");
                        sb.Append("<a href=\"" + link + "" + Reject +"\" target=\"_blank\" style=\"16px; font-family: Times New Roman, Times, Georgia, serif; color: #ffffff; font-weight:normal; text-align:center; background-color: red; text-decoration: none; border: none; -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; display: inline-block;\">");
                        sb.Append("<span style=\"font - size: 16px; font - family: Times New Roman, Times, Georgia, serif; color: #ffffff; font-weight:normal; line-height:1.5em; text-align:center;\">Reject</span></ a > </td></ tr ></ table > ");
                        sb.Append("</td> </ tr ></ table ></ center > ");

                        ToEmail = Convert.ToString(dsResult.Tables[0].Rows[i]["ToMail"].ToString());
                        bool res = clscmbl.addMsgToOutBox(Subject, Convert.ToString(sb), "", ToEmail.Trim(','));

                    }
                    
                }
                return true;

            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableCCAExcelData = null;
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
        public DataSet ValidateAndInsertCCAMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableCCAType,
                                                                                                string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {   new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@CCAType", dataTableCCAType),

                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("USPCCAUpload", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return dataSetResult;
            }
        }



        public String GetNextCCAIDNumber(string strRotationId, int intYear, string strUserName)
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

