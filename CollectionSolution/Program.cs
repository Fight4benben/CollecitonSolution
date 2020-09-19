using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollectionSolution
{
    static class Program
    {
        private static Mutex mutex;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           

            //防止重复运行软件
            mutex = new System.Threading.Mutex(true, "CollectSolution");
            if (mutex.WaitOne(0, false))
            {
                RunProgramRunExample().GetAwaiter().GetResult();
                Application.Run(new View());
            }
            else
            {
                MessageBox.Show(" 软件已运行！请勿重复运行此软件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
        }


        private static async Task RunProgramRunExample()
        {
            try
            {

                // Grab the Scheduler instance from the Factory
                //NameValueCollection props = new NameValueCollection
                //{
                //    { "quartz.serializer.type", "binary" }
                //};
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();


            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }
    }
}
