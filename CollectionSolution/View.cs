using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollectionSolution
{
    public partial class View : Form
    {
        public View()
        {
            InitializeComponent();
            StopOperation();
            this.server.m_rtbLog = this.rtbLog;
            
        }
        //点击启动按钮后记录开始时间
        private TimeSpan m_StartSpan;
        private CollectionServer.Server server = new CollectionServer.Server();

        private List<CollectionData.Models.RealTimeValue> m_RealTimeList = new List<CollectionData.Models.RealTimeValue>();
        private BindingList<CollectionData.Models.RealTimeValue> m_BindingList;

        private void View_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.notifyIcon1.Visible = true;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            SetStartTime();

            bool result = server.StartUpChecking();

            if (result)
            {
                StartOperation();
                server.IsRunning = true;
                Thread serverThread = new Thread(server.RunCollectionServer);
                serverThread.Start();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            server.IsRunning = false;
            this.timerRun.Enabled = false;
            StopOperation();
        }

        private void timerRun_Tick(object sender, EventArgs e)
        {
            TimeSpan currentSpan = new TimeSpan(DateTime.Now.Ticks);

            TimeSpan totalSpan = currentSpan.Subtract(m_StartSpan).Duration();

            this.lblTotalTime.Text = totalSpan.Days + "天" + totalSpan.Hours + "时" + totalSpan.Minutes + "分" + totalSpan.Seconds + "秒";
        }

        private void StartOperation()
        {
            this.btnStart.Enabled = false;
            this.btnStop.Enabled = true;
        }

        private void StopOperation()
        {
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }

        private void SetStartTime()
        {
            string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.lblStartTime.Text = startTime;
            this.lblStartTime.ForeColor = Color.Red;
            m_StartSpan = new TimeSpan(DateTime.Now.Ticks);

            this.timerRun.Enabled = true;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                exitMenu.Show(MousePosition);
            }

            if (e.Button == MouseButtons.Left)
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.notifyIcon1.Visible = true;
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.ShowInTaskbar = false;
                this.notifyIcon1.Visible = true;
                this.Hide();
            }
        }

        private void View_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon1.Visible = true;
                this.WindowState = FormWindowState.Minimized;
                this.Visible = false;
                this.ShowInTaskbar = false;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (m_RealTimeList.Count == 0)
            {
                foreach (int key in server.m_ValueDictionary.Keys)
                {
                    m_RealTimeList.AddRange(server.m_ValueDictionary[key]);
                }

                m_BindingList = new BindingList<CollectionData.Models.RealTimeValue>(m_RealTimeList);

                realTimeData.DataSource = m_BindingList;
            }

            foreach (int key in server.m_ValueDictionary.Keys)
            {
                foreach (var item in server.m_ValueDictionary[key])
                {
                   CollectionData.Models.RealTimeValue rValue = m_RealTimeList.Find(realValue=>realValue.MeterCode == item.MeterCode && realValue.ParamCode == item.ParamCode);

                    if (rValue != null)
                    {
                        if (rValue.Value != item.Value)
                        {
                            int index = m_RealTimeList.FindIndex(realValue => realValue.MeterCode == item.MeterCode && realValue.ParamCode == item.ParamCode);
                            m_RealTimeList[index].Value = item.Value;
                        }
                    }
                }
            }

            realTimeData.Refresh();

        }
    }
}
