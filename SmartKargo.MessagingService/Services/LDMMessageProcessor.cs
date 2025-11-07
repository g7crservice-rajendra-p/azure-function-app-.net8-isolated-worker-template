using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QidWorkerRole.MessageData;
using System.Data;
using QID.DataAccess;
using System.Configuration;
using System.Text.RegularExpressions;

namespace QidWorkerRole
{
    public class LDMMessageProcessor
    {

        public LDMMessageProcessor()
        {
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
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;

        }
        public bool SaveandValidateLDMMessage(int srno, ref MessageData.LDMInfo ldm)
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

                string[] paramName = new string[] { "FlightNo", "Flightdate", "FlightDestination", "Paxcount", "Paxcapacity", "Bagweight", "TailNumber", "Srno" };
                SqlDbType[] paramSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.Int };
                object[] paramValue = new string[] { ldm.flightno, formattedDate, ldm.flightdest, PaxCount.ToString(), Paxcapacitydata.ToString(), Bagweightdata.ToString(), ldm.tailno, srno.ToString() };
                SQLServer sqlServer = new SQLServer();
                sqlServer.SelectRecords("uspSaveLDMMessage", paramName, paramValue, paramSqlType);
                flag = true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }
    }
}
