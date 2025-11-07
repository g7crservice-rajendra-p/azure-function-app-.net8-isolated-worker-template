using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole
{
    public class RapidInterface
    {
        private string SFTPFingerPrint = string.Empty;

        private string StpFolerParth = string.Empty;

        private string SFTPPortNumber = string.Empty;

        private string GHAOutFolderPath = string.Empty;

        private string ppkFileName = string.Empty;

        private string ppkLocalFilePath = string.Empty;

        public string SFTPAddress = string.Empty;

        public string SFTPUserName = string.Empty;

        public string SFTPPassWord = string.Empty;

        private string BlobContainerName = "rapid";

        private GenericFunction genericFunction = new GenericFunction();

        private FTP objftp = new FTP();

        private int portNumber = 0;

        private DataSet dsRapid = new DataSet();

        private DataRow drMsg = null;

        private DataSet dsCTMData = new DataSet();

        private DataSet dsCTMFileName = new DataSet();

        private StringBuilder sbCTM = new StringBuilder();

        private string filenameCTM = "";

        public RapidInterface()
        {
            dsRapid = GetRapidConfiguration();
            if (dsRapid != null && dsRapid.Tables.Count > 0)
            {
                drMsg = dsRapid.Tables[0].Rows[0];
                SFTPAddress = drMsg["FTPID"].ToString();
                SFTPUserName = drMsg["FTPUserName"].ToString();
                SFTPPassWord = drMsg["FTPPassword"].ToString();
                ppkFileName = drMsg["PPKFileName"].ToString().Trim();
                SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                StpFolerParth = drMsg["RemotePath"].ToString();
                SFTPPortNumber = drMsg["PortNumber"].ToString();
                GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
            }
            if (ppkFileName != string.Empty)
            {
                ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
            }
            portNumber = Convert.ToInt32(SFTPPortNumber);
        }

        public void RapidInterfaceData(DateTime dtFromDate, DateTime dtToDate, DateTime NextProcessDateTime)
        {
            clsLog.WriteLogAzure("In RapidInterfaceData fnction");
            InsertRapidInterfaceData(dtFromDate, dtToDate);
            InsertRapidCTMTransaction(dtFromDate, dtToDate);
            GenerateRapidInterface(dtFromDate, dtToDate);
        }

        public void InsertRapidInterfaceData(DateTime dtFromDate, DateTime dtToDate)
        {
            DataSet dataSet = new DataSet();
            DataSet dataSet2 = new DataSet();
            DataSet dataSet3 = new DataSet();
            StringBuilder stringBuilder = new StringBuilder();
            string text = "";
            try
            {
                SQLServer sQLServer = new SQLServer();
                dataSet2 = sQLServer.SelectRecords("uspRapidInterfaceVerifiedAWBInsert", new string[3]
                {
                "FromDate",
                "Todate",
                "UpdatedOn"
                }, new object[3]
                {
                dtFromDate,
                dtToDate,
                DateTime.Now
                }, new SqlDbType[3]
                {
                SqlDbType.DateTime,
                SqlDbType.DateTime,
                SqlDbType.DateTime
                });
                if (dataSet2 != null)
                {
                    if (dataSet2.Tables.Count > 0 && dataSet2.Tables[0] != null && dataSet2.Tables[0].Rows.Count > 0)
                    {
                        DataRow dataRow = dataSet2.Tables[0].Rows[0];
                        if (dataRow != null && !string.IsNullOrEmpty(dataRow["FileName"].ToString()))
                        {
                            text = dataRow["FileName"].ToString();
                            dataSet3 = GetRapidInterfaceData(text);
                            if (dataSet3 != null && dataSet3.Tables.Count > 0)
                            {
                                StringBuilder stringBuilder2 = new StringBuilder();
                                foreach (DataRow row in dataSet3.Tables[0].Rows)
                                {
                                    for (int i = 0; i < dataSet3.Tables[0].Columns.Count; i++)
                                    {
                                        stringBuilder2.Append(row[i].ToString());
                                        stringBuilder2.Append((i == dataSet3.Tables[0].Columns.Count - 1) ? "\n" : "");
                                    }
                                    stringBuilder2.AppendLine();
                                }
                                foreach (DataRow row2 in dataSet3.Tables[1].Rows)
                                {
                                    for (int i = 0; i < dataSet3.Tables[1].Columns.Count; i++)
                                    {
                                        stringBuilder2.Append(row2[i].ToString());
                                        stringBuilder2.Append((i == dataSet3.Tables[1].Columns.Count - 1) ? "\n" : "");
                                    }
                                    stringBuilder2.AppendLine();
                                    DataRow[] array = dataSet3.Tables[2].Select("AWBNumberWithCheckDigit=" + row2["AWBNumberWithCheckDigit"].ToString());
                                    DataRow[] array2 = array;
                                    foreach (DataRow dataRow3 in array2)
                                    {
                                        for (int i = 0; i < dataSet3.Tables[2].Columns.Count; i++)
                                        {
                                            stringBuilder2.Append(dataRow3[i].ToString());
                                            stringBuilder2.Append((i == dataSet3.Tables[2].Columns.Count - 1) ? "\n" : "");
                                        }
                                        stringBuilder2.AppendLine();
                                    }
                                    DataRow[] array3 = dataSet3.Tables[3].Select("AWBNumberWithCheckDigit=" + row2["AWBNumberWithCheckDigit"].ToString());
                                    array2 = array3;
                                    foreach (DataRow dataRow4 in array2)
                                    {
                                        for (int i = 0; i < dataSet3.Tables[3].Columns.Count; i++)
                                        {
                                            stringBuilder2.Append(dataRow4[i].ToString());
                                            stringBuilder2.Append((i == dataSet3.Tables[3].Columns.Count - 1) ? "\n" : "");
                                        }
                                        stringBuilder2.AppendLine();
                                    }
                                }
                                foreach (DataRow row3 in dataSet3.Tables[4].Rows)
                                {
                                    for (int i = 0; i < dataSet3.Tables[4].Columns.Count; i++)
                                    {
                                        stringBuilder2.Append(row3[i].ToString());
                                        stringBuilder2.Append((i == dataSet3.Tables[4].Columns.Count - 1) ? "\n" : "");
                                    }
                                    stringBuilder2.AppendLine();
                                }
                                stringBuilder = stringBuilder2;
                                if (stringBuilder != null)
                                {
                                    MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString()));
                                    string fileUrl = genericFunction.UploadToBlob(stream, DateTime.Now.ToString("MMMM-dd-yyyy_H-mm-ss") + "/" + text + ".txt", BlobContainerName);
                                    SetInterfaceDetails("RAPID", text, fileUrl, "OmanAdmin", DateTime.Now);
                                    objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, stringBuilder.ToString(), text, ".txt", StpFolerParth, portNumber, ppkLocalFilePath);
                                }
                                else
                                {
                                    clsLog.WriteLogAzure("No Data available for AWB file");
                                }
                            }
                            else
                            {
                                clsLog.WriteLogAzure("No Data available for AWB file");
                            }
                        }
                    }
                }
                else
                {
                    clsLog.WriteLogAzure("No Data available for AWB file");
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        public DataSet GetRapidInterfaceData(string strFileName)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                string[] array = new string[1];
                object[] array2 = new object[1];
                SqlDbType[] array3 = new SqlDbType[1];
                array[0] = "BatchIDForTxtFile";
                array3[0] = SqlDbType.VarChar;
                array2[0] = strFileName;
                dataSet = sQLServer.SelectRecords("SP_RapidInterfaceSelect", array, array2, array3);
                if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
                {
                    return dataSet;
                }
                return dataSet;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return dataSet;
            }
        }

        public DataSet InsertRapidCTMTransaction(DateTime dtFromDate, DateTime dtToDate)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                dsCTMFileName = sQLServer.SelectRecords("SP_RapidCTMInsert", new string[2]
                {
                "FromDate",
                "Todate"
                }, new object[2]
                {
                dtFromDate,
                dtToDate
                }, new SqlDbType[2]
                {
                SqlDbType.DateTime,
                SqlDbType.DateTime
                });
                if (dsCTMFileName != null)
                {
                    if (dsCTMFileName.Tables.Count > 0 && dsCTMFileName.Tables[0] != null && dsCTMFileName.Tables[0].Rows.Count > 0)
                    {
                        DataRow dataRow = dsCTMFileName.Tables[0].Rows[0];
                        if (dataRow != null)
                        {
                            if (!string.IsNullOrEmpty(dataRow["FileName"].ToString()))
                            {
                                filenameCTM = dataRow["FileName"].ToString();
                                dsCTMData = GetRapidCTMTransaction(filenameCTM);
                                if (dsCTMData != null && dsCTMData.Tables.Count > 0)
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    foreach (DataTable table in dsCTMData.Tables)
                                    {
                                        foreach (DataRow row in table.Rows)
                                        {
                                            for (int i = 0; i < table.Columns.Count; i++)
                                            {
                                                stringBuilder.Append(row[i].ToString());
                                                stringBuilder.Append((i == table.Columns.Count - 1) ? "\n" : "");
                                            }
                                            stringBuilder.AppendLine();
                                        }
                                    }
                                    sbCTM = stringBuilder;
                                    if (sbCTM != null && sbCTM.Length > 0)
                                    {
                                        MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(sbCTM.ToString()));
                                        string fileUrl = genericFunction.UploadToBlob(stream, DateTime.Now.ToString("MMMM-dd-yyyy_H-mm-ss") + "/" + filenameCTM + ".txt", BlobContainerName);
                                        SetInterfaceDetails("RAPID", filenameCTM, fileUrl, "OmanAdmin", DateTime.Now);
                                        objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, sbCTM.ToString(), filenameCTM, ".txt", StpFolerParth, portNumber, ppkLocalFilePath);
                                    }
                                    else
                                    {
                                        clsLog.WriteLogAzure("No Data available for CTM file");
                                    }
                                }
                            }
                            else
                            {
                                clsLog.WriteLogAzure("No Data available for CTM file");
                            }
                        }
                    }
                }
                else
                {
                    clsLog.WriteLogAzure("No Data available for CTM file");
                }
                return dsCTMFileName;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return dsCTMFileName;
            }
        }

        public DataSet GetRapidCTMTransaction(string strFileName)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                string[] array = new string[1];
                object[] array2 = new object[1];
                SqlDbType[] array3 = new SqlDbType[1];
                array[0] = "BatchIDForTxtFile";
                array3[0] = SqlDbType.VarChar;
                array2[0] = strFileName;
                dataSet = sQLServer.SelectRecords("SP_RapidCTMSelect", array, array2, array3);
                if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
                {
                    return dataSet;
                }
                return dataSet;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return dataSet;
            }
        }

        public void GenerateRapidInterface(DateTime fromDate, DateTime toDate)
        {
            SQLServer sQLServer = new SQLServer();
            string fileName = string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                DataSet dsFlownInsert = new DataSet();
                SQLServer sQLServer2 = new SQLServer();
                dsFlownInsert = sQLServer2.SelectRecords("SP_RapidFlownInsert"
                    , new string[2] {
                            "FromDate"
                            , "ToDate"
                    }
                    , new object[2] {
                            fromDate
                            , toDate
                    }
                    , new SqlDbType[2] {
                            SqlDbType.DateTime
                            , SqlDbType.DateTime
                    }
                );
                if (dsFlownInsert != null && dsFlownInsert.Tables.Count > 0 && dsFlownInsert.Tables[0].Rows.Count > 0)
                {
                    fileName = dsFlownInsert.Tables[0].Rows[0]["FileName"].ToString().Trim();
                }
                if (fileName != string.Empty)
                {
                    StringBuilder stringBuilder2 = new StringBuilder();
                    DataSet dataSet3 = new DataSet();
                    dataSet3 = GetRapidFlownTransaction(fileName);
                    if (dataSet3 != null && dataSet3.Tables.Count > 0)
                    {
                        foreach (DataTable table in dataSet3.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    stringBuilder.Append(row[i].ToString());
                                    stringBuilder.Append((i == table.Columns.Count - 1) ? "\n" : "");
                                }
                                stringBuilder.AppendLine();
                            }
                        }
                    }
                    stringBuilder2 = stringBuilder;
                    if (stringBuilder2 != null && stringBuilder2.Length > 0)
                    {
                        MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(stringBuilder2.ToString()));
                        string fileUrl = genericFunction.UploadToBlob(stream, DateTime.Now.ToString("MMMM-dd-yyyy_H-mm-ss") + "/" + fileName + ".txt", BlobContainerName);
                        SetInterfaceDetails("RAPID", fileName, fileUrl, "OmanAdmin", DateTime.Now);
                        objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, stringBuilder2.ToString(), fileName, ".txt", StpFolerParth, portNumber, ppkLocalFilePath);
                    }
                    else
                    {
                        clsLog.WriteLogAzure("No Data available for Flown file");
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        public DataSet GetRapidFlownTransaction(string strFileName)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                string[] array = new string[1];
                object[] array2 = new object[1];
                SqlDbType[] array3 = new SqlDbType[1];
                array[0] = "BatchIDForTxtFile";
                array3[0] = SqlDbType.VarChar;
                array2[0] = strFileName;
                dataSet = sQLServer.SelectRecords("SP_RapidFlownSelect", array, array2, array3);
                if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
                {
                    return dataSet;
                }
                return dataSet;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return dataSet;
            }
        }

        public DataSet UpdateRapidInterfaceNextdate(DateTime dtFromDate)
        {
            DataSet result = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                result = sQLServer.SelectRecords("spUpdateRapidInterfaceNextdate", new string[1]
                {
                "NextProcessDate"
                }, new object[1]
                {
                dtFromDate
                }, new SqlDbType[1]
                {
                SqlDbType.DateTime
                });
                return result;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return result;
            }
        }

        public DataSet GetRapidConfiguration()
        {
            DataSet result = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                result = sQLServer.SelectRecords("spGetRAPIDConfiguration", new string[1]
                {
                "MessageType"
                }, new object[1]
                {
                "RAPID"
                }, new SqlDbType[1]
                {
                SqlDbType.VarChar
                });
                return result;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return result;
            }
        }

        public DataSet GetLastUpdatedConfiguration()
        {
            DataSet result = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                result = sQLServer.SelectRecords("spGetRAPIDConfiguration", new string[1]
                {
                "MessageType"
                }, new object[1]
                {
                "RAPID"
                }, new SqlDbType[1]
                {
                SqlDbType.VarChar
                });
                return result;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return result;
            }
        }

        public void SetInterfaceDetails(string fileType, string fileName, string fileUrl, string createdBy, DateTime createdDate)
        {
            try
            {
                SQLServer sQLServer = new SQLServer();
                sQLServer.SelectRecords("uspSetInterfaceDetails", new string[5]
                {
                "FileType",
                "FileName",
                "FileURL",
                "CreatedBy",
                "CreatedDate"
                }, new object[5]
                {
                fileType,
                fileName,
                fileUrl,
                createdBy,
                createdDate
                }, new SqlDbType[5]
                {
                SqlDbType.VarChar,
                SqlDbType.VarChar,
                SqlDbType.VarChar,
                SqlDbType.VarChar,
                SqlDbType.DateTime
                });
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
        /*
        string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;

        public string SFTPAddress = string.Empty;
        public string SFTPUserName = string.Empty;
        public string SFTPPassWord = string.Empty;
        string BlobContainerName = "rapid";
        GenericFunction genericFunction = new GenericFunction();
        FTP objftp = new FTP();
        int portNumber = 0;
        DataSet dsRapid = new DataSet();
        DataRow drMsg = null;

        public RapidInterface()
        {

            dsRapid = GetRapidConfiguration();
            if (dsRapid != null)
            {
                if (dsRapid.Tables.Count > 0)
                {
                    drMsg = dsRapid.Tables[0].Rows[0];

                    SFTPAddress = drMsg["FTPID"].ToString();
                    SFTPUserName = drMsg["FTPUserName"].ToString();
                    SFTPPassWord = drMsg["FTPPassword"].ToString();
                    ppkFileName = drMsg["PPKFileName"].ToString().Trim();
                    SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                    StpFolerParth = drMsg["RemotePath"].ToString();
                    SFTPPortNumber = drMsg["PortNumber"].ToString();
                    GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
                
                }
            }

            //SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
            //SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
            //SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");

            //SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
            //StpFolerParth = genericFunction.ReadValueFromDb("msgService_OUT_FolderPath");
            //SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");
            //GHAOutFolderPath = string.Empty;
            //ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
            //ppkLocalFilePath = string.Empty;

            if (ppkFileName != string.Empty)
            {
                ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
            }
             portNumber = Convert.ToInt32(SFTPPortNumber);
        }


        #region RapidInterfaceData
        public void RapidInterfaceData(DateTime dtFromDate, DateTime dtToDate, DateTime NextProcessDateTime)
        {
            clsLog.WriteLogAzure("In RapidInterfaceData fnction");

            InsertRapidInterfaceData(dtFromDate, dtToDate);
            InsertRapidCTMTransaction(dtFromDate, dtToDate);
            GenerateRapidInterface(dtFromDate, dtToDate);
            //UpdateRapidInterfaceNextdate(NextProcessDateTime);
        }


        #region RapidAWB

        public void InsertRapidInterfaceData(DateTime dtFromDate, DateTime dtToDate)
        {
            DataSet ds = new DataSet();
            DataSet dsRpdIntrfcAWBFileName = new DataSet();
            DataSet dsAWBData = new DataSet();
            StringBuilder sbAWB = new StringBuilder();
            string filenameAWBData = "";

            try
            {
                SQLServer da = new SQLServer();

                string[] pName = new string[3];
                pName[0] = "FromDate";
                pName[1] = "Todate";
                pName[2] = "UpdatedOn";

                object[] pValue = new object[3];
                pValue[0] = dtFromDate;
                pValue[1] = dtToDate;
                pValue[2] = DateTime.Now;

                SqlDbType[] pType = new SqlDbType[3];
                pType[0] = SqlDbType.DateTime;
                pType[1] = SqlDbType.DateTime;
                pType[2] = SqlDbType.DateTime;


                dsRpdIntrfcAWBFileName = da.SelectRecords("uspRapidInterfaceVerifiedAWBInsert", pName, pValue, pType);

                if (dsRpdIntrfcAWBFileName != null)
                {
                    if (dsRpdIntrfcAWBFileName.Tables.Count > 0)
                    {
                        if (dsRpdIntrfcAWBFileName.Tables[0] != null)
                        {
                            if (dsRpdIntrfcAWBFileName.Tables[0].Rows.Count > 0)
                            {
                                DataRow drAWBFileName = dsRpdIntrfcAWBFileName.Tables[0].Rows[0];

                                if (drAWBFileName != null)
                                {
                                    if (!string.IsNullOrEmpty(drAWBFileName["FileName"].ToString()))
                                    {

                                        filenameAWBData = drAWBFileName["FileName"].ToString();

                                        dsAWBData = GetRapidInterfaceData(filenameAWBData);
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
                                                DataRow[] drRateline = dsAWBData.Tables[2].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString());
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
                                                DataRow[] drOtherCharges = dsAWBData.Tables[3].Select("AWBNumberWithCheckDigit=" + row["AWBNumberWithCheckDigit"].ToString());
                                                foreach (DataRow rowOT in drOtherCharges) // Select each OT Row
                                                {
                                                    for (int i = 0; i < dsAWBData.Tables[3].Columns.Count; i++)// Write Each coloumn in a OT Row
                                                    {
                                                        result.Append(rowOT[i].ToString());
                                                        result.Append(i == dsAWBData.Tables[3].Columns.Count - 1 ? "\n" : "");
                                                    }
                                                    result.AppendLine();
                                                }
                                            }

                                            //[END]AWb, RateLine and OT

                                            //Trailer Row
                                            foreach (DataRow row in dsAWBData.Tables[4].Rows) // Select each Row
                                            {
                                                for (int i = 0; i < dsAWBData.Tables[4].Columns.Count; i++)// Write Each coloumn in a Row
                                                {
                                                    result.Append(row[i].ToString());
                                                    result.Append(i == dsAWBData.Tables[4].Columns.Count - 1 ? "\n" : "");
                                                }
                                                result.AppendLine();
                                            }


                                            sbAWB = result;

                                            if (sbAWB != null)
                                            {
                                                #region Upload file to Blob
                                                MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(sbAWB.ToString()));
                                                String FileUrl = genericFunction.UploadToBlob(mStream, (DateTime.Now.ToString("MMMM-dd-yyyy_H-mm-ss")) + "/" + filenameAWBData + ".txt", BlobContainerName);

                                                SetInterfaceDetails("RAPID", filenameAWBData, FileUrl, "OmanAdmin", DateTime.Now);

                                                objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, sbAWB.ToString(), filenameAWBData, ".txt", StpFolerParth, portNumber, ppkLocalFilePath, "");

                                                #endregion
                                            }
                                            else
                                            {
                                                clsLog.WriteLogAzure("No Data available for AWB file");
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            clsLog.WriteLogAzure("No Data available for AWB file");
                                            return;
                                        }
                                    }
                                }
                            }                            

                        }
                    }

                }
                else
                {
                    clsLog.WriteLogAzure("No Data available for AWB file");
                    return;

                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        public DataSet GetRapidInterfaceData(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;

                ds = da.SelectRecords("SP_RapidInterfaceSelect", QueryPname, QueryValue, QueryType);

                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }

        #endregion

        #region RapidCTM
        DataSet dsCTMData = new DataSet();
        DataSet dsCTMFileName = new DataSet();

        StringBuilder sbCTM = new StringBuilder();
        string filenameCTM = "";

        public DataSet InsertRapidCTMTransaction(DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[2];
                PName[0] = "FromDate";
                PName[1] = "Todate";


                object[] PValue = new object[2];
                PValue[0] = dtFromDate;
                PValue[1] = dtToDate;

                SqlDbType[] PType = new SqlDbType[2];
                PType[0] = SqlDbType.DateTime;
                PType[1] = SqlDbType.DateTime;


                dsCTMFileName = da.SelectRecords("SP_RapidCTMInsert", PName, PValue, PType);


                if (dsCTMFileName != null)
                {
                    if (dsCTMFileName.Tables.Count > 0)
                    {
                        if (dsCTMFileName.Tables[0] != null)
                        {
                            if (dsCTMFileName.Tables[0].Rows.Count > 0)
                            {
                                DataRow dr = dsCTMFileName.Tables[0].Rows[0];

                                if (dr != null)
                                {
                                    if (!string.IsNullOrEmpty(dr["FileName"].ToString()))
                                    {

                                        filenameCTM = dr["FileName"].ToString();
                                        dsCTMData = GetRapidCTMTransaction(filenameCTM);

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
                                            sbCTM = result;

                                            if (sbCTM != null)
                                            {
                                                #region Upload file to Blob
                                                MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(sbCTM.ToString()));
                                                String FileUrl = genericFunction.UploadToBlob(mStream, (DateTime.Now.ToString("MMMM-dd-yyyy_H-mm-ss")) + "/" + filenameCTM + ".txt", BlobContainerName);

                                                SetInterfaceDetails("RAPID", filenameCTM, FileUrl, "OmanAdmin", DateTime.Now);

                                                objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, sbCTM.ToString(), filenameCTM, ".txt", StpFolerParth, portNumber, ppkLocalFilePath, "");

                                                #endregion
                                            }
                                            else
                                            {
                                                clsLog.WriteLogAzure("No Data available for CTM file");
                                               
                                            }

                                        }

                                    }
                                    else
                                    {
                                        clsLog.WriteLogAzure("No Data available for CTM file");
                                       
                                    }
                                }
                            }
                            
                        }
                    }
                }
                else
                {
                    clsLog.WriteLogAzure("No Data available for CTM file");
                }

                return dsCTMFileName;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return dsCTMFileName;
            }
        }

        public DataSet GetRapidCTMTransaction(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;


                ds = da.SelectRecords("SP_RapidCTMSelect", QueryPname, QueryValue, QueryType);


                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        #endregion

        #region RAPIDFlowDown
        public void GenerateRapidInterface(DateTime fromDate, DateTime toDate)
        {
            SQLServer objsql = new SQLServer();
            //byte[] byteArray = null;
            //MemoryStream stream = null;
            DataSet dsGetSapInfo = new DataSet("BALReveraInterface_dsGetSAPInfo");
            string filenameFlown = "";
            var result = new StringBuilder();

            try
            {

                DataSet ds = new DataSet();
                SQLServer da = new SQLServer();

                string[] pname = new string[2];
                pname[0] = "FromDate";
                pname[1] = "ToDate";

                object[] pvalue = new object[2];
                pvalue[0] = fromDate;
                pvalue[1] = toDate;

                SqlDbType[] ptype = new SqlDbType[2];
                ptype[0] = SqlDbType.DateTime;
                ptype[1] = SqlDbType.DateTime;


                ds = da.SelectRecords("SP_RapidFlownInsert", pname, pvalue, ptype);


                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow dr = ds.Tables[0].Rows[0];

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
                                }
                            }  
                        }
                    }
                }
                StringBuilder sbFlown = new StringBuilder();
                DataSet dsFlownData = new DataSet();
                dsFlownData = GetRapidFlownTransaction(filenameFlown);
                if (dsFlownData != null && dsFlownData.Tables.Count > 0)
                {
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
                }

                sbFlown = result;

                if (sbFlown != null)
                {
                    #region Upload file to Blob
                    MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(sbFlown.ToString()));
                    String FileUrl = genericFunction.UploadToBlob(mStream, (DateTime.Now.ToString("MMMM-dd-yyyy_H-mm-ss")) + "/" + filenameFlown + ".txt", BlobContainerName);
                    
                    SetInterfaceDetails("RAPID", filenameFlown, FileUrl, "OmanAdmin", DateTime.Now);

                    objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, sbFlown.ToString(), filenameFlown, ".txt", StpFolerParth, portNumber, ppkLocalFilePath, "");


                    #endregion
                }
                else
                {
                    clsLog.WriteLogAzure("No Data available for Flown file");
                }

            }
            catch (Exception ex)
            {
                if (dsGetSapInfo != null)
                    dsGetSapInfo.Dispose();
                clsLog.WriteLogAzure(ex);
            }
        }

        public DataSet GetRapidFlownTransaction(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;


                ds = da.SelectRecords("SP_RapidFlownSelect", QueryPname, QueryValue, QueryType);


                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }
        #endregion

        #region UpdateRapidNextDate

        public DataSet UpdateRapidInterfaceNextdate(DateTime dtFromDate)
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[1];
                PName[0] = "NextProcessDate";



                object[] PValue = new object[1];
                PValue[0] = dtFromDate;


                SqlDbType[] PType = new SqlDbType[1];
                PType[0] = SqlDbType.DateTime;

                ds = da.SelectRecords("spUpdateRapidInterfaceNextdate", PName, PValue, PType);

                return ds;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        public DataSet GetRapidConfiguration()
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[1];
                PName[0] = "MessageType";


                object[] PValue = new object[1];
                PValue[0] = "RAPID";

                SqlDbType[] PType = new SqlDbType[1];
                PType[0] = SqlDbType.VarChar;

                ds = da.SelectRecords("spGetRAPIDConfiguration", PName, PValue, PType);

                return ds;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        public DataSet GetLastUpdatedConfiguration()
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[1];
                PName[0] = "MessageType";


                object[] PValue = new object[1];
                PValue[0] = "RAPID";

                SqlDbType[] PType = new SqlDbType[1];
                PType[0] = SqlDbType.VarChar;

                ds = da.SelectRecords("spGetRAPIDConfiguration", PName, PValue, PType);

                return ds;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        #endregion

        public void SetInterfaceDetails(string fileType, string fileName, string fileUrl, string createdBy, DateTime createdDate)
        {

            try
            {
                SQLServer da = new SQLServer();

                string[] pName = new string[5];
                pName[0] = "FileType";
                pName[1] = "FileName";
                pName[2] = "FileURL";
                pName[3] = "CreatedBy";
                pName[4] = "CreatedDate";

                object[] pValue = new object[5];
                pValue[0] = fileType;
                pValue[1] = fileName;
                pValue[2] = fileUrl;
                pValue[3] = createdBy;
                pValue[4] = createdDate;

                SqlDbType[] pType = new SqlDbType[5];
                pType[0] = SqlDbType.VarChar;
                pType[1] = SqlDbType.VarChar;
                pType[2] = SqlDbType.VarChar;
                pType[3] = SqlDbType.VarChar;
                pType[4] = SqlDbType.DateTime;

                da.SelectRecords("uspSetInterfaceDetails", pName, pValue, pType);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
        #endregion
        */
    }
}
