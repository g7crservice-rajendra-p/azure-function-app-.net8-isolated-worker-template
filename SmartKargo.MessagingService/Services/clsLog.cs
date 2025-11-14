//using System;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//namespace QidWorkerRole
//{

//    //using Microsoft.WindowsAzure;
//    //using Microsoft.WindowsAzure.StorageClient;


//    /// <summary>
//    /// Summary description for clsLog
//    /// </summary>
//    public class clsLog
//    {
//        SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();
//        const string PAGE_NAME = "clsLog";
//        static object objLocker = "SCM";  // For Locking the thread.
//        #region WriteLog

//        private static void ProcessMessage(String Message)
//        {
//            try
//            {
//                Thread thDeueue = new Thread(() => WriteToFile(Message));
//                thDeueue.Start();
//                Thread.Sleep(0);    // Passed 0 to process context switch.         
//            }
//            catch (Exception objex)
//            {
//                WriteLog("Error while ProcessMessage c# queue due to " + objex.Message);
//            }

//        }

//        private static void WriteToFile(String Message)
//        {
//            try
//            {
//                lock (objLocker)
//                {
//                    string APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\Log\\" + "Log_" + DateTime.Now.ToString("yyMMdd") + ".txt";
//                    StreamWriter strmWriter = new StreamWriter(APP_PATH, true); ;
//                    strmWriter.WriteLine(Message);
//                    strmWriter.Close();
//                    strmWriter.Dispose();
//                }
//            }
//            catch (Exception obJex)
//            {
//                WriteLog("Error while Dequeue from c# queue due to " + obJex.Message);
//            }
//        }

//        public static void WriteLog(String Message)
//        {

//            object APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\" + "Log.txt";
//            long length = 0;
//            lock (APP_PATH)
//            {
//                StreamWriter sw1;
//                if (File.Exists(APP_PATH.ToString()))
//                {
//                    FileInfo file = new FileInfo(APP_PATH.ToString());
//                    length = file.Length;
//                }
//                if (length > 10000000)
//                    sw1 = new StreamWriter(APP_PATH.ToString(), false);
//                else
//                    sw1 = new StreamWriter(APP_PATH.ToString(), true);
//                sw1.WriteLine(Message);
//                sw1.Close();
//                sw1.Dispose();
//            }

//        }


//        public static void WriteLogAzure(String ex)
//        {
//            ProcessMessage(DateTime.Now.ToString() + " " + ex);
//            DateTime FromDate = Convert.ToDateTime(DateTime.UtcNow.Date.ToString("MM/dd/yyyy") + " 00:00:00");
//            DateTime ToDate = Convert.ToDateTime(DateTime.UtcNow.Date.ToString("MM/dd/yyyy") + " 00:15:00");
//            DateTime CurrentDate = DateTime.UtcNow;
//            if (CurrentDate > FromDate && ToDate > CurrentDate)
//            {
//                clsLog.DeleteLogFiles(10, "Log_");
//            }
//        }
//        public static void WriteLogAzure(Exception ex)
//        {
//            try
//            {
//                ProcessMessage(DateTime.Now.ToString() + "\r\n"
//                    + "ERROR MESSAGE\t-: " + ex.Message.Trim() + "\r\n"
//                    + (ex.InnerException == null ? "" : "INNER EXCEPTION\t-: " + ex.InnerException.Message.Trim() + "\r\n")
//                    + "STACK TRACE\t-: " + ex.StackTrace.Trim() + "\r\n");
//            }
//            catch (Exception e)
//            {
//                WriteLogAzure(e.Message);
//            }
//        }
//        public static void WriteLogAzure(String Message, Exception objEx)
//        {

//            String strMsg = DateTime.Now.ToString() + " " + Message;

//            if (objEx != null)
//                strMsg += "\r\n" + objEx.Message + "\r\n" + objEx.InnerException + "\r\n" + " StackTrace -: " + objEx.StackTrace;
//            ProcessMessage(strMsg);

//        }
//        public static void WriteLogAzure(Exception ex, String pageName, String funName)
//        {
//            try
//            {

//                ProcessMessage(DateTime.Now.ToString() + " " + "Error in " + pageName + " -> " + funName + " Due to " + ex.Message + "\r\n" + ex.InnerException + "\r\n" + " " + "Stacktrace " + ex.StackTrace);

//            }
//            catch (Exception exp)
//            {
//                ProcessMessage(exp.Message);
//            }
//        }
//        public static void WriteLogAzure(Exception ex, String pageName, String className, String funName)
//        {
//            try
//            {
//                ProcessMessage(DateTime.Now.ToString() + " " + "Error in " + pageName + " -> " + className + " -> " + funName + " Due to " + ex.Message + "\r\n" + ex.InnerException + "\r\n" + "Stacktrace " + ex.StackTrace);
//            }
//            catch (Exception exp)
//            {
//                ProcessMessage(exp.Message);
//            }
//        }
//        public static void WriteLogAzure(Exception ex, String pageName, String funName, String errorMessage, String errorCode)
//        {
//            try
//            {
//                ProcessMessage(DateTime.Now.ToString() + " " + "Error in " + pageName + " -> " + funName + " Due to " + errorMessage + ". Error Code -" + errorCode + " " + ex.Message + "\r\n" + ex.InnerException + "\r\n" + "Stacktrace " + ex.StackTrace);
//            }
//            catch (Exception exp)
//            {
//                ProcessMessage(exp.Message);
//            }
//        }

//        #endregion

//        #region WriteLogwithParameter
//        public static void WriteLog(String Message, string Logpath)
//        {
//            try
//            {
//                string APP_PATH = Logpath;

//                long length = 0;
//                StreamWriter sw1;
//                if (File.Exists(APP_PATH))
//                {
//                    FileInfo file = new FileInfo(APP_PATH);
//                    length = file.Length;
//                }
//                if (length > 10000000)
//                    sw1 = new StreamWriter(APP_PATH, false);
//                else
//                    sw1 = new StreamWriter(APP_PATH, true);
//                sw1.WriteLine(Message);
//                sw1.Close();
//            }
//            catch (Exception exp)
//            {
//                ProcessMessage(exp.Message);
//            }
//        }
//        #endregion

//        public static void DeleteLogFiles(int FlileDeleteBeforeDays, string FileStartWith)
//        {
//            string DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\Log";
//            DirectoryInfo dir = new DirectoryInfo(DirectoryPath);
//            if (DirectoryPath != String.Empty)
//            {
//                DateTime deleteBeforeDate = DateTime.Now.AddDays(-FlileDeleteBeforeDays);
//                foreach (FileInfo fileInfo in dir.GetFiles())
//                {
//                    if (fileInfo.Extension == ".txt" && fileInfo.Name.StartsWith(FileStartWith))
//                    {
//                        DateTime fileDate = fileInfo.LastWriteTime;
//                        if (fileDate < deleteBeforeDate)
//                        {
//                            File.Delete(fileInfo.FullName);
//                        }
//                    }
//                }
//            }
//        }
//    }


//}