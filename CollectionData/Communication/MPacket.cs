using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Communication
{
    public class MPacket
    {
        /// <summary>
        /// 关联的服务器ID
        /// </summary>
        public int ServerID { get; set; }
        /// <summary>
        /// NModbus4只能解析ushort类型的起始地址;
        /// 该属性表示该包的起始地址
        /// </summary>
        public ushort StartAddress { get; set; }

        /// <summary>
        /// 单包寄存器数量建议：100个
        /// </summary>
        public ushort TotalCount { get; set; }
    }
}
