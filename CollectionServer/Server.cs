using CollectionData;
using Modbus.Device;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollectionServer
{
    public class Server
    {
        /// <summary>
        /// 字段：是否正在运行该线程，通过属性来修改该字段
        /// </summary>
        private bool m_IsRunning;

        public bool IsRunning { get => m_IsRunning; set => m_IsRunning = value; }

        public RichTextBox m_rtbLog;
        private delegate void ShowLogEventHandler(string info);

        /// <summary>
        /// 启动状态：true->启动成功，false->启动失败
        /// </summary>
        private bool m_StartState;

        /// <summary>
        /// Mysql数据库帮助类：用于读取ModbusTCP配置的基本信息
        /// </summary>
        private CollectionData.MySQLHelper m_SqlHelper;

        /// <summary>
        /// ModbusTCP服务端列表
        /// </summary>
        private List<CollectionData.Communication.MServer> m_ServerList;

        /// <summary>
        /// 存放ModbusTCP连接的信息
        /// </summary>
        private Dictionary<int, ModbusIpMaster> m_ModbusMasterDic;
        /// <summary>
        /// 存放用于解析ModbusTCP报文的实体类
        /// </summary>
        private Dictionary<int, List<CollectionData.Application.MRegister>> m_RegisterDic;

        /// <summary>
        /// 连接中断
        /// </summary>
        private Dictionary<int, bool> m_ConnectErrorDic;

        public Dictionary<int, List<CollectionData.Models.RealTimeValue>> m_ValueDictionary = new Dictionary<int, List<CollectionData.Models.RealTimeValue>>();

        public Server()
        {
            InitBaseInfo();
        }

        /// <summary>
        /// 初始化基本信息
        /// </summary>
        private void InitBaseInfo()
        {
            m_ModbusMasterDic = new Dictionary<int, ModbusIpMaster>();
            m_RegisterDic = new Dictionary<int, List<CollectionData.Application.MRegister>>();
            m_ConnectErrorDic = new Dictionary<int, bool>();
            
        }

        /// <summary>
        /// 启动时用于检测各项数据与TCP连接是否正常
        /// </summary>
        public bool StartUpChecking()
        {
            ShowLog("开始检查系统运行信息...");
            
            ShowLog("ModbusTCP服务连接测试...");
            try
            {
                InitBaseInfo();

                string connectionString = ConfigurationManager.ConnectionStrings["MySQLString"].ConnectionString;
                m_SqlHelper = new CollectionData.MySQLHelper();
                m_ServerList = m_SqlHelper.GetCommunicationServers(connectionString);
                foreach (var item in m_ServerList)
                {
                    TcpClient client = new TcpClient(item.Address, item.Port);
                    m_ModbusMasterDic.Add(item.ID, ModbusIpMaster.CreateIp(client));
                    m_RegisterDic.Add(item.ID, m_SqlHelper.GetRegisters(item.ID, connectionString));

                    ShowLog($"<服务：{item.Name}，IP：{item.Address}，端口:{item.Port}>启动成功！");
                }

                m_StartState = true;
            }
            catch (Exception er)
            {
                m_StartState = false;
                ShowLog($"ModbusTCP服务启动失败,原因：<{er.Message}>。");
            }

            if (!m_StartState)
            {
                ShowLog("ModbusTCP服务启动失败，请检查原因！");
                return false;
            } 
            else
            {
                ShowLog("ModbusTCP服务启动成功,即将开始数据采集服务！");
                return true;
            }
                
        }

        public void RunCollectionServer()
        {
            ShowLog("采集循环任务已启动...");
            while (m_IsRunning)
            {
                Thread.Sleep(2000);

                //redis 存储上一次值
                if (m_ValueDictionary.Count != 0)
                {
                    SaveDataToRedis("Last");
                }

                //通过TCP获取数值并解析存储到变量m_RegisterDic中
                AccessAndAnaylysis();

                //redis存储当前数值
                SaveDataToRedis("Current");
                ShowLog("数据已经存储到实时库...");
            }

            ShowLog("采集任务已停止,等待下一次重启");
        }

        /// <summary>
        /// 读取并解析ModbusTCP数据
        /// </summary>
        private void AccessAndAnaylysis()
        {
            foreach (var item in m_ServerList)
            {
                //1.判断当前Server的TCPClient是否正常

                //2.核心逻辑：读取并解析
                List<ushort> list = new List<ushort>();
                List<CollectionData.Models.RealTimeValue> finalList = new List<CollectionData.Models.RealTimeValue>();

                foreach (CollectionData.Communication.MPacket packet in item.Packets)
                {
                    try
                    {
                        ushort[] array = m_ModbusMasterDic[item.ID].ReadHoldingRegisters(item.SlaveID, packet.StartAddress, packet.TotalCount);

                        list.AddRange(array);
                    }
                    catch (Exception er)
                    {
                        ushort[] emptyarray = new ushort[packet.TotalCount];
                        list.AddRange(emptyarray);

                        if (!m_ConnectErrorDic.ContainsKey(item.ID))
                            m_ConnectErrorDic.Add(item.ID, false);
                        else
                            m_ConnectErrorDic[item.ID] = false;

                        break;
                    }
                    
                }

                //解析当前服务器读取的数据,当前server对应的值为false时表示通讯中断，不解析数据；true表示通讯正常
                if (m_ConnectErrorDic.ContainsKey(item.ID) && !m_ConnectErrorDic[item.ID])
                    continue;

                foreach (var register in m_RegisterDic[item.ID])
                {
                    ushort[] tempShorts = new ushort[register.WordCount];

                    for (int i = 0; i < tempShorts.Length; i++)
                    {
                        tempShorts[i] = list[register.Address + i];
                    }

                    float value = 0.0f;
                    switch (register.DataType)
                    {
                        case CollectionData.Models.MDataType.Float:
                            value = CollectionData.Util.DataTypeChange.Convert2Float(tempShorts, false);
                            break;
                    }

                    finalList.Add(new CollectionData.Models.RealTimeValue()
                    {
                        MeterCode = register.MeterCode,
                        ParamCode = register.ParamCode,
                        Value = value
                    });
                }

                if (!m_ValueDictionary.ContainsKey(item.ID))
                    m_ValueDictionary.Add(item.ID, finalList);
                else
                    m_ValueDictionary[item.ID] = finalList;

            }

        }

        private void SaveDataToRedis(string keyName)
        {
            string jsonString = JsonConvert.SerializeObject(m_ValueDictionary);
            RedisHelper.StringSet(keyName, jsonString);
        }

        public void ShowLog(string info)
        {
            if (m_rtbLog == null)
                return;


            if (m_rtbLog.InvokeRequired)
            {
                ShowLogEventHandler showLogHandler = new ShowLogEventHandler(ShowLog);
                m_rtbLog.BeginInvoke(showLogHandler, info);
            }
            else
            {
                string s = "";
                for (int i = 0; i < m_rtbLog.Lines.Length; i++)
                {
                    if (i > 300)
                        break;
                    s += ("\r\n" + m_rtbLog.Lines[i]);
                }

                m_rtbLog.Text = (DateTime.Now.ToString("HH:mm:ss.fff") + " >>>   " + info + s);
            }
        }
    }
}
