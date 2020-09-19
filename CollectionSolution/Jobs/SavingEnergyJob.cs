using CollectionData;
using CollectionData.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionSolution.Jobs
{
    public class SavingEnergyJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            string mysqlConnectionString = ConfigurationManager.ConnectionStrings["MySQLString"].ConnectionString;
            CollectionData.Util.GraphQueryHelper graphHelper = new CollectionData.Util.GraphQueryHelper();
            MySQLHelper helper = new MySQLHelper();

            List<RealTimeValue> list =  graphHelper.GetCurrentValue();

            DateTime now = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,DateTime.Now.Hour,DateTime.Now.Minute,0);

            try
            {
                string buildId = "000001G007";

                List<RealTimeValue> energyList = list.FindAll(x=>x.ParamCode=="EPI");

                List<EnergyData> insertList = new List<EnergyData>();

                foreach (var item in energyList)
                {
                    insertList.Add(new EnergyData() { BuildId= buildId ,MeterCode=item.MeterCode,Time=now,Value=item.Value,Calced=false});
                }

                int result = helper.InsertEnergyData(insertList, mysqlConnectionString);
            }
            catch(Exception e)
            {
                Config._log.Error(e.Message);
                Config._log.Error(e.StackTrace);
            }
            return Task.CompletedTask;
        }
    }
}
