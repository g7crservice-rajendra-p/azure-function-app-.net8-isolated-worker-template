using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole
{
    public class CGOProcessor
    {
        //string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();
        //const string PAGE_NAME = "CGOProcessor";

        #region

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<CGOProcessor> _logger;
        public CGOProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<CGOProcessor> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion

        #region Decode Receive CGO Message
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cgoMsg"></param>
        /// <param name="ActualCapacity"></param>
        /// <returns></returns>
        /// 

        /*removed ActualCapacity as it is not in use*/
        //public async Task<bool> DecodeReceiveCGOMessage(string cgoMsg, ref float ActualCapacity)
        public async Task<bool> DecodeReceiveCGOMessage(string cgoMsg)
        {
            bool flag = false;
            //MessageData.AWBBuildBUP awbBup = new MessageData.AWBBuildBUP("");
            //MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
            try
            {
                string FlightNo = string.Empty, FlightOrigin = string.Empty, AllocatedSpace = "0", FlightDate = string.Empty, AircraftType = string.Empty, Month, Day;

                try
                {
                    if (cgoMsg.StartsWith("CGO", StringComparison.OrdinalIgnoreCase))
                    {
                        //cgoMsg = cgoMsg.Replace("\r\n", "$");
                        string[] str = cgoMsg.Split('$');
                        if (str.Length >= 5)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {

                                flag = true;

                                #region Line 1

                                if (str[i].StartsWith("CGO", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                }
                                #endregion
                                if (i == 1)
                                {
                                    string[] msg = str[i].Split('/');
                                    FlightNo = msg[0];
                                    Day = msg[1].Substring(0, 2);
                                    Month = msg[1].Substring(2);
                                    #region Switch FlightMonth
                                    string FlightMonth = "";
                                    switch (Month.Trim().ToUpper())
                                    {
                                        case "JAN":
                                            {
                                                FlightMonth = "01";
                                                break;
                                            }
                                        case "FEB":
                                            {
                                                FlightMonth = "02";
                                                break;
                                            }
                                        case "MAR":
                                            {
                                                FlightMonth = "03";
                                                break;
                                            }
                                        case "APR":
                                            {
                                                FlightMonth = "04";
                                                break;
                                            }
                                        case "MAY":
                                            {
                                                FlightMonth = "05";
                                                break;
                                            }
                                        case "JUN":
                                            {
                                                FlightMonth = "06";
                                                break;
                                            }
                                        case "JUL":
                                            {
                                                FlightMonth = "07";
                                                break;
                                            }
                                        case "AUG":
                                            {
                                                FlightMonth = "08";
                                                break;
                                            }
                                        case "SEP":
                                            {
                                                FlightMonth = "09";
                                                break;
                                            }
                                        case "OCT":
                                            {
                                                FlightMonth = "10";
                                                break;
                                            }
                                        case "NOV":
                                            {
                                                FlightMonth = "11";
                                                break;
                                            }
                                        case "DEC":
                                            {
                                                FlightMonth = "12";
                                                break;
                                            }
                                        default:
                                            {
                                                FlightMonth = "00";
                                                break;
                                            }
                                    }
                                    FlightDate = FlightMonth.PadLeft(2, '0') + "/" + Day.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

                                    //******** Modified by Vishal on 28 DEC 2015 to resolve issue of next year in flight date.
                                    //Find out if flight date with current year is less than server date time by at least 100 days.
                                    DateTime dtFlightDate = DateTime.Now;
                                    if (DateTime.TryParseExact(FlightDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFlightDate))
                                    {
                                        if (DateTime.Now.AddDays(-100) > dtFlightDate)
                                        {   //Advance year in flight date to next year.
                                            dtFlightDate = dtFlightDate.AddYears(1);
                                            FlightDate = dtFlightDate.ToString("MM/dd/yyyy");
                                        }
                                    }
                                    //******** Modified by Vishal on 28 DEC 2015 to resolve issue of next year in flight date.

                                    #endregion
                                    //FlightDate = msg[1];
                                    FlightOrigin = msg[2];
                                    AircraftType = msg[3];
                                }
                                if (i == 5)
                                {
                                    string[] msg = str[i].Split(' ');
                                    AllocatedSpace = msg[msg.Length - 1].ToString();
                                }

                            }
                        }
                    }

                    flag = await UpdateCapacityFromCGOMessage(FlightNo, DateTime.Parse(FlightDate), FlightOrigin, AircraftType, float.Parse(AllocatedSpace));
                }
                catch (Exception)
                {
                    flag = false;
                }
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
        public async Task<bool> UpdateCapacityFromCGOMessage(string FlightNo, DateTime FlightDate, string FlightOrigin, string AircraftType, float CargoCapacity)
        {
            //SQLServer dtb = new SQLServer();

            try
            {
                //string[] QueryNames = new string[5];
                //object[] QueryValues = new object[5];
                //SqlDbType[] QueryTypes = new SqlDbType[5];

                //QueryNames[0] = "FlightNo";
                //QueryNames[1] = "FlightDate";
                //QueryNames[2] = "FlightOrigin";
                //QueryNames[3] = "AircraftType";
                //QueryNames[4] = "CargoCapacity";


                //QueryValues[0] = FlightNo;
                //QueryValues[1] = FlightDate;
                //QueryValues[2] = FlightOrigin;
                //QueryValues[3] = AircraftType;
                //QueryValues[4] = CargoCapacity;


                //QueryTypes[0] = SqlDbType.VarChar;
                //QueryTypes[1] = SqlDbType.VarChar;
                //QueryTypes[2] = SqlDbType.VarChar;
                //QueryTypes[3] = SqlDbType.VarChar;
                //QueryTypes[4] = SqlDbType.Float;

                //if (dtb.ExecuteProcedure("uspUdateCargoCapacityfromCGO", QueryNames, QueryTypes, QueryValues))
                //{
                //    return true; 
                //}
                //else
                //{
                //    return false;
                //}

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = FlightDate },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin },
                    new SqlParameter("@AircraftType", SqlDbType.VarChar) { Value = AircraftType },
                    new SqlParameter("@CargoCapacity", SqlDbType.Float) { Value = CargoCapacity }
                };

                return await _readWriteDao.ExecuteNonQueryAsync("uspUdateCargoCapacityfromCGO", parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Error while save Cargo Capacity via CGO Msg " + FlightNo + ex);
                return false;
            }
            #endregion
        }
    }
}
