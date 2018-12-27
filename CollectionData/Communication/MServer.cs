using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Communication
{
    public class MServer
    {
        public MServer()
        {
            this.Packets = new List<MPacket>();
        }
        /// <summary>
        /// 编号
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 服务器IPv4地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// TCP服务的SlaveID编号
        /// </summary>
        public byte SlaveID { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 一个服务器可以拆若干个包
        /// </summary>
        public List<MPacket> Packets { get; set; }
    }
}
