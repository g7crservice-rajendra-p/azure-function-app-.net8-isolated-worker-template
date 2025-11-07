using System;
using System.Collections.Generic;
using QidWorkerRole.SIS.Model.ISValidationReport;

using System.Reflection;
using System.Globalization;
using System.IO;
using System.Data;
using System.Linq;

namespace QidWorkerRole.SIS.FileHandling.ISValidationReport
{
    public class ISValidationReportReader
    {
        
        public List<ISValidationSummaryReport> ReadISValidationSummaryReportR1(string iSValidationSummaryReportR1FilePath)
        {
            List<ISValidationSummaryReport> listISValidationSummaryReport = new List<ISValidationSummaryReport>();

            try
            {
                DataTable dtCsv = ReadCsvFileToDataTable(iSValidationSummaryReportR1FilePath);

                if (dtCsv.Rows.Count >= 1)
                {
                    for (int i = 0; i < dtCsv.Rows.Count; i++)
                    {
                        ISValidationSummaryReport iSValidationSummaryReport = new ISValidationSummaryReport();

                        iSValidationSummaryReport.SerialNo = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Serial No"])) ? 0 : Convert.ToInt32(dtCsv.Rows[i]["Serial No"]);
                        iSValidationSummaryReport.BillingEntityCode = Convert.ToString(dtCsv.Rows[i]["Billing Entity Code"]);
                        iSValidationSummaryReport.BillingYear = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Clearance Month"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Clearance Month"]).Substring(0, 4));
                        iSValidationSummaryReport.BillingMonth = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Clearance Month"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Clearance Month"]).Substring(4, 2));
                        iSValidationSummaryReport.BillingPeriod = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Period Number"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Period Number"]));
                        iSValidationSummaryReport.BillingCategory = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Billing Category"])) ? 'C' : Convert.ToChar(Convert.ToString(dtCsv.Rows[i]["Billing Category"]));
                        iSValidationSummaryReport.BillingFileName = Convert.ToString(dtCsv.Rows[i]["Billing File Name"]);
                        iSValidationSummaryReport.BillingFileSubmissionDate = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Billing File Submission Date"])) ? DateTime.UtcNow : DateTime.ParseExact(Convert.ToString(dtCsv.Rows[i]["Billing File Submission Date"]), "yyyyMMdd", CultureInfo.InvariantCulture);
                        iSValidationSummaryReport.SubmissionFormat = Convert.ToString(dtCsv.Rows[i]["Submission Format"]);
                        iSValidationSummaryReport.BilledEntityCode = Convert.ToString(dtCsv.Rows[i]["Billed Entity Code"]);
                        iSValidationSummaryReport.InvoiceNumber = Convert.ToString(dtCsv.Rows[i]["Invoice Number"]);
                        iSValidationSummaryReport.CurrencyOfBilling = Convert.ToString(dtCsv.Rows[i]["Currency Of Billing"]);
                        iSValidationSummaryReport.InvoiceAmountInBillingCurrency = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Invoice Amount In BillingCurrency"])) ? 0 : Convert.ToDecimal(Convert.ToString(dtCsv.Rows[i]["Invoice Amount In BillingCurrency"]));
                        iSValidationSummaryReport.InvoiceStatus = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Invoice Status"])) ? 'Z' : Convert.ToChar(dtCsv.Rows[i]["Invoice Status"]);
                        iSValidationSummaryReport.ErrorAtInvoiceLevel = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Error At Invoice Level"])) ? 'Y' : Convert.ToChar(dtCsv.Rows[i]["Error At Invoice Level"]);
                        iSValidationSummaryReport.TotalNoOfBillingRecords = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Total number of billing records"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Total number of billing records"]));
                        iSValidationSummaryReport.TotalNoOfSuccessfullyValidatedRecords = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Records successfully validated"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Records successfully validated"]));
                        iSValidationSummaryReport.TotalNoOfRecordsInValidationError = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Records in Validation Error"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Records in Validation Error"]));

                        listISValidationSummaryReport.Add(iSValidationSummaryReport);
                    }
                }

                //string r1CsvFileData = File.ReadAllText(iSValidationSummaryReportR1FilePath);

                //if (!string.IsNullOrWhiteSpace(r1CsvFileData))
                //{
                //    foreach (string r1CsvFileDataRows in r1CsvFileData.Split('\n'))
                //    {
                //        if (!string.IsNullOrEmpty(r1CsvFileDataRows) && !r1CsvFileDataRows.ToUpper().Contains("SERIAL NO"))
                //        {
                //            string[] r1CsvFileDataRowColumns = r1CsvFileDataRows.Split(',');

                //            ISValidationSummaryReport iSValidationSummaryReport = new ISValidationSummaryReport();
                //            iSValidationSummaryReport.SerialNo = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[0]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[0]);
                //            iSValidationSummaryReport.BillingEntityCode = r1CsvFileDataRowColumns[1];
                //            iSValidationSummaryReport.BillingYear = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[2]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[2].Substring(0, 4));
                //            iSValidationSummaryReport.BillingMonth = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[2]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[2].Substring(4, 2));
                //            iSValidationSummaryReport.BillingPeriod = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[3]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[3]);
                //            iSValidationSummaryReport.BillingCategory = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[4]) ? 'C' : Convert.ToChar(r1CsvFileDataRowColumns[4]);
                //            iSValidationSummaryReport.BillingFileName = r1CsvFileDataRowColumns[5];
                //            iSValidationSummaryReport.BillingFileSubmissionDate = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[6])
                //                                                                  ? DateTime.UtcNow
                //                                                                  : DateTime.ParseExact(r1CsvFileDataRowColumns[6], "yyyyMMdd", CultureInfo.InvariantCulture);
                //            iSValidationSummaryReport.SubmissionFormat = r1CsvFileDataRowColumns[7];
                //            iSValidationSummaryReport.BilledEntityCode = r1CsvFileDataRowColumns[8];
                //            iSValidationSummaryReport.InvoiceNumber = r1CsvFileDataRowColumns[9];
                //            iSValidationSummaryReport.CurrencyOfBilling = r1CsvFileDataRowColumns[10];
                //            iSValidationSummaryReport.InvoiceAmountInBillingCurrency = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[11]) ? 0 : Convert.ToDecimal(r1CsvFileDataRowColumns[11]);
                //            iSValidationSummaryReport.InvoiceStatus = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[12]) ? 'Z' : Convert.ToChar(r1CsvFileDataRowColumns[12]);
                //            iSValidationSummaryReport.ErrorAtInvoiceLevel = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[13]) ? 'Y' : Convert.ToChar(r1CsvFileDataRowColumns[13]);
                //            iSValidationSummaryReport.TotalNoOfBillingRecords = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[14]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[14]);
                //            iSValidationSummaryReport.TotalNoOfSuccessfullyValidatedRecords = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[15]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[15]);
                //            iSValidationSummaryReport.TotalNoOfRecordsInValidationError = string.IsNullOrWhiteSpace(r1CsvFileDataRowColumns[16]) ? 0 : Convert.ToInt32(r1CsvFileDataRowColumns[16]);

                //            listISValidationSummaryReport.Add(iSValidationSummaryReport);
                //        }
                //    }
                //}

                return listISValidationSummaryReport;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadISValidationSummaryReportR1. ", exception);
                return listISValidationSummaryReport;
            }
        }

        public List<ISValidationDetailErrorReport> ReadISValidationDetailErrorReportR2(string iSValidationDetailErrorReportR2FilePath)
        {
            List<ISValidationDetailErrorReport> listISValidationDetailErrorReport = new List<ISValidationDetailErrorReport>();

            try
            {
                DataTable dtCsv = ReadCsvFileToDataTable(iSValidationDetailErrorReportR2FilePath);

                if(dtCsv.Rows.Count >= 1)
                {
                    for (int i = 0; i < dtCsv.Rows.Count; i++)
                    {
                        ISValidationDetailErrorReport iSValidationDetailErrorReport = new ISValidationDetailErrorReport();

                        iSValidationDetailErrorReport.SerialNo = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Serial No"])) ? 0 : Convert.ToInt32(dtCsv.Rows[i]["Serial No"]);
                        iSValidationDetailErrorReport.BillingEntityCode = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Billing Entity Code"])) ? "" : Convert.ToString(dtCsv.Rows[i]["Billing Entity Code"]);
                        iSValidationDetailErrorReport.BillingYear = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Clearance Month"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Clearance Month"]).Substring(0, 4));
                        iSValidationDetailErrorReport.BillingMonth = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Clearance Month"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Clearance Month"]).Substring(4, 2));
                        iSValidationDetailErrorReport.BillingPeriod = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Period Number"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["Period Number"]));
                        iSValidationDetailErrorReport.BillingCategory = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Billing Category"])) ? 'C' : Convert.ToChar(Convert.ToString(dtCsv.Rows[i]["Billing Category"]));
                        iSValidationDetailErrorReport.BillingFileName = Convert.ToString(dtCsv.Rows[i]["Billing File Name"]);
                        iSValidationDetailErrorReport.BillingFileSubmissionDate = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Billing File Submission Date"])) ? DateTime.UtcNow : DateTime.ParseExact(Convert.ToString(dtCsv.Rows[i]["Billing File Submission Date"]), "yyyyMMdd", CultureInfo.InvariantCulture);
                        iSValidationDetailErrorReport.SubmissionFormat = Convert.ToString(dtCsv.Rows[i]["Submission Format"]);
                        iSValidationDetailErrorReport.BilledEntityCode = Convert.ToString(dtCsv.Rows[i]["Billed Entity Code"]);
                        iSValidationDetailErrorReport.InvoiceNumber = Convert.ToString(dtCsv.Rows[i]["Invoice Number"]);
                        iSValidationDetailErrorReport.CGOBillingCode = Convert.ToString(dtCsv.Rows[i]["CGO Billing Code"]);
                        iSValidationDetailErrorReport.CGOBlank = Convert.ToString(dtCsv.Rows[i]["CGO (Blank)"]);
                        iSValidationDetailErrorReport.CGOBatchNumber = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["CGO Batch Number"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["CGO Batch Number"]));
                        iSValidationDetailErrorReport.CGOSeqNumber = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["CGO Seq Number"])) ? 0 : Convert.ToInt32(Convert.ToString(dtCsv.Rows[i]["CGO Seq Number"]));
                        iSValidationDetailErrorReport.MainDocNo = Convert.ToString(dtCsv.Rows[i]["Main Doc No"]);
                        iSValidationDetailErrorReport.LinkedDocNo = Convert.ToString(dtCsv.Rows[i]["Linked Doc No"]);
                        iSValidationDetailErrorReport.ErrorCode = Convert.ToString(dtCsv.Rows[i]["Error Code"]);
                        iSValidationDetailErrorReport.ErrorLevel = Convert.ToString(dtCsv.Rows[i]["Error Level"]);
                        iSValidationDetailErrorReport.FieldName = Convert.ToString(dtCsv.Rows[i]["Field Name"]);
                        iSValidationDetailErrorReport.FieldValue = Convert.ToString(dtCsv.Rows[i]["Field Value"]);
                        iSValidationDetailErrorReport.ErrorDescription = Convert.ToString(dtCsv.Rows[i]["Error Description"]).Length > 200 ? Convert.ToString(dtCsv.Rows[i]["Error Description"]).Substring(0, 199) : Convert.ToString(dtCsv.Rows[i]["Error Description"]);
                        iSValidationDetailErrorReport.ErrorStatus = string.IsNullOrWhiteSpace(Convert.ToString(dtCsv.Rows[i]["Error Status"])) ? 'Z' : Convert.ToString(dtCsv.Rows[i]["Error Status"]).Length > 1 ? Convert.ToChar(Convert.ToString(dtCsv.Rows[i]["Error Status"]).Substring(0, 1)) : Convert.ToChar(Convert.ToString(dtCsv.Rows[i]["Error Status"]));
                        
                        listISValidationDetailErrorReport.Add(iSValidationDetailErrorReport);
                    }
                }

                //string r2CsvFileData = File.ReadAllText(iSValidationDetailErrorReportR2FilePath);

                //if (!string.IsNullOrWhiteSpace(r2CsvFileData))
                //{
                //    foreach (string r2CsvFileDataRows in r2CsvFileData.Split('\n'))
                //    {
                //        if (!string.IsNullOrEmpty(r2CsvFileDataRows) && !r2CsvFileDataRows.ToUpper().Contains("SERIAL NO"))
                //        {
                //            string[] r2CsvFileDataRowColumns = r2CsvFileDataRows.Split(',');
                //            ISValidationDetailErrorReport iSValidationDetailErrorReport = new ISValidationDetailErrorReport();

                //            iSValidationDetailErrorReport.SerialNo = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[0]) ? 0 : Convert.ToInt32(r2CsvFileDataRowColumns[0]);
                //            iSValidationDetailErrorReport.BillingEntityCode = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[1]) ? "" : r2CsvFileDataRowColumns[1];
                //            iSValidationDetailErrorReport.BillingYear = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[2]) ? 0 : Convert.ToInt32(r2CsvFileDataRowColumns[2].Substring(0, 4));
                //            iSValidationDetailErrorReport.BillingMonth = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[2]) ? 0 : Convert.ToInt32(r2CsvFileDataRowColumns[2].Substring(4, 2));
                //            iSValidationDetailErrorReport.BillingPeriod = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[3]) ? 0 : Convert.ToInt32(r2CsvFileDataRowColumns[3]);
                //            iSValidationDetailErrorReport.BillingCategory = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[4]) ? 'C' : Convert.ToChar(r2CsvFileDataRowColumns[4]);
                //            iSValidationDetailErrorReport.BillingFileName = r2CsvFileDataRowColumns[5];
                //            iSValidationDetailErrorReport.BillingFileSubmissionDate = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[6])
                //                                                                  ? DateTime.UtcNow
                //                                                                  : DateTime.ParseExact(r2CsvFileDataRowColumns[6], "yyyyMMdd", CultureInfo.InvariantCulture);
                //            iSValidationDetailErrorReport.SubmissionFormat = r2CsvFileDataRowColumns[7];
                //            iSValidationDetailErrorReport.BilledEntityCode = r2CsvFileDataRowColumns[8];
                //            iSValidationDetailErrorReport.InvoiceNumber = r2CsvFileDataRowColumns[9];
                //            iSValidationDetailErrorReport.CGOBillingCode = r2CsvFileDataRowColumns[10];
                //            iSValidationDetailErrorReport.CGOBillingCode = r2CsvFileDataRowColumns[11];
                //            iSValidationDetailErrorReport.CGOBatchNumber = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[12]) ? 0 : Convert.ToInt32(r2CsvFileDataRowColumns[12]);
                //            iSValidationDetailErrorReport.CGOSeqNumber = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[13]) ? 0 : Convert.ToInt32(r2CsvFileDataRowColumns[13]);
                //            iSValidationDetailErrorReport.MainDocNo = r2CsvFileDataRowColumns[14];
                //            iSValidationDetailErrorReport.LinkedDocNo = r2CsvFileDataRowColumns[15];
                //            iSValidationDetailErrorReport.ErrorCode = r2CsvFileDataRowColumns[16];
                //            iSValidationDetailErrorReport.ErrorLevel = r2CsvFileDataRowColumns[17];
                //            iSValidationDetailErrorReport.FieldName = r2CsvFileDataRowColumns[18];
                //            iSValidationDetailErrorReport.FieldValue = r2CsvFileDataRowColumns[19];
                //            iSValidationDetailErrorReport.ErrorDescription = r2CsvFileDataRowColumns[20].Length > 200 ? r2CsvFileDataRowColumns[20].Substring(0,199) : r2CsvFileDataRowColumns[20];
                //            iSValidationDetailErrorReport.ErrorStatus = string.IsNullOrWhiteSpace(r2CsvFileDataRowColumns[21]) ? 'Z' : r2CsvFileDataRowColumns[21].Length > 1 ? Convert.ToChar(r2CsvFileDataRowColumns[21].Substring(0, 1)) : Convert.ToChar(r2CsvFileDataRowColumns[21]);

                //            listISValidationDetailErrorReport.Add(iSValidationDetailErrorReport);
                //        }
                //    }
                //}

                return listISValidationDetailErrorReport;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadISValidationDetailErrorReportR2.", exception);
                return listISValidationDetailErrorReport;
            }
        }

        public DataTable ReadCsvFileToDataTable(string FileSaveWithPath)
        {
            DataTable dtCsv = new DataTable();
            try
            {                
                string Fulltext;
                using (StreamReader sr = new StreamReader(FileSaveWithPath))
                {
                    while (!sr.EndOfStream)
                    {
                        Fulltext = sr.ReadToEnd().ToString(); //read full file text  
                        string[] rows = Fulltext.Split('\n'); //split full file text into rows  
                        for (int i = 0; i < rows.Count() - 1; i++)
                        {
                            string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                            {
                                if (i == 0)
                                {
                                    for (int j = 0; j < rowValues.Count(); j++)
                                    {
                                        dtCsv.Columns.Add(rowValues[j].ToString().Replace("\r", "")); //add headers  
                                    }
                                }
                                else
                                {
                                    DataRow dr = dtCsv.NewRow();
                                    for (int k = 0; k < rowValues.Count(); k++)
                                    {
                                        dr[k] = rowValues[k].ToString();
                                    }
                                    dtCsv.Rows.Add(dr); //add other rows  
                                }
                            }
                        }
                    }
                }
                return dtCsv;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadCsvFileToDataTable", exception);
                return dtCsv;
            }
        }
    }
}