using Microsoft.Extensions.Logging;
//using QID.DataAccess;//Not in used
using QidWorkerRole.UploadMasters;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.Globalization;

namespace QidWorkerRole
{
    public class MessagingProcess
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<MessagingProcess> _logger;
        private readonly EMAILOUT _emailOut;
        private readonly GenericFunction _genericFunction;
        private readonly Cls_BL _clsBL;
        private readonly AzureDrive _azureDrive;
        private readonly FTP _ftp;
        private readonly UploadMaster _uploadMaster;


        #region Constructor
        public MessagingProcess(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<MessagingProcess> logger, EMAILOUT emailOut, GenericFunction genericFunction, Cls_BL clsBL, AzureDrive azureDrive, FTP ftp, UploadMaster uploadMaster)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _emailOut = emailOut;
            _genericFunction = genericFunction;
            _clsBL = clsBL;
            _azureDrive = azureDrive;
            _ftp = ftp;
            _uploadMaster = uploadMaster;
        }
        #endregion
        int ThreadSleepTime = 10000; // 10 Seconds by default
        int outMsgThreadSleepTime = 5000;

        #region :: Public Methods ::
        /// <summary>
        /// Method to create and start thread for DBCall, Send Mail, Receive Message
        /// </summary>
        public void RunMessagingProcess(bool IsMessagingProcessRunning)
        {
            try
            {
                if (IsMessagingProcessRunning)
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    bool isReadMessageQueue = false, IsSAPEnabled = false;

                    //if (!int.TryParse(genericFunction.GetConfigurationValues("ThreadSleepTime"), out ThreadSleepTime))
                    if (!int.TryParse(ConfigCache.Get("ThreadSleepTime"), out ThreadSleepTime))
                    {
                        ThreadSleepTime = 10000;
                    }
                    //if (!int.TryParse(genericFunction.GetConfigurationValues("OutMsgThreadSleepTime"), out outMsgThreadSleepTime))
                    if (!int.TryParse(ConfigCache.Get("OutMsgThreadSleepTime"), out outMsgThreadSleepTime))
                    {
                        outMsgThreadSleepTime = 5000;
                    }
                    //if (!bool.TryParse(genericFunction.GetConfigurationValues("ReadMessageQueue"), out isReadMessageQueue))
                    if (!bool.TryParse(ConfigCache.Get("ReadMessageQueue"), out isReadMessageQueue))
                    {
                        isReadMessageQueue = false;
                    }
                    //if (!bool.TryParse(genericFunction.GetConfigurationValues("EnableSAPInterface"), out IsSAPEnabled))
                    if (!bool.TryParse(ConfigCache.Get("EnableSAPInterface"), out IsSAPEnabled))
                    {
                        IsSAPEnabled = false;
                    }

                    clsLog.WriteLogAzure("Messaging Service Started!!!");

                    #region : Service start-restart alert : 
                    int outport = 0;
                    //EMAILOUT emailOut = new EMAILOUT();
                    //string accountEmail = genericFunction.ReadValueFromDb("msgService_OutEmailId");
                    string accountEmail = ConfigCache.Get("msgService_OutEmailId");
                    //string password = genericFunction.ReadValueFromDb("msgService_OutEmailPassword");
                    string password = ConfigCache.Get("msgService_OutEmailPassword");
                    //string MailIouterver = genericFunction.ReadValueFromDb("msgService_EmailOutServer");
                    string MailIouterver = ConfigCache.Get("msgService_EmailOutServer");
                    //string MailsendPort = genericFunction.ReadValueFromDb("msgService_OutgoingMessagePort");
                    string MailsendPort = ConfigCache.Get("msgService_OutgoingMessagePort");

                    if (MailsendPort != "")
                        outport = int.Parse(MailsendPort == "" ? "110" : MailsendPort);
                    else
                        //outport = int.Parse(Convert.ToString(genericFunction.ReadValueFromDb("OutPort")));
                        outport = int.Parse(Convert.ToString(ConfigCache.Get("OutPort")));

                    //emailOut.sendMail(accountEmail, "prashant@smartkargo.com", password, "Messaging service start-restart alert"
                    //    , "Hi,\r\n\r\nMessaging service has been started/restarted, please check start/restart reason.\r\n\r\nThanks,\r\nTeam SmartKargo.", false, outport, "", "");
                    _emailOut.sendMail(accountEmail, "prashant@smartkargo.com", password, "Messaging service start-restart alert"
                        , "Hi,\r\n\r\nMessaging service has been started/restarted, please check start/restart reason.\r\n\r\nThanks,\r\nTeam SmartKargo.", false, outport, "", "");
                    #endregion Service start/restart alert

                    #region : Thread :
                    Thread threadDBCalls = new Thread(new ThreadStart(() => DBCalls()));
                    Thread threadSendMail = new Thread(new ThreadStart(() => SendMail()));
                    Thread threadReceiveMessage = new Thread(new ThreadStart(() => ReceiveMessage()));
                    Thread threadFTPListener = new Thread(new ThreadStart(() => FTPListener()));
                    ///Need to uncomment to read master upload files from file share. It is required for Alaska
                    //Thread threadUploadMasters = new Thread(new ThreadStart(() => UploadMasters()));
                    Thread threadUploadFlightSchedule = new Thread(new ThreadStart(() => UploadMastersProcess()));

                    if (isReadMessageQueue)
                    {
                        Thread threadReceiveMQMessage = new Thread(new ThreadStart(() => ReceiveMQMessage()));
                        threadReceiveMQMessage.IsBackground = true;
                        threadReceiveMQMessage.Start();
                    }
                    if (IsSAPEnabled)
                    {
                        Thread threadSAP = new Thread(new ThreadStart(() => SAPProcess()));
                        threadSAP.IsBackground = true;
                        threadSAP.Start();
                    }

                    threadDBCalls.IsBackground = true;
                    threadSendMail.IsBackground = true;
                    threadReceiveMessage.IsBackground = true;
                    threadFTPListener.IsBackground = true;
                    //threadUploadMasters.IsBackground = true;
                    threadUploadFlightSchedule.IsBackground = true;

                    threadDBCalls.Start();
                    threadSendMail.Start();
                    threadReceiveMessage.Start();
                    threadFTPListener.Start();
                    //threadUploadMasters.Start();
                    threadUploadFlightSchedule.Start();
                    #endregion

                    #region : Task :
                    //CallWithAsync();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
        #endregion

        #region :: Private Methods ::
        //private void DBCalls()
        private async Task DBCalls()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //Cls_BL objbl = new Cls_BL();
                while (true)
                {
                    //objbl.DBCalls();
                    await _clsBL.DBCalls();
                    Thread.Sleep(ThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        //private void SendMail()
        private async Task SendMail()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //Cls_BL objbl = new Cls_BL();
                while (true)
                {
                    //objbl.SendMessage();
                    //objbl.SendMail();
                    await _clsBL.SendMail();
                    Thread.Sleep(outMsgThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private void ReceiveMessage()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //AzureDrive objAzureDrive = new AzureDrive();
                //Cls_BL objbl = new Cls_BL();
                //FTP fTP = new FTP();
                while (true)
                {
                    //objbl.ReadMailFromMailBox();
                    _clsBL.ReadMailFromMailBox();
                    //fTP.SITASFTPDownloadFile();
                    _ftp.SITASFTPDownloadFile();
                    //objAzureDrive.ReadFromSITADrive();
                    _azureDrive.ReadFromSITADrive();
                    Thread.Sleep(ThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        //private void FTPListener()
        private async Task FTPListener()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //Cls_BL objbl = new Cls_BL();
                while (true)
                {
                    //objbl.FTPListener();
                    await _clsBL.FTPListener();
                    Thread.Sleep(ThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        //private void ReceiveMQMessage()
        private async Task ReceiveMQMessage()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //Cls_BL objbl = new Cls_BL();
                while (true)
                {
                    //objbl.ReceiveMQMessage();
                    await _clsBL.ReceiveMQMessage();
                    Thread.Sleep(ThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private async Task UploadMastersProcess()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                while (true)
                {
                    //clsLog.WriteLogAzure("In UploadMastersProcess()");

                    DataSet dsUploadMasters = new DataSet();
                    //SQLServer sqlServerUplodedFile = new SQLServer();
                    //dsUploadMasters = sqlServerUplodedFile.SelectRecords("uspGetUplodedFile");
                    dsUploadMasters = await _readWriteDao.SelectRecords("uspGetUplodedFile");

                    if (dsUploadMasters != null && dsUploadMasters.Tables.Count > 0 && dsUploadMasters.Tables[0].Rows.Count > 0)
                    {
                        clsLog.WriteLogAzure("Count of master files to be uploaded: " + dsUploadMasters.Tables[0].Rows.Count.ToString());
                        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                        uploadMasterCommon.UploadMasters(dsUploadMasters);
                    }

                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private void UploadMasters()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //UploadMaster uploadMaster = new UploadMaster();
                while (true)
                {
                    //uploadMaster.UploadMasterFile(UploadMasterType.AgentUpdate);
                    //uploadMaster.UploadMasterFile(UploadMasterType.FlightCapacity);
                    //uploadMaster.UploadMasterFile(UploadMasterType.CapacityAllocation);
                    //uploadMaster.UploadMasterFile(UploadMasterType.FlightSchedule);
                    //uploadMaster.UploadMasterFile(UploadMasterType.RateLine);
                    //uploadMaster.UploadMasterFile(UploadMasterType.Agent);
                    //uploadMaster.UploadMasterFile(UploadMasterType.ShipperConsignee);
                    //uploadMaster.UploadMasterFile(UploadMasterType.OtherCharges);
                    //uploadMaster.UploadMasterFile(UploadMasterType.TaxLine);
                    //uploadMaster.UploadMasterFile(UploadMasterType.FlightBudget);
                    //uploadMaster.UploadMasterFile(UploadMasterType.RouteControls);
                    //uploadMaster.UploadMasterFile(UploadMasterType.Airports);
                    //uploadMaster.UploadMasterFile(UploadMasterType.DCM);
                    _uploadMaster.UploadMasterFile(UploadMasterType.AgentUpdate);
                    _uploadMaster.UploadMasterFile(UploadMasterType.FlightCapacity);
                    _uploadMaster.UploadMasterFile(UploadMasterType.CapacityAllocation);
                    _uploadMaster.UploadMasterFile(UploadMasterType.FlightSchedule);
                    _uploadMaster.UploadMasterFile(UploadMasterType.RateLine);
                    _uploadMaster.UploadMasterFile(UploadMasterType.Agent);
                    _uploadMaster.UploadMasterFile(UploadMasterType.ShipperConsignee);
                    _uploadMaster.UploadMasterFile(UploadMasterType.OtherCharges);
                    _uploadMaster.UploadMasterFile(UploadMasterType.TaxLine);
                    _uploadMaster.UploadMasterFile(UploadMasterType.FlightBudget);
                    _uploadMaster.UploadMasterFile(UploadMasterType.RouteControls);
                    _uploadMaster.UploadMasterFile(UploadMasterType.Airports);
                    _uploadMaster.UploadMasterFile(UploadMasterType.DCM);
                    Thread.Sleep(60000);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private void SAPProcess()
        {
            try
            {
                WorkerRole workerRole = new WorkerRole();
                while (true)
                {
                    workerRole.SAPProcess();
                    Thread.Sleep(ThreadSleepTime);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
        //Not in used commeneted 
        //private async void CallWithAsync()
        //{
        //    Task Task1 = AsyncDBCalls();
        //    Task Task2 = AsyncSendMail();
        //    Task Task3 = AsyncReceiveMessage();
        //    Task Task4 = AsyncReceiveMQMessage();
        //    Task Task5 = AsyncUploadMasters();
        //    Task Task6 = AsyncUploadMastersProcess();
        //    await Task.WhenAll(Task1, Task2, Task3, Task4, Task5, Task6);
        //}
         
        //private Task AsyncDBCalls()
        //{
        //    return Task.Run(() =>
        //    {
        //        DBCalls();
        //    });
        //}
        
        //private Task AsyncSendMail()
        //{
        //    return Task.Run(() =>
        //    {
        //        SendMail();
        //    });
        //}

        //private Task AsyncReceiveMessage()
        //{
        //    return Task.Run(() =>
        //    {
        //        ReceiveMessage();
        //    });
        //}

        //private Task AsyncReceiveMQMessage()
        //{
        //    return Task.Run(() =>
        //    {
        //        ReceiveMQMessage();
        //    });
        //}

        //private Task AsyncUploadMasters()
        //{
        //    return Task.Run(() =>
        //    {
        //        UploadMasters();
        //    });
        //}

        //private Task AsyncUploadMastersProcess()
        //{
        //    return Task.Run(() =>
        //    {
        //        UploadMastersProcess();
        //    });
        //}
        #endregion
    }
}