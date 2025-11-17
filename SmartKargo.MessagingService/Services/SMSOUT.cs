using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Net;
using System.Text;

namespace QidWorkerRole
{
    public class SMSOUT
    {

        #region variables
        //GenericFunction genericFunction = new GenericFunction();
        private readonly GenericFunction _genericFunction;
        private readonly ILogger<SMSOUT> _logger;
        #endregion

        #region Constructor
        public SMSOUT(GenericFunction genericFunction,
                ILogger<SMSOUT> logger)
        {
            _genericFunction = genericFunction;
            _logger = logger;
        }

        #endregion

        #region Send SMS
        /// <summary>
        /// sending SMS through http link
        /// </summary>
        /// <param name="mobileno"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public bool sendSMS(string mobileno, string message)
        {
            bool flag = false;
            try
            {

                #region Constants
                StringBuilder sb = new StringBuilder();
                int count = 0;
                string tempstring = "";
                byte[] buf = new byte[8192];
                # endregion

                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://www.smscountry.com/SMSCwebservice.asp?User=" + Convert.ToString(_genericFunction.ReadValueFromDb("SMSUN")) + "&passwd=" + Convert.ToString(_genericFunction.ReadValueFromDb("SMSPASS")) + "&mobilenumber=" + mobileno + "&message=" + message + "&sid=QIDAlert&mtype=N&DR=Y");
                HttpWebResponse Reponse = (HttpWebResponse)Request.GetResponse();
                Stream Response_Stream = Reponse.GetResponseStream();

                do
                {
                    count = Response_Stream.Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        tempstring = Encoding.ASCII.GetString(buf, 0, count);
                        sb.Append(tempstring);
                    }
                } while (count > 0);
                clsLog.WriteLogAzure("SMS sent @ " + DateTime.Now.ToString(), null);
                flag = true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Exception while sending SMS : "+ ex.Message+" @ "+ DateTime.Now.ToString(),ex);
                flag = false;
            }
            return flag;
        }

        public bool sendSMS(string mobileno, string message, string username, string password)
        {
            bool flag = false;
            try
            {

                #region Constants
                StringBuilder sb = new StringBuilder();
                int count = 0;
                string tempstring = "";
                byte[] buf = new byte[8192];
                # endregion

                //clsLog.WriteLog("SMS Message = " + message + " @ " + System.DateTime.Now.ToString());
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://www.smscountry.com/SMSCwebservice.asp?User=" + username + "&passwd=" + password + "&mobilenumber=" + mobileno + "&message=" + message + "&sid=QIDAlert&mtype=N&DR=Y");
                //HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://www.smscountry.com/SMSCwebservice.asp?User=" + ConfigurationManager.AppSettings["SMSUN"].ToString() + "&passwd=" + ConfigurationManager.AppSettings["SMSPASS"].ToString() + "&mobilenumber=" + "919224255512," + ",919767816454" + "&message=" + Message + "&sid=QIDAlert&mtype=N&DR=Y");

                HttpWebResponse Reponse = (HttpWebResponse)Request.GetResponse();
                Stream Response_Stream = Reponse.GetResponseStream();

                do
                {
                    count = Response_Stream.Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        tempstring = Encoding.ASCII.GetString(buf, 0, count);
                        sb.Append(tempstring);
                    }
                } while (count > 0);
                clsLog.WriteLogAzure("SMS sent @ " + DateTime.Now.ToString(), null);
                flag = true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Exception while sending SMS : ", ex);
                flag = false;
            }
            return flag;
        }
        #endregion

    }
}
