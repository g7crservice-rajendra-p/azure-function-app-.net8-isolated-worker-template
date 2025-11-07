using System.Data;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using Microsoft.Data.SqlClient;

namespace QidWorkerRole
{
    public class AWBDetailsAPI
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<EMAILOUT> _logger;
        GenericFunction genericFunction = new GenericFunction();

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public AWBDetailsAPI(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<EMAILOUT> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion

        public async Task GetAWBDetails(string awbPrefix, string awbNumber)
        {
            DataSet dsAWBDeatils = new DataSet();
            AWBDetails objawbDetails = new AWBDetails();
            List<Flight> Flight = new List<Flight>();
            List<Itinerary> Itinerary = new List<Itinerary>();
            List<PieceDetail> pieceDetails = new List<PieceDetail>();

            try
            {
                //SQLServer db = new SQLServer();
                //string[] QueryNames = { "AWBprefix", "AWBNumber", };
                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                //string[] QueryValues = { awbPrefix, awbNumber };
                //dsAWBDeatils = db.SelectRecords("USPGetAWBDetails", QueryNames, QueryValues, QueryTypes);

                DataSet? dsAWBdetails = new DataSet();
                StringBuilder[] sb = new StringBuilder[0];
                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBprefix", SqlDbType.VarChar) { Value = awbPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber }
                };

                dsAWBDeatils = await _readWriteDao.SelectRecords("USPGetAWBDetails", parameters);

                if (dsAWBDeatils != null)
                {
                    if (dsAWBDeatils.Tables.Count > 1 && dsAWBDeatils.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsAWBDeatils.Tables[0].Rows[0];
                        objawbDetails.awbPrefix = Convert.ToString(dr["AWBPrefix"]);
                        objawbDetails.awbNumber = Convert.ToString(dr["AWBNumber"]);

                        Itinerary = new List<Itinerary>();
                        Itinerary objItinerary = new Itinerary();
                        Flight = new List<Flight>();
                        objItinerary.originAirportCode = Convert.ToString(dr["OriginCode"]);
                        objItinerary.destinationAirportCode = Convert.ToString(dr["DestinationCode"]);
                        if (dsAWBDeatils.Tables.Count > 1 && dsAWBDeatils.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow drAWBDetails in dsAWBDeatils.Tables[1].Rows)
                            {
                                Flight objFlight = new Flight();
                                objFlight.flightId = Convert.ToString(drAWBDetails["FltNumber"]);
                                objFlight.originAirportCode = Convert.ToString(drAWBDetails["FltOrigin"]);
                                objFlight.destinationAirportCode = Convert.ToString(drAWBDetails["FltDestination"]);
                                objFlight.departureDate = Convert.ToDateTime(drAWBDetails["FlightSchDept"]).ToString("ddMMyyyy");
                                objFlight.departureTime = Convert.ToDateTime(drAWBDetails["FlightSchDept"]).ToString("HH:mm");
                                objFlight.arrivalDate = Convert.ToDateTime(drAWBDetails["FlightSchArr"]).ToString("ddMMyyyy");
                                objFlight.arrivateTime = Convert.ToDateTime(drAWBDetails["FlightSchArr"]).ToString("HH:mm");
                                Flight.Add(objFlight);
                            }
                        }

                        objItinerary.flights = Flight;
                        objawbDetails.itinerary = objItinerary;

                        CargoDetails objcargoDetails = new CargoDetails();
                        objcargoDetails.totalPieces = Convert.ToInt16(dr["PiecesCount"]);
                        objcargoDetails.grossWeight = Convert.ToDouble(dr["GrossWeight"]);
                        objcargoDetails.volume = Convert.ToDouble(dr["Volume"]);
                        objcargoDetails.chargeWeight = Convert.ToDouble(dr["ChargedWeight"]);
                      
                        if (dsAWBDeatils.Tables.Count > 2 && dsAWBDeatils.Tables[2].Rows.Count > 0)
                        {
                            foreach (DataRow drAWBDetails in dsAWBDeatils.Tables[2].Rows)
                            {
                                PieceDetail objPieceDetail = new PieceDetail();
                                objPieceDetail.numberOfPieces = Convert.ToInt16(drAWBDetails["PcsCount"]);
                                objPieceDetail.grossWeight = Convert.ToDouble(drAWBDetails["GrossWt"]);
                                objPieceDetail.lengthInCm = Convert.ToDouble(drAWBDetails["Length"]);
                                objPieceDetail.breadthInCm = Convert.ToDouble(drAWBDetails["Breadth"]);
                                objPieceDetail.heightInCm = Convert.ToDouble(drAWBDetails["Height"]);
                                objPieceDetail.volume = Convert.ToDouble(drAWBDetails["Volume"]);
                                objPieceDetail.volumeWeight = Convert.ToDouble(drAWBDetails["Weight"]);
                                pieceDetails.Add(objPieceDetail);
                            }
                        }

                        objcargoDetails.pieceDetails = pieceDetails;

                        Spec objspec = new Spec();
                        List<string> commodities = dr["CommodityCode"].ToString().Split(',').ToList();
                        List<string> shc = dr["SHCCodes"].ToString().Split(',').ToList();
                        List<string> productType = dr["ProductType"].ToString().Split(',').ToList();

                        objspec.commodities = commodities;
                        objspec.shc = shc;
                        objspec.productType = productType;
                        objspec.priority = dr["ShipmentPriority"].ToString();

                        objcargoDetails.spec = objspec;
                        objawbDetails.cargoDetails = objcargoDetails;

                        Shipper objshipper = new Shipper();
                        objshipper.id = Convert.ToString(dr["ShipperAccCode"]);
                        objshipper.name = Convert.ToString(dr["ShipperName"]);
                        objshipper.address = Convert.ToString(dr["ShipperAddress"]);
                        objshipper.countryCode = Convert.ToString(dr["ShipperCountryCode"]);
                        objshipper.country = Convert.ToString(dr["ShipperCountryName"]);
                        objshipper.state = Convert.ToString(dr["ShipperState"]);
                        objshipper.city = Convert.ToString(dr["ShipperCity"]);
                        objshipper.zipCode = Convert.ToString(dr["ShipperPinCode"]);
                        objshipper.gstin = Convert.ToString(dr["ShipperGSTINNo"]);
                        objshipper.aeo = Convert.ToString(dr["ShipAEONum"]);
                        objshipper.phone = Convert.ToString(dr["ShipperTelephone"]);
                        objshipper.email = Convert.ToString(dr["ShipperEmailId"]);
                        objshipper.contactPersonName = Convert.ToString(dr["ShipContactPerson"]);
                        objshipper.contactPersonPhone = Convert.ToString(dr["ShipContactTelephone"]);

                        Consignee objconsignee = new Consignee();

                        objconsignee.id = Convert.ToString(dr["ConsigAccCode"]);
                        objconsignee.name = Convert.ToString(dr["ConsigneeName"]);
                        objconsignee.address = Convert.ToString(dr["ConsigneeAddress"]);
                        objconsignee.countryCode = Convert.ToString(dr["ConsigneeCountryCode"]);
                        objconsignee.country = Convert.ToString(dr["ConsigneeCountryName"]);
                        objconsignee.state = Convert.ToString(dr["ConsigneeState"]);
                        objconsignee.city = Convert.ToString(dr["ConsigneeCity"]);
                        objconsignee.zipCode = Convert.ToString(dr["ConsigneePincode"]);
                        objconsignee.gstin = Convert.ToString(dr["ConsigneeGSTINNo"]);
                        objconsignee.aeo = Convert.ToString(dr["ConsAEONum"]);
                        objconsignee.phone = Convert.ToString(dr["ConsigneeTelephone"]);
                        objconsignee.email = Convert.ToString(dr["ConsigEmailId"]);
                        objconsignee.contactPersonName = Convert.ToString(dr["ConsContactPerson"]);
                        objconsignee.contactPersonPhone = Convert.ToString(dr["ConsContactTelephone"]);

                        Extras objextras = new Extras();
                        objextras.id = Convert.ToString(dr["ConsigAccCode"]);
                        objextras.name = Convert.ToString(dr["NotifyName"]);
                        objextras.address = Convert.ToString(dr["NotifyAddress"]);
                        objextras.country = Convert.ToString(dr["NotifyCountry"]);
                        objextras.state = Convert.ToString(dr["NotifyState"]);
                        objextras.city = Convert.ToString(dr["NotifyCity"]);
                        objextras.zipCode = Convert.ToString(dr["NotifyPincode"]);
                        objextras.hsCode = Convert.ToString(dr["HSCode"]);
                        objextras.phone = Convert.ToString(dr["NotifyTelephone"]);
                        objextras.gstin = Convert.ToString(dr["ConsigneeGSTINNo"]);
                        objextras.aeo = Convert.ToString(dr["ConsAEONum"]);
                        objextras.phone = Convert.ToString(dr["ConsigneeTelephone"]);
                        objextras.email = Convert.ToString(dr["NotifyEmailId"]);
                        objextras.contactPersonName = Convert.ToString(dr["ConsContactPerson"]);
                        objextras.contactPersonPhone = Convert.ToString(dr["ConsContactTelephone"]);

                        ConsignmentDetails objConsignmentDetails = new ConsignmentDetails();
                        objConsignmentDetails.shipper = objshipper;
                        objConsignmentDetails.consignee = objconsignee;
                        objConsignmentDetails.extras = objextras;

                        Price objprices = new Price();
                        objprices.rateType = Convert.ToString(dr["RateType"]);
                        objprices.currencyCode = Convert.ToString(dr["Currency"]);
                        objprices.ratePerKg = Convert.ToDouble(dr["RatePerKg"]);
                        objprices.taxes = Convert.ToDouble(dr["MKTTax"]);
                        objprices.serviceCharge = Convert.ToDouble(dr["ServTax"]);
                        objprices.otherCharges = Convert.ToDouble(dr["OtherCharges"]);
                        objprices.totalAmount = Convert.ToDouble(dr["Total"]);

                        objawbDetails.consignmentDetails = objConsignmentDetails;
                        objawbDetails.price = objprices;
                        objawbDetails.bookingStatus = Convert.ToString(dr["BookingStatus"]);
                        objawbDetails.awbStatus = Convert.ToString(dr["AWBStatus"]);

                        //string JsonInput = new JavaScriptSerializer().Serialize(objawbDetails);
                        string JsonInput = System.Text.Json.JsonSerializer.Serialize(objawbDetails);
                        //Generate Access Token
                        string accessToken = await GenerateAccessToken();

                        string FCTAPIURL;
                        FCTAPIURL = genericFunction.GetConfigurationValues("FCTAPIURL").Trim();

                        HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
                        HttpClient client = new HttpClient(handler);
                        client.BaseAddress = new Uri(FCTAPIURL);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Uri.EscapeUriString(client.BaseAddress.ToString()));
                        request.Content = new StringContent(JsonInput, Encoding.UTF8, "application/json");
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpResponseMessage tokenResponse = client.PostAsync(Uri.EscapeUriString(client.BaseAddress.ToString()), request.Content).Result;
                        string APIError = "";
                        Boolean IsProcessed = false;
                        if (Convert.ToInt16(tokenResponse.StatusCode) == 200)
                        {
                            IsProcessed = true;
                            APIError = "";

                        }
                        else
                        {
                            var Response = tokenResponse.Content.ReadAsStringAsync().Result;
                            APIResponse objAPIResponse = JsonConvert.DeserializeObject<APIResponse>(Response);
                            List<Error> lstError = new List<Error>();

                            lstError = objAPIResponse.errors;
                            if (lstError.Count > 0)
                            {
                                foreach (Error objError in lstError)
                                {
                                    APIError = APIError + " Code: " + objError.code + "; title:" + objError.title + "; body:" + objError.body + "; description:" + objError.description;
                                }
                            }
                            IsProcessed = false;

                        }

                        //db = new SQLServer();
                        //db.SelectRecords("USPGetPendingAWBList", QueryNames1, QueryValues1, QueryTypes1);
                        //string[] QueryNames1 = { "IsUpdate", "AWBprefix", "AWBNumber", "IsProcessed", "APIError" };
                        //SqlDbType[] QueryTypes1 = { SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                        //object[] QueryValues1 = { true, awbPrefix, awbNumber, IsProcessed, APIError };


                        SqlParameter[] parametersValue =
                        {
                            new SqlParameter("@IsUpdate", SqlDbType.Bit)       { Value = true },
                            new SqlParameter("@AWBprefix", SqlDbType.VarChar)  { Value = awbPrefix },
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar)  { Value = awbNumber },
                            new SqlParameter("@IsProcessed", SqlDbType.Bit)    { Value = IsProcessed },
                            new SqlParameter("@APIError", SqlDbType.VarChar)   { Value = APIError }
                        };
                       await _readWriteDao.SelectRecords("USPGetPendingAWBList", parametersValue);

                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on GetAWBDetails.");
            }



        }

        public async Task GetPendingAWBList()
        {
           try
            {
                //SQLServer db = new SQLServer();
               // dsAWBlist = db.SelectRecords("USPGetPendingAWBList");

                DataSet? dsAWBlist = new DataSet();
                dsAWBlist = await _readWriteDao.SelectRecords("USPGetPendingAWBList");

                if (dsAWBlist != null && dsAWBlist.Tables.Count > 0 && dsAWBlist.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drAWBDetails in dsAWBlist.Tables[0].Rows)
                    {
                       GetAWBDetails(Convert.ToString(drAWBDetails["AWBPrefix"]), Convert.ToString(drAWBDetails["AWBNumber"]));
                    }

                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on GetPendingAWBList.");
            }
        }

        public async Task<string> GenerateAccessToken()
        {
            AccessTokenResponse token = null;

            try
            {
                DataSet dsResult = new DataSet();
                string tokenKey = string.Empty;
                string tokenURL = string.Empty;
                string FCTClientId = string.Empty;

                string FCTClientSecret = string.Empty;
                try
                {
                    //Check Token is expired or not.
                    dsResult = await validateOrUpdateToken(tokenKey, "C");
                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    {
                        //If token is not expired return exsting Token key
                        if (Convert.ToString(dsResult.Tables[0].Rows[0]["IsExpired"]).Equals("N"))
                        {
                            tokenKey = dsResult.Tables[0].Rows[0]["TokenKey"].ToString();
                            return tokenKey;
                        }

                        // If token is expired generate new Token
                        if (Convert.ToString(dsResult.Tables[0].Rows[0]["IsExpired"]).Equals("Y"))
                        {
                            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
                            HttpClient client = new HttpClient(handler);
                            tokenURL = genericFunction.GetConfigurationValues("FCTTokenURL").Trim();
                            FCTClientId = genericFunction.GetConfigurationValues("FCTClientID").Trim();
                            FCTClientSecret = genericFunction.GetConfigurationValues("FCTClientSecret").Trim();

                            client.BaseAddress = new Uri(tokenURL);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(
                                     System.Text.ASCIIEncoding.ASCII.GetBytes(
                                        $"{FCTClientId}:{FCTClientSecret}")));
                            string body = "grant_type=client_credentials&scope=public";
                            client.BaseAddress = new Uri(tokenURL);
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
                            request.Content = new StringContent(body,
                                                                Encoding.UTF8,
                                                                "application/x-www-form-urlencoded");//CONTENT-TYPE header

                            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                            postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

                            request.Content = new FormUrlEncodedContent(postData);
                            HttpResponseMessage tokenResponse = client.PostAsync(tokenURL, new FormUrlEncodedContent(postData)).Result;

                            var token1 = tokenResponse.Content.ReadAsStringAsync().Result;
                            token = JsonConvert.DeserializeObject<AccessTokenResponse>(token1);
                            tokenKey = token.access_token;
                            if (!string.IsNullOrEmpty(tokenKey))
                            {
                                //update new token key to database
                                dsResult = await validateOrUpdateToken(tokenKey, "U");
                                return tokenKey;
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on GenerateAccessToken.");
                    return "";
                }
                return tokenKey;


            }
            catch (HttpRequestException ex)
            {
                clsLog.WriteLogAzure(ex);
                return "";
            }

        }
        public class AccessTokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_expires_in { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }

        }

        public class Error
        {
            public string code { get; set; }
            public string title { get; set; }
            public string body { get; set; }
            public string description { get; set; }
        }

        public class APIResponse
        {
            public List<Error> errors { get; set; }
        }



        public async Task<DataSet> validateOrUpdateToken(string tokenKey, string flag)
        {
            DataSet? dsResult = new DataSet();
            //SQLServer sqlServer = new SQLServer();
            //dsResult = sqlServer.SelectRecords("uspUpdateTokenForFCTAPI", sqlParameter);
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] {
                     new SqlParameter("@TokenKey",tokenKey)
                    ,new SqlParameter("@Flag",flag)
                };
                dsResult = await _readWriteDao.SelectRecords("uspUpdateTokenForFCTAPI", sqlParameter);
                GC.Collect();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on validateOrUpdateToken.");
                return null;
            }
            return dsResult;
        }


    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CargoDetails
    {
        public int totalPieces { get; set; }
        public double grossWeight { get; set; }
        public double volume { get; set; }
        public double chargeWeight { get; set; }
        public List<PieceDetail> pieceDetails { get; set; }
        public Spec spec { get; set; }
    }

    public class Consignee
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string countryCode { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zipCode { get; set; }
        public string gstin { get; set; }
        public string aeo { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string contactPersonName { get; set; }
        public string contactPersonPhone { get; set; }
    }

    public class ConsignmentDetails
    {
        public Shipper shipper { get; set; }
        public Consignee consignee { get; set; }
        public Extras extras { get; set; }
    }

    public class Extras
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zipCode { get; set; }
        public string gstin { get; set; }
        public string aeo { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string contactPersonName { get; set; }
        public string contactPersonPhone { get; set; }
        public string hsCode { get; set; }
    }

    public class Flight
    {
        public string flightId { get; set; }
        public string originAirportCode { get; set; }
        public string destinationAirportCode { get; set; }
        public string departureDate { get; set; }
        public string departureTime { get; set; }
        public string arrivalDate { get; set; }
        public string arrivateTime { get; set; }
    }

    public class Itinerary
    {
        public string originAirportCode { get; set; }
        public string destinationAirportCode { get; set; }
        public List<Flight> flights { get; set; }
    }

    public class PieceDetail
    {
        public int numberOfPieces { get; set; }
        public double grossWeight { get; set; }
        public double lengthInCm { get; set; }
        public double breadthInCm { get; set; }
        public double heightInCm { get; set; }
        public double volume { get; set; }
        public double volumeWeight { get; set; }
    }

    public class Price
    {
        public string rateType { get; set; }
        public string currencyCode { get; set; }
        public double ratePerKg { get; set; }
        public double taxes { get; set; }
        public double serviceCharge { get; set; }
        public double otherCharges { get; set; }
        public double totalAmount { get; set; }
    }

    public class AWBDetails
    {
        public string awbPrefix { get; set; }
        public string awbNumber { get; set; }
        public Itinerary itinerary { get; set; }
        public CargoDetails cargoDetails { get; set; }
        public ConsignmentDetails consignmentDetails { get; set; }
        public Price price { get; set; }

        public string bookingStatus { get; set; }
        public string awbStatus { get; set; }
    }

    public class Shipper
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string countryCode { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zipCode { get; set; }
        public string gstin { get; set; }
        public string aeo { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string contactPersonName { get; set; }
        public string contactPersonPhone { get; set; }
    }

    public class Spec
    {
        public List<string> commodities { get; set; }
        public List<string> shc { get; set; }
        public List<string> productType { get; set; }
        public string priority { get; set; }
    }

}
