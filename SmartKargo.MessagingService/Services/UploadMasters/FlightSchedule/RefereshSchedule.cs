using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QidWorkerRole
{
   public class RefereshSchedule
    {
       SQLServer sqlServer = new SQLServer();

        public Boolean GetRefereshSchedule()
        {
            sqlServer.SelectRecords("uspRefreshAirlineScheduleRouteForecast");
            return true;
        }
    }
}
