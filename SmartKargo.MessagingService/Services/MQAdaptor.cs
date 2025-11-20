using IBM.WMQ;
using System.Text;


namespace QueueManager
{

    /// <summary>
    /// Enum for message type. Use Async for asynchronous write of message.
    /// </summary>
    public enum MessagingType
    {
        ASync,
        Sync
    }

    public class MQAdapter
    {

        #region :: Variable Declaration ::
        // The name of the queue manager.
        private string strQueManager;
        // The name of the queue where you will be dropping your messages.
        private string strInQueName;
        // The name of the queue where you will be getting your messages.
        private string strOutQueName;
        // The time you want a thread to wait on an empty queue until a relevant message shows up.
        private int intWaitInterval;
        private MQQueueManager qMgr;
        private string strMQChannel;
        private string strMQHost;
        private string strMQPort;
        private string strMQUser;
        #endregion

        #region :: Constructor ::
        /// <summary>
        /// Constructor which initiates MQ Object.
        /// </summary>
        /// <param name="Type">Type of Queue</param>
        /// <param name="QueManager">MQ Manager Name</param>
        /// <param name="MQChannel">MQ Channel Name</param>
        /// <param name="MQHost">MQ Host URL or IP Address</param>
        /// <param name="MQPort">MQ Port Number</param>
        /// <param name="MQUser">MQ User Name. This can be left blank in most of the cases</param>
        /// <param name="WriteToMQName">MQ Name where messages are to be written to.</param>
        /// <param name="ReadFromMQName">MQ Name from where messages are to be read from.</param>
        public MQAdapter(MessagingType Type, string QueManager, string MQChannel, string MQHost, string MQPort, string MQUser,
            string WriteToMQName, string ReadFromMQName, int WaitInterval)
        {
            try
            {
                strQueManager = QueManager;
                strMQChannel = MQChannel;
                strMQHost = MQHost;
                strMQPort = MQPort;
                strMQUser = MQUser;
                strInQueName = WriteToMQName;
                strOutQueName = ReadFromMQName;
                intWaitInterval = WaitInterval;

                if (Type == MessagingType.ASync)
                {

                    System.Collections.Hashtable properties = new System.Collections.Hashtable();
                    properties.Add(MQC.CONNECTION_NAME_PROPERTY, strMQHost + "(" + strMQPort + ")");
                    properties.Add(MQC.CHANNEL_PROPERTY, strMQChannel);
                    if (strMQUser != "")
                    {
                        properties.Add(MQC.USER_ID_PROPERTY, strMQUser);
                    }


                    qMgr = new MQQueueManager(strQueManager, properties);
                }
            }
            catch (Exception)
            {
                throw;
                //clsLog.WriteLogAzure(ex);
            }
        }
        #endregion Constructor

        #region :: Public Methods ::
        /// <summary>
        /// Writes message to MQ.
        /// </summary>
        /// <param name="Message">MQ Message to be written to Queue.</param>
        /// <param name="ErrorMessage">Output parameter which will return Error Message if any error occurs.</param>
        /// <returns>Returns Status Flag. 0 means Success and other value means Error.</returns>
        public string SendMessage(string Message, out string ErrorMessage)
        {
            ErrorMessage = "";
            int InOpenOptions;
            MQQueue InputQueue = null;
            MQMessage MqsMsg = null;
            UTF8Encoding utf8Enc;
            MQPutMessageOptions Pmo;

            try
            {
                InOpenOptions = MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING;

                InputQueue = qMgr.AccessQueue(strInQueName, InOpenOptions);

                MqsMsg = new MQMessage();
                utf8Enc = new UTF8Encoding();
                MqsMsg.WriteBytes(Message);
                Pmo = new MQPutMessageOptions();
                InputQueue.Put(MqsMsg, Pmo);

                string result = MqsMsg.MessageFlags.ToString();

                return result;

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                //clsLog.WriteLogAzure(ex);
            }
            finally
            {
                if (InputQueue != null)
                {
                    InputQueue.Close();
                }
            }
            return ("1");
        }

        /// <summary>
        /// Returns next MQ Message from Read Queue.
        /// </summary>
        /// <param name="ErrorMessage">Output parameter which will return Error Message if any error occurs.</param>
        /// <returns>Returns MQ Message as String.</returns>
        public string ReadMessage(out string ErrorMessage)
        {
            ErrorMessage = "";
            try
            {
                System.Collections.Hashtable properties = new System.Collections.Hashtable();
                properties.Add(MQC.CONNECTION_NAME_PROPERTY, strMQHost + "(" + strMQPort + ")");
                properties.Add(MQC.CHANNEL_PROPERTY, strMQChannel);
                if (strMQUser != "")
                {
                    properties.Add(MQC.USER_ID_PROPERTY, strMQUser);
                }

                qMgr = new MQQueueManager(strQueManager, properties);
                /** MQOO_INPUT_AS_Q_DEF -- open queue to get message using queue-define default.
                 *  MQOO_FAIL_IF_QUIESCING -- access fail if queue manange is quiescing. **/
                int openOptions = MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING;

                MQQueue queue = qMgr.AccessQueue(strOutQueName, openOptions);

                /** MQGMO_FAIL_IF_QUIESCING -- get message fail if queue manager is quiescing.
                 *  MQGMO_WAIT -- waits for suitable message to arrive.
                 *  MQWI_UNLIMITED -- unlimited wait interval. **/
                MQGetMessageOptions gmo = new MQGetMessageOptions();
                gmo.Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_WAIT;
                gmo.WaitInterval = MQC.MQWI_UNLIMITED;
                MQMessage message = new MQMessage();
                //wait for message
                queue.Get(message, gmo);

                queue.Close();

                //release resource.
                qMgr.Close();
                queue = null;
                gmo = null;
                System.GC.Collect();

                return message.ReadString(message.MessageLength);

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                //clsLog.WriteLogAzure(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// Method to close the MQAdapter object
        /// </summary>
        public void DisposeQueue()
        {
            try
            {
                qMgr.Close();
            }
            catch (Exception ex)
            {
                throw;
                //clsLog.WriteLogAzure(ex);
            }
        }
        #endregion

        #region :: Destructor ::
        ~MQAdapter()
        {
            try
            {
                qMgr.Close();
            }
            catch (Exception ex)
            {
                throw;
                //clsLog.WriteLogAzure(ex);
            }
        }
        #endregion
    }

}
