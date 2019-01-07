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
            this.timerTopology.Enabled = false;

        }
        //点击启动按钮后记录开始时间
        private TimeSpan m_StartSpan;
        private CollectionServer.Server server = new CollectionServer.Server();
        private CollectionData.MySQLHelper helper = new CollectionData.MySQLHelper();

        private string mysqlConnectionString = ConfigurationManager.ConnectionStrings["MySQLString"].ConnectionString;

        private List<CollectionData.Models.RealTimeValue> m_RealTimeList = new List<CollectionData.Models.RealTimeValue>();
        private BindingList<CollectionData.Models.RealTimeValue> m_BindingList;

        private CollectionData.Util.GraphQueryHelper graphHelper = new CollectionData.Util.GraphQueryHelper();

        private Svg.SvgDocument m_SvgDocument;
        private string m_ImageBasePath = System.Environment.CurrentDirectory.ToString() + "\\Views\\";
        private string m_ImagePath;

        private List<CollectionData.Models.RealTimeValue> m_TopologicalValues = new List<CollectionData.Models.RealTimeValue>();
        private List<string> m_IdList = new List<string>();

        private int m_TopologicalID;

        private bool m_ViewStatus = true;

        private void View_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.notifyIcon1.Visible = true;

            LoadComponentData();

            
        }

        private void LoadComponentData()
        {
            this.cbTopology.SelectedIndexChanged -= cbTopology_SelectedIndexChanged;
            List<CollectionData.Models.TopologicalGraph> list = helper.GetAllGraph(mysqlConnectionString);
            this.cbTopology.DataSource = list;
            this.cbTopology.ValueMember = "ID";
            this.cbTopology.DisplayMember = "Name";

            m_TopologicalID = Convert.ToInt32(this.cbTopology.SelectedValue);

            this.cbTopology.SelectedIndexChanged += cbTopology_SelectedIndexChanged;
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

        private void cbTopology_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_TopologicalID = Convert.ToInt32(this.cbTopology.SelectedValue.ToString());
        }

        private void btnAutoRefresh_Click(object sender, EventArgs e)
        {
            if (btnAutoRefresh.Text == "自动刷新")
            {
                //开始启动自动刷新

                string svgName = helper.GetSvgPath(Convert.ToInt32(this.cbTopology.SelectedValue), mysqlConnectionString);
                if (svgName == "error")
                    return;

                m_ImagePath = m_ImageBasePath + svgName;
                CollectionData.Util.SVGParser.MaximumSize = new Size(this.picView.Width, this.picView.Height);
                m_SvgDocument = CollectionData.Util.SVGParser.GetSvgDocument(m_ImagePath);

                //1. 加载页面
                LoadData();
                //2. 启动自动刷新定时器
                this.timerTopology.Enabled = true;

                //3. 修改名称为"手动刷新"

                this.btnAutoRefresh.Text = "停止刷新";
            }
            else
            {
                //开始启动自动刷新

                //1. 停止自动刷新定时器
                this.timerTopology.Enabled = false;

                //2. 修改名称为"自动刷新"

                this.btnAutoRefresh.Text = "自动刷新";
            }
        }

        private void timerTopology_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        private void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshView();
        }

        private void RefreshView()
        {
            m_TopologicalValues.Clear();
            m_IdList.Clear();
            List<CollectionData.Models.TopologicalRegister> topologicalRegisters = helper.GetTopologicalRegisters(m_TopologicalID, mysqlConnectionString);
            List<CollectionData.Models.RealTimeValue> allValues = graphHelper.GetCurrentValue();

            foreach (var item in topologicalRegisters)
            {
                if (!m_IdList.Contains(item.MeterCode))
                    m_IdList.Add(item.MeterCode);

                CollectionData.Models.RealTimeValue tempValue = allValues.Find(value=>item.MeterCode==value.MeterCode && item.ParamCode==value.ParamCode);
                if (tempValue == null)
                    continue;

                m_TopologicalValues.Add(tempValue);
            }
        }

        private void LoadData()
        {
            if (!this.bgworker.IsBusy)
                this.bgworker.RunWorkerAsync();
        }

        private void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowView();
        }

        private void ShowView()
        {

            

            foreach (string id in m_IdList)
            {
                Svg.SvgElement group = m_SvgDocument.GetElementById(id);

                if (group == null)
                    continue;

                foreach (var item in group.Children)
                {
                    if (!item.ContainsAttribute("name"))
                        continue;

                    if (item is Svg.SvgText)
                    {
                        if ((item as Svg.SvgText).ContainsAttribute("name"))
                        {
                            var tempResult = m_TopologicalValues.Find(result => result.MeterCode == id && result.ParamCode.ToLower() == item.CustomAttributes["name"].ToString().ToLower());
                            if (tempResult != null)
                                switch ((item as Svg.SvgText).CustomAttributes["name"].ToString().ToLower())
                                {
                                    case "ua":
                                    case "ub":
                                    case "uc":
                                    case "uab":
                                    case "ubc":
                                    case "uca":
                                        (item as Svg.SvgText).Text = tempResult.Value + " V";
                                        break;
                                    case "ia":
                                    case "ib":
                                    case "ic":
                                        (item as Svg.SvgText).Text = tempResult.Value + " A";
                                        break;
                                }
                        }
                    }
                    else if(item is Svg.SvgGroup)
                    {
                        if ((item as Svg.SvgGroup).ContainsAttribute("name"))
                        {
                            if ((item as Svg.SvgGroup).CustomAttributes["name"].ToString().ToLower() == "on")
                                item.Opacity = 1;
                            else if ((item as Svg.SvgGroup).CustomAttributes["name"].ToString().ToLower() == "off")
                                item.Opacity = 0;

                        }
                    }
                }
                //if()
            }

            if (!m_ViewStatus)
                return;
            this.picView.Image = m_SvgDocument.Draw();
        }

        private void View_MinimumSizeChanged(object sender, EventArgs e)
        {
            this.timerTopology.Enabled = false;
            m_ViewStatus = false;
        }

        private void View_Shown(object sender, EventArgs e)
        {
            if (m_SvgDocument == null)
                return;
            this.timerTopology.Enabled = true;
            m_ViewStatus = true;
        }
    }
}
