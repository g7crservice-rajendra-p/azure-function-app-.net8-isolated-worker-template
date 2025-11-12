using Microsoft.Extensions.Logging;
//using QID.DataAccess;//Not in used
using SmartKargo.MessagingService.Data.Dao.Interfaces;

namespace QidWorkerRole
{
    public class MQMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<MQMessageProcessor> _logger;
        private readonly Cls_BL _clsBL;

        #region Constructor
        public MQMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<MQMessageProcessor> logger, Cls_BL clsBL)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _clsBL = clsBL;
        }
        #endregion
        /// <summary>
        /// Method to send the MQMessage using MQAdapter
        /// </summary>
        /// Not in use currently, no reference found
        //public void SendMQMessage()
        //{
        //    try
        //    {   
        //        //TO DO : to be merged with SendMail method in Cls_BL
        //        //Cls_BL cls_BL = new Cls_BL();
        //        bool isOn = false;
        //        bool ishtml = false;
        //        string status = "Processed";
        //        //SQLServer objsql = new SQLServer();

        //        do
        //        {
        //            string ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty, ccadd = string.Empty, FileExtension = string.Empty, msgCommType = string.Empty;
        //            isOn = false;
        //            DataSet ds = null;

        //            ds = objsql.SelectRecords("spMailtoSend");

        //            if (ds == null)
        //            {
        //                Console.WriteLine("Data set is null");

        //            }
        //            if (ds != null)
        //            {
        //                if (ds.Tables.Count > 0)
        //                {
        //                    if (ds.Tables[0].Rows.Count > 0)
        //                    {
        //                        isOn = true;
        //                        bool isMessageSent = false;
        //                        DataRow dr = ds.Tables[0].Rows[0];
        //                        string subject = dr[1].ToString();
        //                        msgCommType = "EMAIL";
        //                        DataRow drMsg = null;
        //                        string FileName = dr["Subject"].ToString();
        //                        string body = dr[2].ToString();
        //                        string sentadd = dr[4].ToString().Trim(',');
        //                        if (dr[3].ToString().Length > 3)
        //                            ccadd = dr[3].ToString().Trim(',');
        //                        ishtml = bool.Parse(dr["ishtml"].ToString() == "" ? "0" : dr["ishtml"].ToString());

        //                        if (ds.Tables[2].Rows.Count > 0)
        //                        {
        //                            drMsg = ds.Tables[2].Rows[0];
        //                            msgCommType = drMsg["MsgCommType"].ToString().ToUpper().Trim();
        //                            FileExtension = drMsg["FileExtension"].ToString().ToUpper().Trim();
        //                        }

        //                        if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
        //                            status = "Processed";
        //                        if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
        //                            status = "Re-Processed";
        //                        ///Region added by prashant on 13-Dec-2016
        //                        if (msgCommType.ToUpper() == "MESSAGE QUEUE")
        //                        {
        //                            if (drMsg != null)
        //                            {
        //                                //lblStatus.Text = "Sending..";
        //                                string MQManager = Convert.ToString(drMsg["MQManager"]);
        //                                string MQChannel = Convert.ToString(drMsg["MQChannel"]);
        //                                string MQHost = Convert.ToString(drMsg["MQHost"]);
        //                                string MQPort = Convert.ToString(drMsg["MQPort"]);
        //                                string MQUser = Convert.ToString(drMsg["MQUser"]);
        //                                string MQInqueue = "CG.BOOKINGS.CARGOSPOT.SMARTKARGO";
        //                                string MQOutqueue = "";
        //                                string Message = body;
        //                                int WaitInterval = 0;

        //                                string ErrorMessage = string.Empty;

        //                                MQAdapter mqAdapter = new MQAdapter(MessagingType.ASync, MQManager, MQChannel, MQHost, MQPort, MQUser, MQInqueue, MQOutqueue, WaitInterval);

        //                                Console.WriteLine();
        //                                Console.WriteLine("*********** Send ***********");
        //                                Console.WriteLine("MQInqueue :" + MQInqueue);
        //                                Console.WriteLine("MQOutqueue :" + MQOutqueue);
        //                                Console.WriteLine("Message :" + Message);

        //                                string result = mqAdapter.SendMessage(Message, out ErrorMessage);


        //                                if (ErrorMessage.Trim() == string.Empty)
        //                                {
        //                                    clsLog.WriteLogAzure("MQMessage sent successfully");
        //                                    Console.WriteLine("MQMessage sent successfully");
        //                                }
        //                                else
        //                                {
        //                                    clsLog.WriteLogAzure("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
        //                                    Console.WriteLine("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
        //                                }
        //                                //TO DO : To be removed
        //                                Console.ReadLine();

        //                                mqAdapter.DisposeQueue();

        //                                //MessageBox.Show("Result: " + result);
        //                                //lblStatus.Text = "Data sent successfully to Queue";
        //                            }
        //                        }

        //                        if (!isMessageSent)
        //                        {
        //                            string[] pname = { "num", "Status", "ErrorMsg" };
        //                            object[] pvalue = { int.Parse(dr[0].ToString()), status, "Error occured while processing sending request." };
        //                            SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar };
        //                            if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
        //                                clsLog.WriteLogAzure("Email not Sent successfully to:" + dr[0].ToString());
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //                isOn = false;

        //        } while (isOn);

        //        objsql = null;
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error In (SendMQMessage Processor Method): " + DateTime.Now + " : " + ex.Message);
        //    }
        //}

        /// <summary>
        /// Method to receive MQMessage by using the MQAdapter
        /// </summary>
        /// Not in use currently, no reference found
        //public void ReceiveMQMessage()
        //{
        //    Cls_BL objCls_BL = new Cls_BL();
        //    DataSet dsMQConfiguration = new DataSet();
        //    SQLServer objsql = new SQLServer();
        //    try
        //    {
        //        //TO DO : Need to get below values from configurationIN

        //        dsMQConfiguration = objsql.SelectRecords("uspGetMQConfiguration");
        //        if (dsMQConfiguration != null)
        //        {
        //            if (dsMQConfiguration.Tables.Count > 0 && dsMQConfiguration.Tables[0].Rows.Count > 0)
        //            {
        //                for (int i = 0; i < dsMQConfiguration.Tables[0].Rows.Count; i++)
        //                {
        //                    DataRow drMQConfiguration = dsMQConfiguration.Tables[0].Rows[i];
        //                    string MessageBody = string.Empty;

        //                    //string MQManager = "QMMFT";
        //                    //string MQChannel = "SK.SVRCONN";
        //                    //string MQHost = "159.49.252.69";
        //                    //string MQPort = "1415";
        //                    //string MQUser = "";
        //                    //string MQInqueue = "";// "CG.BOOKINGS.CARGOSPOT.SMARTKARGO";
        //                    //string MQOutqueue = "CG.BOOKINGS.CARGOSPOT.SMARTKARGO";//CG.BOOKINGS.SMARTKARGO.CARGOSPOT";

        //                    string MQManager = drMQConfiguration["MQManager"].ToString();
        //                    string MQChannel = drMQConfiguration["MQChannel"].ToString();
        //                    string MQHost = drMQConfiguration["MQHost"].ToString();
        //                    string MQPort = drMQConfiguration["MQPort"].ToString();
        //                    string MQUser = drMQConfiguration["MQUser"].ToString();
        //                    string MQInqueue = "";
        //                    string MQOutqueue = drMQConfiguration["MQInQueue"].ToString();//TO DO : column to be changed from MQInQueue to MQOuteque


        //                    MQAdapter mqAdapter = new MQAdapter(MessagingType.ASync, MQManager, MQChannel, MQHost, MQPort, MQUser, MQInqueue, MQOutqueue, 0);
        //                    string ErrorMessage = "";

        //                    MessageBody = mqAdapter.ReadMessage(out ErrorMessage);

        //                    string Subject = "MQ Message";
        //                    string fromEmail = "";
        //                    string toEmail = "";
        //                    DateTime dtRec = DateTime.Now;
        //                    DateTime dtSend = DateTime.Now;
        //                    string MessageType = "MQMessage";
        //                    string status = string.Empty;

        //                    Console.WriteLine();
        //                    Console.WriteLine("*********** Recieve ***********");
        //                    Console.WriteLine("MQInqueue :" + MQInqueue);
        //                    Console.WriteLine("MQOutqueue :" + MQOutqueue);
        //                    Console.WriteLine("Message :" + MessageBody);

        //                    if (objCls_BL.SaveMessage(Subject.ToUpper(), MessageBody.Trim(), fromEmail, toEmail, dtRec, dtSend, MessageType, status, "MQMessage", "", "", Convert.ToDateTime("1900-01-01 00:00:00.000")))
        //                    {
        //                        clsLog.WriteLogAzure("MQMessage saved successfully in to inbox : " + DateTime.Now);
        //                        Console.WriteLine("MQMessage saved successfully in to inbox");
        //                    }
        //                    else
        //                    {
        //                        clsLog.WriteLogAzure("Fail to save MQMessage : " + DateTime.Now);
        //                        Console.WriteLine("Fail to save MQMessage");
        //                    }
        //                    mqAdapter.DisposeQueue();
        //                }
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error : In(ReceiveMQMessage() method) [" + DateTime.Now + "] " + ex.Message);
        //    }
        //}
    }
}
