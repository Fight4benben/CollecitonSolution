using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Application
{
    public class MServer
    {
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
        /// 服务器端口
        /// </summary>
        public int Port { get; set; }

        public List<MRegister> RegisterParams { get; set; }
    }
}
