using Microsoft.Extensions.Logging;
using System.Net;
using System.Xml.Linq;

namespace QidWorkerRole
{
    #region WebServiceCalss
    /// <summary>
    /// name: Sushant Gavas,
    /// Added webservice class
    /// AK-3714
    /// </summary>
    public class WebService
    {
        //public string Url { get; set; }
        //public string MethodName { get; set; }
        //public Dictionary<string, string> Params = new Dictionary<string, string>();
        //public XDocument ResultXML;
        //public string ResultString;
        //string UserName;
        //string Password;
        //string SoapRequest;

        private readonly ILogger<WebService> _logger;

        public WebService(ILogger<WebService> logger)
        {
            _logger = logger;
        }

        //public WebService(string url, string methodName, string userName, string password, string MessageBody)
        //{
        //    Url = url;
        //    MethodName = methodName;
        //    UserName = userName;
        //    Password = password;
        //    SoapRequest = MessageBody;
        //}

        /// <summary>
        /// Invokes service
        /// </summary>
        //public void Invoke(string customsName)
        //{
        //    Invoke(true, customsName);
        //}

        /// <summary>
        /// Invokes service
        /// </summary>
        /// <param name="encode">Added parameters will encode? (default: true)</param>
        //public void Invoke(bool encode)
        //{
        //    string soapStr = SoapRequest;         

        //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
        //    req.Headers.Add("SOAPAction", "\"http://tempuri.org/" + MethodName + "\"");
        //    req.ContentType = "text/xml;charset=\"utf-8\"";
        //    req.Accept = "text/xml";
        //    req.Method = "POST";
        //    req.Credentials = new NetworkCredential(UserName, Password);

        //    using (Stream stm = req.GetRequestStream())
        //    {              
        //        //soapStr = string.Format(soapStr, MethodName);
        //        using (StreamWriter stmw = new StreamWriter(stm))
        //        {
        //            stmw.Write(soapStr);
        //        }
        //    }

        //    using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
        //    {
        //        string result = responseReader.ReadToEnd();
        //        ResultXML = XDocument.Parse(result);
        //        ResultString = result.Replace("&lt;", "<");
        //    }
        //}

        //public void Invoke(bool encode, string customsName)
        //{
        //    string soapStr = SoapRequest;
        //    string strMsgName = MethodName;
        //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
        //    if (customsName.ToUpper() == "DAKAR")
        //    {
        //        req.Headers.Add("SOAPAction", "");
        //        req.Headers.Add("Username", UserName);
        //        req.Headers.Add("Password", Password);
        //    }
        //    else
        //    {
        //        req.Headers.Add("SOAPAction", strMsgName);
        //    }
        //    req.ContentType = "text/xml;charset=\"utf-8\"";
        //    req.Method = "POST";

        //    req.Credentials = new NetworkCredential(UserName, Password);

        //    try
        //    {
        //        using (Stream stm = req.GetRequestStream())
        //        {
        //            using (StreamWriter stmw = new StreamWriter(stm))
        //            {
        //                stmw.Write(soapStr);
        //            }
        //        }
        //        using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
        //        {
        //            string result = responseReader.ReadToEnd();

        //            ResultXML = XDocument.Parse(result);
        //            ResultString = result.Replace("&lt;", "<");
        //        }
        //    }
        //    catch (WebException webex)
        //    {
        //        clsLog.WriteLogAzure("WEBSERVICE Error" + webex.Response.ToString());
        //    }
        //}

        public string Invoke(string url, string methodName, string userName, string password, string messageBody, string customsName)
        {
            //string soapStr = SoapRequest;
            //string strMsgName = MethodName;
            string ResultString = string.Empty;
            XDocument ResultXML;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            //if (customsName.ToUpper() == "DAKAR")
            if (customsName.Equals("DAKAR", StringComparison.CurrentCultureIgnoreCase))
            {
                req.Headers.Add("SOAPAction", "");
                req.Headers.Add("Username", userName);
                req.Headers.Add("Password", password);
            }
            else
            {
                req.Headers.Add("SOAPAction", methodName);
            }
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Method = "POST";

            req.Credentials = new NetworkCredential(userName, password);

            try
            {
                using (Stream stm = req.GetRequestStream())
                {
                    using (StreamWriter stmw = new StreamWriter(stm))
                    {
                        //stmw.Write(soapStr);
                        stmw.Write(messageBody);
                    }
                }
                using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    string result = responseReader.ReadToEnd();
                    ResultXML = XDocument.Parse(result);
                    ResultString = result.Replace("&lt;", "<");
                }
            }
            catch (WebException webex)
            {
                //clsLog.WriteLogAzure("WEBSERVICE Error" + webex.Response.ToString());
                _logger.LogError(webex, "WEBSERVICE Error: {ErrorResponse}", webex?.Response?.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on Webservice Invoke");

            }
            return ResultString;
        }
    }
    #endregion
}
