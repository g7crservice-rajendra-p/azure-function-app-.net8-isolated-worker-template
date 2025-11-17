using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole
{
    public class LDMMessageProcessor
    {

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<LDMMessageProcessor> _logger;

        public LDMMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory, ILogger<LDMMessageProcessor> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        public bool DecodeReceiveLDMMessage(string strMsg, ref MessageData.LDMInfo ldm, int srno)
        {
            bool flag = false;
            string lastrec = string.Empty;
            strMsg = strMsg.Replace("\r\n", "$");
            string[] arrLDMMsg = strMsg.Split('$');
            try
            {
                for (int i = 0; i < arrLDMMsg.Length; i++)
                {
                    if (arrLDMMsg[i].StartsWith("LDM", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] msg = arrLDMMsg[i].Split('/');
                    }
                    if (i == 1)
                    {
                        string[] msg = arrLDMMsg[i].Split('/');
                        ldm.flightno = msg[0];
                        string[] tailno = msg[1].Split('.');
                        ldm.flightdate = tailno[0];
                        ldm.tailno = tailno[1];
                        ldm.paxcapacity = tailno[2].Substring(1);

                    }
                    if (i == 2)
                    {
                        string[] msg = arrLDMMsg[i].Split('/');
                        string[] dest = msg[0].Split('.');
                        ldm.flightdest = dest[0].Replace("-", "");

                    }
                    if (i == 3)
                    {
                        string[] msg = arrLDMMsg[i].Split('/');
                        string[] paxcount = msg[2].Split('.');
                        ldm.paxcount = paxcount[0];

                    }
                    if (i == 4)
                    {
                        string[] msg = arrLDMMsg[i].Split('/');
                        string[] bagweight = msg[1].Split(' ');
                        ldm.bagweight = bagweight[0];

                    }
                }
                flag = true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;

        }

        public async Task<(bool success, MessageData.LDMInfo ldm)> SaveandValidateLDMMessage(int srno, MessageData.LDMInfo ldm)
        {
            bool flag = false;
            string formattedDate = string.Empty;
            decimal Paxcapacitydata = decimal.Parse(ldm.paxcapacity);
            decimal Bagweightdata = decimal.Parse(ldm.bagweight);
            int PaxCount = int.Parse(ldm.paxcount);

            if (int.TryParse(ldm.flightdate, out int inputDay))
            {
                DateTime now = DateTime.Now;
                int year = now.Year;
                int month = now.Month;
                int daysInMonth = DateTime.DaysInMonth(year, month);
                int validDay = Math.Min(inputDay, daysInMonth);
                DateTime date = new DateTime(year, month, validDay, 0, 0, 0, 0);
                formattedDate = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            try
            {
                //string[] paramName = new string[] { "FlightNo", "Flightdate", "FlightDestination", "Paxcount", "Paxcapacity", "Bagweight", "TailNumber", "Srno" };
                //SqlDbType[] paramSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.Int };
                //object[] paramValue = new string[] { ldm.flightno, formattedDate, ldm.flightdest, PaxCount.ToString(), Paxcapacitydata.ToString(), Bagweightdata.ToString(), ldm.tailno, srno.ToString() };

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = ldm.flightno },
                    new SqlParameter("@Flightdate", SqlDbType.VarChar) { Value = formattedDate },
                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = ldm.flightdest },
                    new SqlParameter("@Paxcount", SqlDbType.Int) { Value = PaxCount.ToString() },
                    new SqlParameter("@Paxcapacity", SqlDbType.Decimal) { Value = Paxcapacitydata.ToString() },
                    new SqlParameter("@Bagweight", SqlDbType.Decimal) { Value = Bagweightdata.ToString() },
                    new SqlParameter("@TailNumber", SqlDbType.VarChar) { Value = ldm.tailno },
                    new SqlParameter("@Srno", SqlDbType.Int) { Value = srno.ToString() }
                };


                //SQLServer sqlServer = new SQLServer();
                //sqlServer.SelectRecords("uspSaveLDMMessage", paramName, paramValue, paramSqlType);

                await _readWriteDao.ExecuteNonQueryAsync("uspSaveLDMMessage", sqlParameters);
                flag = true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return (flag, ldm);
        }
    }
}
