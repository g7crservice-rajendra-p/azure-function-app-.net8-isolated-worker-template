using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using QID.DataAccess;
using System.Data.SqlClient;

namespace QidWorkerRole.UploadMasters.Agent
{
    public class UploadAgentMasterUpdate
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        public Boolean UpdateAgent(DataSet dataSetFileData)
        {
            try
            {
                string FilePath = "";
                
                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowDataSetFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowDataSetFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowDataSetFileData["FileName"]), Convert.ToString(dataRowDataSetFileData["ContainerName"]), "AgentUpdate", out FilePath))
                        {
                            ProcessFile(Convert.ToInt32(dataRowDataSetFileData["SrNo"]), FilePath);
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowDataSetFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowDataSetFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                            continue;
                        }

                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowDataSetFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowDataSetFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
            }
            return false;
        }

        public Boolean ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = new DataTable();
            try
            {
                dataTable = ConvertJsonToDataTable(filepath);
                dataSet = UpdateAgent(dataTable, srNotblMasterUploadSummaryLog);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(dataSet.GetXml());
                MemoryStream memoryStream = new MemoryStream();
                StreamWriter streamWriter = new StreamWriter(memoryStream);
                streamWriter.Write(stringBuilder.ToString());
                streamWriter.Flush();

                GenericFunction genericFunction = new QidWorkerRole.GenericFunction();
                genericFunction.UploadToBlob(memoryStream, "AgentUpdate.xls", "AgentUpdate");
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure(exception);
            }
            return true;
        }
        
        public Boolean ProcessFile(int srNotblMasterUploadSummaryLog, byte[] downloadStream)
        {
            try
            {
                UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(downloadStream, 0, downloadStream.Length);
                memoryStream.Position = 0;
                StreamReader streamReader = new StreamReader(memoryStream);
                int count = 0;
                string Line = "";
                DataTable dataTable = CreateTemplateDataTable();
                Line = streamReader.ReadLine();
                count = 1;
                do
                {
                    string s = streamReader.ReadLine();
                    
                    dataTable.Rows.Add(count++, s.Substring(0, 11),
                                                s.Substring(11, 30),
                                                s.Substring(41, 30),
                                                s.Substring(71, 30),
                                                s.Substring(101, 30),
                                                s.Substring(131, 30),
                                                s.Substring(161, 2),
                                                s.Substring(163, 5),
                                                s.Substring(168, 1),
                                                s.Substring(169, 4),
                                                s.Substring(173, 2),
                                                s.Substring(175, 7),
                                                s.Substring(182, 7),
                                                s.Substring(189, 1),
                                                s.Substring(190, 14),
                                                s.Substring(205, 7));
                }
                while (streamReader.Peek() != -1);
                
                streamReader.Close();
                dataTable.EndInit();
                dataTable.AcceptChanges();
                streamReader.Close();

                DataSet dataSet = new DataSet();
                dataSet = UpdateAgent(dataTable, srNotblMasterUploadSummaryLog);

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(dataSet.GetXml());
                MemoryStream memoryStreamNew = new MemoryStream();
                StreamWriter streamWriter = new StreamWriter(memoryStreamNew);
                streamWriter.Write(stringBuilder.ToString());
                streamWriter.Flush();

                GenericFunction genericFunction = new QidWorkerRole.GenericFunction();
                genericFunction.UploadToBlob(memoryStreamNew, "SSIMTest.xls", "AgentUpdate");
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure(exception);
            }
            return true;
        }

        private DataTable ConvertJsonToDataTable(string FilePath)
        {
            DataTable dataTable = new DataTable();
            try
            {
                string jsonString = File.ReadAllText(FilePath);

                if (!String.IsNullOrWhiteSpace(jsonString))
                {
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonString);
                    dataTable.Columns.Add("Account", typeof(string));
                    dataTable.Columns.Add("CompanyName", typeof(string));
                    dataTable.Columns.Add("Address1", typeof(string));
                    dataTable.Columns.Add("Address2", typeof(string));
                    dataTable.Columns.Add("Address3", typeof(string));
                    dataTable.Columns.Add("Address4", typeof(string));
                    dataTable.Columns.Add("State", typeof(string));
                    dataTable.Columns.Add("Zip", typeof(string));
                    dataTable.Columns.Add("ZipWithoutExtension", typeof(string));
                    dataTable.Columns.Add("Clerk", typeof(string));
                    dataTable.Columns.Add("DateOpened", typeof(string));
                    dataTable.Columns.Add("DateChanged", typeof(string));
                    dataTable.Columns.Add("ValidataCode", typeof(string));
                    dataTable.Columns.Add("CreditLimit", typeof(string));
                    dataTable.Columns.Add("City", typeof(string));
                    dataTable.Columns.Add("Arctic", typeof(string));
                    dataTable.Columns.Add("Country", typeof(string));
                    dataTable.Columns.Add("SanitizedStreetAddress", typeof(string));

                    foreach (var cou in dynObj)
                    {
                        string cou1 = Convert.ToString(cou);
                        string[] RowData = Regex.Split(cou1.Replace("{", "").Replace("}", ""), ",");
                        DataRow nr = dataTable.NewRow();
                        foreach (string rowData in RowData)
                        {
                            try
                            {
                                int idx = rowData.IndexOf(":");
                                string RowColumns = rowData.Substring
                                (0, idx - 1).Replace("\"", "").Trim();
                                string RowDataString = rowData.Substring(idx + 1).Replace("\"", "").Trim();
                                nr[RowColumns] = RowDataString;
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                        dataTable.Rows.Add(nr);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return dataTable;
        }

        private DataSet UpdateAgent(DataTable agentTableData, int srNotblMasterUploadSummaryLog)
        {
            DataSet dsUpdateAgent = new DataSet();
            try
            {
                SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParams = new SqlParameter[] { new SqlParameter("@AgentTableData", agentTableData),
                                                                new SqlParameter("@MasterLogId", srNotblMasterUploadSummaryLog)
                                                              };

                dsUpdateAgent = sqlServer.SelectRecords("uspUpdateAgentFromFile", sqlParams);
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure(exception);
            }
            return dsUpdateAgent;
        }

        private DataTable CreateTemplateDataTable()
        {
            DataTable dt = new DataTable();

            try
            {
                dt.Columns.AddRange(new DataColumn[17] { new DataColumn("Id", typeof(int)),
                                                         new DataColumn("CustomerCode", typeof(string)),
                                                         new DataColumn("AgentName",typeof(string)) ,
                                                         new DataColumn("BillingAddress1", typeof(string)),
                                                         new DataColumn("BillingAddress2", typeof(string)),
                                                         new DataColumn("BillingAddress3", typeof(string)),
                                                         new DataColumn("BillingCity", typeof(string)),
                                                         new DataColumn("BillingState", typeof(string)),
                                                         new DataColumn("BillingZIPCode", typeof(string)),
                                                         new DataColumn("BillingZIPDash", typeof(string)),
                                                         new DataColumn("BillingZIPCode2", typeof(string)),
                                                         new DataColumn("ClerkIdentification", typeof(string)),
                                                         new DataColumn("ValidFrom", typeof(string)),
                                                         new DataColumn("UpdatedOn", typeof(string)),
                                                         new DataColumn("IsActive", typeof(string)),
                                                         new DataColumn("CreditLimit", typeof(string)),
                                                         new DataColumn("AcctLCIDate", typeof(string))
                                                       });
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure(exception);
                return null;
            }
            return dt;
        }
    }
}
