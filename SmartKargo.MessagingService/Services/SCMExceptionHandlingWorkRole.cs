using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using QID.DataAccess;

namespace QidWorkerRole
{
    /// <summary>
    /// Below Class used to Save Exception in Azure Table
    /// </summary>
    public  class SCMExceptionHandlingWorkRole
    {
        
        string BlobKey = string.Empty;
        string BlobName = string.Empty;
        public SCMExceptionHandlingWorkRole()
        {

        }

        private  void logexception(ref Exception ex, string parameters)
        {
            try
            {
                string ip = "";
                string username = "";

                string stc = ex.StackTrace;
                string fun, page = "", Line = "";

                if (stc.Contains("in "))
                {
                    try
                    {
                        fun = stc.Substring(0, stc.IndexOf("in "));
                    }
                    catch (Exception)
                    {

                        fun = "";
                    }

                }
                if (stc.Contains(":line") && stc.Contains("in "))
                {
                    try
                    {
                        page = stc.Substring(stc.IndexOf("in "), stc.IndexOf(":line ") - stc.IndexOf("in "));
                    }
                    catch (Exception)
                    {

                        page = "";
                    }

                }
                if (stc.Contains(":line"))
                {
                    try
                    {
                        Line = stc.Substring(stc.IndexOf(":line ") + 6, stc.Length - stc.IndexOf(":line ") - 6);
                    }
                    catch (Exception)
                    {

                        Line = "";
                    }

                }

                WriteToAzureTableErrorLog(page, username, ip, ex.StackTrace, ex.Message, Line, parameters);

            }
            catch (Exception Ex)
            {
                clsLog.WriteLogAzure("Error :", Ex);
            }
        }
        public  void logexception(ref Exception ex)
        {
            logexception(ref ex, "");
        }

        private  bool WriteToAzureTableErrorLog(string url, string username, string ipaddress, string stacktrace, string errormessage, string lineNumber, string parameters)
        {
            CreateAzureTable("ErrorLog");

            CloudTable table = GetAzureTable("ErrorLog");

            // Create a new customer entity.
            ErrorLog el = new ErrorLog("PK" + System.DateTime.Now.ToString("ddMMyyhhmmss"), "RK" + System.DateTime.Now.ToString("ddMMyyhhmmss"));

            el.url = url;
            el.username = username;
            el.ipaddress = ipaddress;
            el.StackTrace = stacktrace;
            el.errormessage = errormessage;
            el.lineNumber = lineNumber;
            el.Parameter = parameters;


            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(el);
            TableResult tr = new TableResult();
            // Execute the insert operation.
            try
            {
                tr = table.Execute(insertOperation);
            }
            catch (Exception)
            {

                throw;
            }


            return true;
        }

        private bool CreateAzureTable(string AzureTableName)
        {
            CloudTable table = GetAzureTable(AzureTableName);

            // Create the table if it doesn't exist.
            return table.CreateIfNotExists();
        }

        private  CloudTable GetAzureTable(string TableName)
        {
           
            Microsoft.WindowsAzure.Storage.Auth.StorageCredentials sc = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(GetStorageName(), GetStorageKey());
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(sc, true);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(TableName);
            return table;
        }

        private string GetStorageKey()
        {
            GenericFunction genericFunction = new GenericFunction();
            return genericFunction.ReadValueFromDb("BlobStorageKey");
        }

        private string GetStorageName()
        {
            GenericFunction genericFunction = new GenericFunction();
            return  genericFunction.ReadValueFromDb("BlobStorageName");

        }
        //public string ReadValueFromDb(string Parameter)
        //{
        //    try
        //    {
        //        string ParameterValue = string.Empty;
        //        SQLServer da = new SQLServer();
        //        string[] QName = new string[] { "PType" };
        //        object[] QValues = new object[] { Parameter };
        //        SqlDbType[] QType = new SqlDbType[] { SqlDbType.VarChar };
        //        ParameterValue = da.GetStringByProcedure("spGetSystemParameter", QName, QValues, QType);
        //        if (ParameterValue == null)
        //            ParameterValue = "";
        //        da = null;
        //        QName = null;
        //        QValues = null;
        //        QType = null;
        //        GC.Collect();
        //        return ParameterValue;                
        //    }
        //    catch (Exception objEx)
        //    {
        //        clsLog.WriteLogAzure(objEx, "General Function", "ReadValueFromDb");
        //    }
        //    return null;
        //}

    }

    public class ErrorLog : TableEntity
    {
        public ErrorLog()
        {

        }
        public ErrorLog(string PK, string RK)
        {
            PartitionKey = PK;
            RowKey = RK;
        }


        public string url { get; set; }
        public string username { get; set; }
        public string ipaddress { get; set; }
        public string StackTrace { get; set; }
        public string errormessage { get; set; }
        public string lineNumber { get; set; }
        public string Parameter { get; set; }

    }


    #region Enum
    /// <summary>
    /// Type of Log.
    /// </summary>
    public enum LogType
    {
        
        InMessage,
        OutMessage,
        AWBOperations,
        MasterSummary,
        MasterDetails,
        Report,
        UserAccess,
        ULD,
        Alerts,
        Interface,
        ULDOperations,
        BillingAudit,
        VendorAudit
    }
    #endregion

    #region Table Entities
    /// <summary>
    /// Entity for AWB Operations Audit Log.
    /// </summary>

    public class AWBOperations : TableEntity
    {
        public long AWBID { get; set; }
        public string AWBPrefix { get; set; }
        public string AWBNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string FlightNo { get; set; }
        public DateTime FlightDate { get; set; }
        public string FlightOrigin { get; set; }
        public string FlightDestination { get; set; }
        public int BookedPcs { get; set; }
        public double BookedWgt { get; set; }
        public string UOM { get; set; }
        public DateTime Createdon { get; set; }
        public string Createdby { get; set; }
        public DateTime Updatedon { get; set; }
        public string Updatedby { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
    /// <summary>
    /// Entry of Incoming Message
    /// </summary>

    public class InBox : TableEntity
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromiD { get; set; }
        public string ToiD { get; set; }
        public DateTime RecievedOn { get; set; }
        public bool IsProcessed { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
      
        public string AWBNumber { get; set; }
        public string FlightNumber { get; set; }
        public string FlightOrigin { get; set; }
        public string FlightDestination { get; set; }
        public DateTime FlightDate { get; set; }
        public string MessageCategory { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// OutGoing Message
    /// </summary>
    public class OutBox : TableEntity
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromiD { get; set; }
        public string ToiD { get; set; }
        public DateTime SentOn { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsHTML { get; set; }
        public bool IsInternal { get; set; }
        public string AWBNumber { get; set; }
        public string FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string FlightOrigin { get; set; }
        public string FlightDestination { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
    #endregion

    public class AuditLog
    {
        
        SCMExceptionHandlingWorkRole scmexception = new SCMExceptionHandlingWorkRole();
        private string StorageType = "azuretable";
        //private string StorageKey = "";
        //private string StorageName = "";

        public AuditLog()
        {
            //GenericFunction gf = new GenericFunction();

            //Get Connection
            //if (StorageType == "azuretable")
            //{
            //    StorageKey = GetStorageKey();
            //    StorageName =GetStorageName();
            //}

            //Get Table Names from Configuration in Dictionary.


            //If Azure Storage then create all Audit Log Tables
            if (StorageType == "azuretable")
            {
           
                CreateAzureTable("InMessage");
                CreateAzureTable("OutMessage");
                CreateAzureTable("AWBOperations");
            }
        }

        #region Common Functions
        public bool SaveLog(LogType logType, string KeyType, string KeyValue, Object objEntity)
        {
            switch (logType.ToString())
            {

                case "InMessage":
                    return SaveLog_InComingMessages(logType.ToString(), KeyType, KeyValue, (InBox)objEntity);
                case "OutMessage":
                    return SaveLog_OutgoingMessages(logType.ToString(), KeyType, KeyValue, (OutBox)objEntity);
                case "AWBOperations":
                    return SaveLog_AWBOperations(logType.ToString(), KeyType, KeyValue, (AWBOperations)objEntity);
                default:
                    break;
            }

            return (true);
        }

       

        private bool CreateAzureTable(string AzureTableName)
        {
            CloudTable table = GetAzureTable(AzureTableName);

            // Create the table if it doesn't exist.
            return table.CreateIfNotExists();
        }

        private CloudTable GetAzureTable(string TableName)
        {
            try
            {
                GenericFunction genericfunction = new GenericFunction();
                Microsoft.WindowsAzure.Storage.Auth.StorageCredentials sc = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(genericfunction.GetStorageName(), genericfunction.GetStorageKey());
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(sc, true);

                // Create the table client.
                Microsoft.WindowsAzure.Storage.Table.CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "people" table.
                CloudTable table = tableClient.GetTableReference(TableName);

                // Create table if does not exist
                table.CreateIfNotExists();

                return table;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Error :", ex);
            }
            return (null);
        }

   

        private TableResult InsertLogInAzureTable(string strTableName, TableEntity objEntity)
        {
            try
            {
                CloudTable table = GetAzureTable(strTableName);

                // Create the TableOperation object that inserts the entity.
                TableOperation insertOperation = TableOperation.Insert(objEntity);
                TableResult tr = new TableResult();

                // Execute the insert operation.
                tr = table.Execute(insertOperation);

                return tr;
            }
            catch (Exception )
            {
                return (null);
            }
        }


    

        #endregion

        #region Save Log





        /// <summary>
        /// Method to save outgoing messages in Azure Table Storage
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="MessageBody"></param>
        /// <param name="fromID"></param>
        /// <param name="toID"></param>
        /// <param name="MessageReceivedDateTime"></param>
        /// <param name="messageStatus"></param>
        /// <param name="messageType"></param>
        /// <param name="BlobName"></param>
        /// <param name="BlobKey"></param>
        /// <param name="AWBNumber"></param>
        /// <param name="FlightNumber"></param>
        /// <param name="FlightOrigin"></param>
        /// <param name="FlightDestination"></param>
        /// <returns></returns>
        private bool SaveLog_OutgoingMessages(string LogType, string KeyType, string KeyValue, OutBox objOutbox)
        {
            try
            {
                if (StorageType == "azuretable")
                {
                    //Set Keys and TimeStamp for Audit Log Entity.
                    objOutbox.PartitionKey = objOutbox.AWBNumber;
                    objOutbox.RowKey = Guid.NewGuid().ToString();
                    objOutbox.Timestamp = objOutbox.SentOn;

                    //Insert log in Table Storage.
                    InsertLogInAzureTable(LogType, objOutbox);
                }

                if (StorageType == "sqltable")
                {
                    //Insert log in SQL Tabele.
                    string procedure = "spInsertMsgToOutbox";
                    SQLServer dtb = new SQLServer();

                    string[] paramname = new string[] { "Subject",
                                                "Body",
                                                "FromEmailID",
                                                "ToEmailID",
                                                "IsHTML",
                                                "isInternal",
                                                "Type",
                                                "CreatedOn" };

                    object[] paramvalue = new object[] {objOutbox.Subject,
                                                objOutbox.Body,
                                                objOutbox.FromiD,
                                                objOutbox.ToiD,
                                                objOutbox.IsHTML,
                                                objOutbox.IsInternal,
                                                objOutbox.Type,
                                                objOutbox.SentOn};

                    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.Bit,
                                                     SqlDbType.Bit,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.DateTime };

                    dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                    return true;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }

            return true;
        }


        /// <summary>
        /// Method to save outgoing messages in Azure Table Storage
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="MessageBody"></param>
        /// <param name="fromID"></param>
        /// <param name="toID"></param>
        /// <param name="MessageReceivedDateTime"></param>
        /// <param name="messageStatus"></param>
        /// <param name="messageType"></param>
        /// <param name="BlobName"></param>
        /// <param name="BlobKey"></param>
        /// <param name="AWBNumber"></param>
        /// <param name="FlightNumber"></param>
        /// <param name="FlightOrigin"></param>
        /// <param name="FlightDestination"></param>
        /// <returns></returns>
        private bool SaveLog_InComingMessages(string LogType, string KeyType, string KeyValue, InBox inBox)
        {
            try
            {
                if (StorageType == "azuretable")
                {
                    //Set Keys and TimeStamp for Audit Log Entity.
                    if(inBox.AWBNumber!="")
                    inBox.PartitionKey = inBox.AWBNumber;
                    else
                        inBox.PartitionKey = inBox.FlightNumber;
                    inBox.RowKey = Guid.NewGuid().ToString();
                    inBox.Timestamp = inBox.RecievedOn;

                    //Insert log in Table Storage.
                    InsertLogInAzureTable(LogType, inBox);
                }

                //if (StorageType == "sqltable")
                //{
                //    //Insert log in SQL Tabele.
                //    string procedure = "spInsertMsgToOutbox";
                //    SQLServer dtb = new SQLServer();

                //    string[] paramname = new string[] { "Subject",
                //                                "Body",
                //                                "FromEmailID",
                //                                "ToEmailID",
                //                                "IsHTML",
                //                                "isInternal",
                //                                "Type",
                //                                "CreatedOn" };

                //    object[] paramvalue = new object[] {inBox.Subject,
                //                                inBox.Body,
                //                                inBox.FromiD,
                //                                inBox.ToiD,
                //                                inBox.IsHTML,
                //                                inBox.IsInternal,
                //                                inBox.Type,
                //                                inBox.SentOn};

                //    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.Bit,
                //                                     SqlDbType.Bit,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime };

                //    dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                    return true;
                
            }
            catch (Exception ex)
            {
             clsLog.WriteLogAzure(ex);
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Saves AWB Ops audit log
        /// </summary>
        /// <param name="LogType"></param>
        /// <param name="KeyType"></param>
        /// <param name="KeyValue"></param>
        /// <param name="objAWB"></param>
        /// <returns></returns>
        private bool SaveLog_AWBOperations(string LogType, string KeyType, string KeyValue, AWBOperations objAWB)
        {
            try
            {
                if (StorageType == "azuretable")
                {
                    //Set Keys and TimeStamp for Audit Log Entity.
                    objAWB.PartitionKey = objAWB.AWBPrefix + "-" + objAWB.AWBNumber;
                    objAWB.RowKey = Guid.NewGuid().ToString();
                    objAWB.Timestamp = objAWB.Createdon;

                    //Insert log in Table Storage.
                    InsertLogInAzureTable(LogType, objAWB);
                }

                if (StorageType == "sqltable")
                {
                    //Insert log in SQL Tabele.
                    //BAL.MasterAuditBAL objAudit = new BAL.MasterAuditBAL();
                    //return objAudit.AddAWBAuditLog(objAWB.AWBPrefix, objAWB.AWBNumber, objAWB.Origin, objAWB.Destination, objAWB.BookedPcs.ToString(),
                    //                objAWB.BookedWgt.ToString(), objAWB.FlightNo, objAWB.FlightDate, objAWB.FlightOrigin, objAWB.FlightDestination,
                    //                objAWB.Action, objAWB.Message, objAWB.Description, objAWB.Updatedby, objAWB.Updatedon.ToString(), false);

                }
            }
            catch (Exception)
            {
                return (false);
            }
            return (true);
        }


    }

    public class MasterSummary : TableEntity
    {
        public string SerialNumber { get; set; }
        public string MasterKey { get; set; }
        public string MasterValue { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string MasterDetails { get; set; }
    }
}
