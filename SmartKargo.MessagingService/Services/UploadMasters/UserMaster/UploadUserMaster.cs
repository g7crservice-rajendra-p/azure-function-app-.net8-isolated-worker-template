using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Net.Mail;

namespace QidWorkerRole.UploadMasters.UserMaster
{
    /// <summary>
    /// Class to Upload User Master File.
    /// </summary>
    public class UploadUserMaster
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadUserMaster> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;

        #region Constructor
        public UploadUserMaster(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadUserMaster> logger,
            Func<UploadMasterCommon> uploadMasterCommonFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion

        /// <summary>
        /// Method to Uplaod User Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<bool> UserMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "UserMasterUploadFile", out uploadFilePath))
                        {
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", exception.Message, exception.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to Process User Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> User Master Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableUserExcelData = new DataTable("dataTableUserExcelData");

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
                dataTableUserExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommonFactory().RemoveEmptyRows(dataTableUserExcelData);

                foreach (DataColumn dataColumn in dataTableUserExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableUserExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating UserMasterTableType DataTable

                DataTable UserMasterTableType = new DataTable("UserMasterTableType");
                UserMasterTableType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
	            UserMasterTableType.Columns.Add("SrNo", System.Type.GetType("System.Int32"));
	            UserMasterTableType.Columns.Add("LoginName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("Password", System.Type.GetType("System.String"));
                UserMasterTableType.Columns.Add("RoleName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("RoleID", System.Type.GetType("System.Int32"));
	            UserMasterTableType.Columns.Add("AgentName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("AgentCode", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("StationCode", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("CreatedBy", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("CreatedOn", System.Type.GetType("System.DateTime"));
	            UserMasterTableType.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
	            UserMasterTableType.Columns.Add("UserName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("UserEmail", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
	            UserMasterTableType.Columns.Add("IsAllStn", System.Type.GetType("System.Byte"));
	            UserMasterTableType.Columns.Add("MobileNumber", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("UpdatedTime", System.Type.GetType("System.DateTime"));
	            UserMasterTableType.Columns.Add("RandamPassword", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("SessionID", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("LastAccessTime", System.Type.GetType("System.DateTime"));
	            UserMasterTableType.Columns.Add("LastAccessStation", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("IsInternal", System.Type.GetType("System.Byte"));
	            UserMasterTableType.Columns.Add("PwdExpiresIn", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("PwdUpdatedOn", System.Type.GetType("System.DateTime"));
	            UserMasterTableType.Columns.Add("DATEFORMAT", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("AllowedPaymentTypes", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("AllowedMaxCollection", System.Type.GetType("System.Decimal"));
                UserMasterTableType.Columns.Add("FailedCount", System.Type.GetType("System.Int16"));
	            UserMasterTableType.Columns.Add("BaseStation", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("CompanyType", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("CompanyName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("LinkAccount", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("VerificationCode", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("IsVerify", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("IsLock", System.Type.GetType("System.Byte"));
	            UserMasterTableType.Columns.Add("AccessLockDate", System.Type.GetType("System.DateTime"));
	            UserMasterTableType.Columns.Add("IsChangePwd", System.Type.GetType("System.Byte"));
	            UserMasterTableType.Columns.Add("ApprovalStatus", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("FirstName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("LastName", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("HomePage", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("Remark", System.Type.GetType("System.String"));
	            UserMasterTableType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));

                #endregion Creating UserMasterTableType DataTable

                string validationDetailsUser = string.Empty;

                for (int i = 0; i < dataTableUserExcelData.Rows.Count; i++)
                {
                    validationDetailsUser = string.Empty;

                    #region Create row for UserMasterTableType Data Table

                    DataRow dataRowUserMasterTableType = UserMasterTableType.NewRow();

                    #region ReferenceID INT NOT NULL

                    dataRowUserMasterTableType["ReferenceID"] = i + 1;

                    #endregion ReferenceID
	                
                    #region SrNo INT

                    dataRowUserMasterTableType["SrNo"] = 0;

                    #endregion SrNo

	                #region LoginName VARCHAR(100) NOT NULL

                    if (columnNames.Contains("loginid"))
                    {
                        if (dataTableUserExcelData.Rows[i]["loginid"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["LoginName"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " LoginID is required;";
                        }
                        else
                        {
                            if (dataTableUserExcelData.Rows[i]["loginid"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowUserMasterTableType["LoginName"] = string.Empty;
                                validationDetailsUser = validationDetailsUser + " LoginID is more than 100 Chars;";
                            }
                            else
                            {
                                if (UserMasterTableType.Select("LoginName = ''").Length == 0)
                                {
                                    if (UserMasterTableType.Select(string.Format("LoginName = '{0}'", dataTableUserExcelData.Rows[i]["loginid"].ToString().Trim().Trim(','))).Length == 0)
                                    {
                                        dataRowUserMasterTableType["LoginName"] = dataTableUserExcelData.Rows[i]["loginid"].ToString().Trim().Trim(',');
                                    }
                                    else
                                    {
                                        dataRowUserMasterTableType["LoginName"] = dataTableUserExcelData.Rows[i]["loginid"].ToString().Trim().Trim(',');
                                        validationDetailsUser = validationDetailsUser + " Duplicate LoginID in same file;";
                                    }
                                }
                                else
                                {
                                    dataRowUserMasterTableType["LoginName"] = dataTableUserExcelData.Rows[i]["loginid"].ToString().Trim().Trim(',');
                                }
                            }
                        }
                    }

                    #endregion LoginName

	                #region Password VARCHAR(500)

                    dataRowUserMasterTableType["Password"] = string.Empty;

                    #endregion Password

                    #region RoleName VARCHAR(100)

                    if (columnNames.Contains("rolename"))
                    {
                        if (dataTableUserExcelData.Rows[i]["rolename"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["RoleName"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " RoleName is required;";
                        }
                        else
                        {
                            if (dataTableUserExcelData.Rows[i]["rolename"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowUserMasterTableType["RoleName"] = string.Empty;
                                validationDetailsUser = validationDetailsUser + " RoleName is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowUserMasterTableType["RoleName"] = dataTableUserExcelData.Rows[i]["rolename"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion RoleName

	                #region RoleID INT NULL

                    dataRowUserMasterTableType["RoleID"] = 0;

                    #endregion RoleID

	                #region AgentName VARCHAR(50)

                    dataRowUserMasterTableType["AgentName"] = DBNull.Value;

                    #endregion AgentName

	                #region AgentCode VARCHAR(35)

                    if (columnNames.Contains("agentcode"))
                    {
                        if (dataTableUserExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',').Length > 35)
                        {
                            dataRowUserMasterTableType["AgentCode"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " AgentCode is more than 35 Chars;";
                        }
                        else
                        {
                            dataRowUserMasterTableType["AgentCode"] = dataTableUserExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion AgentCode

	                #region StationCode VARCHAR(MAX)

                    if (columnNames.Contains("stationcode"))
                    {
                        dataRowUserMasterTableType["StationCode"] = dataTableUserExcelData.Rows[i]["stationcode"].ToString().Trim().Trim(',');
                    }

                    #endregion StationCode

	                #region CreatedBy VARCHAR(100)

                    dataRowUserMasterTableType["CreatedBy"] = DBNull.Value;

                    #endregion CreatedBy

	                #region CreatedOn DATETIME NULL

                    dataRowUserMasterTableType["CreatedOn"] = DBNull.Value;

                    #endregion CreatedOn

	                #region IsActive BIT NOT NULL

                    if (columnNames.Contains("isactive"))
                    {
                        if (dataTableUserExcelData.Rows[i]["isactive"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["IsActive"] = DBNull.Value;
                            validationDetailsUser = validationDetailsUser + " IsActive is required;";
                        }
                        else
                        {
                            switch (dataTableUserExcelData.Rows[i]["isactive"].ToString().ToLower().Trim().Trim(','))
                            {
                                case "true":
                                case "1":
                                case "yes":
                                    dataRowUserMasterTableType["IsActive"] = 1;
                                    break;
                                case "false":
                                case "0":
                                case "no":
                                    dataRowUserMasterTableType["IsActive"] = 0;
                                    break;
                                default:
                                    dataRowUserMasterTableType["IsActive"] = DBNull.Value;
                                    validationDetailsUser = validationDetailsUser + " IsActive is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion IsActive

	                #region UserName VARCHAR(100) NOT NULL

                    if (columnNames.Contains("username"))
                    {
                        if (dataTableUserExcelData.Rows[i]["username"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["UserName"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " UserName is required;";
                        }
                        else
                        {
                            if (dataTableUserExcelData.Rows[i]["username"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowUserMasterTableType["UserName"] = string.Empty;
                                validationDetailsUser = validationDetailsUser + " UserName is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowUserMasterTableType["UserName"] = dataTableUserExcelData.Rows[i]["username"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion UserName

	                #region UserEmail VARCHAR(100)

                    if (columnNames.Contains("emailid"))
                    {
                        if (string.IsNullOrWhiteSpace(dataTableUserExcelData.Rows[i]["emailid"].ToString().Trim().Trim(',')))
                        {
                            dataRowUserMasterTableType["UserEmail"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableUserExcelData.Rows[i]["emailid"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowUserMasterTableType["UserEmail"] = string.Empty;
                                validationDetailsUser = validationDetailsUser + " EmailID is more than 100 Chars;";
                            }
                            else
                            {
                                try
                                {
                                    MailAddress mailAddress = new MailAddress(dataTableUserExcelData.Rows[i]["emailid"].ToString().Trim().Trim(','));
                                }
                                catch (FormatException)
                                {
                                    dataRowUserMasterTableType["UserEmail"] = string.Empty;
                                    validationDetailsUser = validationDetailsUser + " EmailID is invalid;";
                                }
                            }
                        }
                    }

                    #endregion UserEmail

	                #region UpdatedBy VARCHAR(100)

                    dataRowUserMasterTableType["UpdatedBy"] = DBNull.Value;

                    #endregion UpdatedBy

	                #region UpdatedOn DATETIME NULL

                    dataRowUserMasterTableType["UpdatedOn"] = DBNull.Value;

                    #endregion UpdatedOn

	                #region IsAllStn BIT NULL

                    if (columnNames.Contains("isallstn"))
                    {
                        if (dataTableUserExcelData.Rows[i]["isallstn"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["IsAllStn"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableUserExcelData.Rows[i]["isallstn"].ToString().Trim().ToLower())
                            {
                                case "true":
                                case "1":
                                case "yes":
                                    dataRowUserMasterTableType["IsAllStn"] = 1;
                                    break;
                                case "false":
                                case "0":
                                case "no":
                                    dataRowUserMasterTableType["IsAllStn"] = 0;
                                    break;
                                default:
                                    dataRowUserMasterTableType["IsAllStn"] = DBNull.Value;
                                    validationDetailsUser = validationDetailsUser + " Invalid IsAllStn;";
                                    break;
                            }                            
                        }
                    }

                    #endregion IsAllStn

	                #region MobileNumber VARCHAR(30)

                    if (columnNames.Contains("mobilenumber"))
                    {
                        if (dataTableUserExcelData.Rows[i]["mobilenumber"].ToString().Trim().Trim(',').Length > 30)
                        {
                            dataRowUserMasterTableType["MobileNumber"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " MobileNumber is more than 30 Chars;";
                        }
                        else
                        {
                            dataRowUserMasterTableType["MobileNumber"] = dataTableUserExcelData.Rows[i]["mobilenumber"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion MobileNumber

	                #region UpdatedTime DATETIME NULL

                    dataRowUserMasterTableType["UpdatedTime"] = DBNull.Value;

                    #endregion UpdatedTime

	                #region RandamPassword VARCHAR(20)

                    dataRowUserMasterTableType["RandamPassword"] = DBNull.Value;

                    #endregion RandamPassword

	                #region SessionID VARCHAR(300)

                    dataRowUserMasterTableType["SessionID"] = DBNull.Value;

                    #endregion SessionID

	                #region LastAccessTime DATETIME NULL

                    dataRowUserMasterTableType["LastAccessTime"] = DBNull.Value;

                    #endregion LastAccessTime

	                #region LastAccessStation VARCHAR(10)

                    dataRowUserMasterTableType["LastAccessStation"] = DBNull.Value;

                    #endregion LastAccessStation

	                #region IsInternal BIT NULL

                    dataRowUserMasterTableType["IsInternal"] = DBNull.Value;

                    #endregion IsInternal

	                #region PwdExpiresIn VARCHAR(10)

                    dataRowUserMasterTableType["PwdExpiresIn"] = DBNull.Value;

                    #endregion PwdExpiresIn

	                #region PwdUpdatedOn DATETIME NULL

                    dataRowUserMasterTableType["PwdUpdatedOn"] = DBNull.Value;

                    #endregion PwdUpdatedOn

	                #region DATEFORMAT VARCHAR(20)

                    dataRowUserMasterTableType["DATEFORMAT"] = DBNull.Value;

                    #endregion DATEFORMAT

	                #region AllowedPaymentTypes VARCHAR(100)

                    dataRowUserMasterTableType["AllowedPaymentTypes"] = DBNull.Value;

                    #endregion AllowedPaymentTypes

	                #region AllowedMaxCollection DECIMAL(18, 2) NULL

                    dataRowUserMasterTableType["AllowedMaxCollection"] = DBNull.Value;

                    #endregion AllowedMaxCollection

	                #region FailedCount TINYINT NULL

                    dataRowUserMasterTableType["FailedCount"] = DBNull.Value;

                    #endregion FailedCount

	                #region BaseStation VARCHAR(20)

                    if (columnNames.Contains("basestation"))
                    {
                        if (dataTableUserExcelData.Rows[i]["basestation"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["BaseStation"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " BaseStation is required;";
                        }
                        else
                        {
                            if (dataTableUserExcelData.Rows[i]["basestation"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowUserMasterTableType["BaseStation"] = string.Empty;
                                validationDetailsUser = validationDetailsUser + " BaseStation is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowUserMasterTableType["BaseStation"] = dataTableUserExcelData.Rows[i]["basestation"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion BaseStation

	                #region CompanyType VARCHAR(10)

                    dataRowUserMasterTableType["CompanyType"] = DBNull.Value;

                    #endregion CompanyType

	                #region CompanyName VARCHAR(100)

                    dataRowUserMasterTableType["CompanyName"] = DBNull.Value;

                    #endregion CompanyName

	                #region LinkAccount VARCHAR(50)

                    dataRowUserMasterTableType["LinkAccount"] = DBNull.Value;

                    #endregion LinkAccount

	                #region VerificationCode NVARCHAR(200)

                    dataRowUserMasterTableType["VerificationCode"] = DBNull.Value;

                    #endregion VerificationCode

	                #region IsVerify VARCHAR(50)

                    dataRowUserMasterTableType["IsVerify"] = DBNull.Value;

                    #endregion IsVerify

	                #region IsLock BIT NULL

                    if (columnNames.Contains("islock"))
                    {
                        if (dataTableUserExcelData.Rows[i]["islock"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowUserMasterTableType["IsLock"] = DBNull.Value;
                            validationDetailsUser = validationDetailsUser + " IsLock is required;";
                        }
                        else
                        {
                            switch (dataTableUserExcelData.Rows[i]["islock"].ToString().ToLower().Trim().Trim(','))
                            {
                                case "true":
                                case "1":
                                case "yes":
                                    dataRowUserMasterTableType["IsLock"] = 1;
                                    break;
                                case "false":
                                case "0":
                                case "no":
                                    dataRowUserMasterTableType["IsLock"] = 0;
                                    break;
                                default:
                                    dataRowUserMasterTableType["IsLock"] = DBNull.Value;
                                    validationDetailsUser = validationDetailsUser + " IsLock is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion IsLock

	                #region AccessLockDate DATETIME NULL

                    dataRowUserMasterTableType["AccessLockDate"] = DBNull.Value;

                    #endregion AccessLockDate

	                #region IsChangePwd BIT NULL

                    dataRowUserMasterTableType["IsChangePwd"] = DBNull.Value;

                    #endregion IsChangePwd

	                #region ApprovalStatus VARCHAR(20)

                    if (columnNames.Contains("approvalstatus"))
                    {
                        if (dataTableUserExcelData.Rows[i]["approvalstatus"].ToString().Trim().Trim(',').Length > 20)
                        {
                            dataRowUserMasterTableType["ApprovalStatus"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " ApprovalStatus is more than 20 Chars;";
                        }
                        else
                        {
                            dataRowUserMasterTableType["ApprovalStatus"] = dataTableUserExcelData.Rows[i]["approvalstatus"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion ApprovalStatus

	                #region FirstName VARCHAR(50)

                    if (columnNames.Contains("firstname"))
                    {
                        if (dataTableUserExcelData.Rows[i]["firstname"].ToString().Trim().Trim(',').Length > 50)
                        {
                            dataRowUserMasterTableType["FirstName"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " FirstName is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowUserMasterTableType["FirstName"] = dataTableUserExcelData.Rows[i]["firstname"].ToString().Trim().Trim(',');
                        }                        
                    }

                    dataRowUserMasterTableType["FirstName"] = DBNull.Value;

                    #endregion FirstName

	                #region LastName VARCHAR(50)

                    if (columnNames.Contains("lastname"))
                    {
                        if (dataTableUserExcelData.Rows[i]["lastname"].ToString().Trim().Trim(',').Length > 50)
                        {
                            dataRowUserMasterTableType["LastName"] = string.Empty;
                            validationDetailsUser = validationDetailsUser + " LastName is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowUserMasterTableType["LastName"] = dataTableUserExcelData.Rows[i]["lastname"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion LastName

	                #region HomePage VARCHAR(200)

                    dataRowUserMasterTableType["HomePage"] = DBNull.Value;

                    #endregion HomePage

	                #region Remark VARCHAR(300)

                    dataRowUserMasterTableType["Remark"] = DBNull.Value;

                    #endregion Remark

                    dataRowUserMasterTableType["ValidationDetails"] = validationDetailsUser;

                    #endregion Create row for UserMasterTableType Data Table

                    UserMasterTableType.Rows.Add(dataRowUserMasterTableType);
                }

                // Database Call to Validate & Insert/Update User Master
                string errorInSp = string.Empty;
                DataSet? dataSetResult = new DataSet();

                dataSetResult = await ValidateAndInsertUpdateUserMaster(srNotblMasterUploadSummaryLog, UserMasterTableType, errorInSp);

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
                dataTableUserExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting User Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="userMasterNewTableType"> User Master Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet> ValidateAndInsertUpdateUserMaster(int srNotblMasterUploadSummaryLog, DataTable userMasterNewTableType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [ 
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@UserMasterNewTableType", userMasterNewTableType),
                    new SqlParameter("@Error", errorInSp)
                ];

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("Masters.uspUploadUserMaster", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspUploadUserMaster", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", exception.Message, exception.StackTrace);
                return dataSetResult;
            }
        }
    }
}
