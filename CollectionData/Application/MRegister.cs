using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Application
{
    public class MRegister
    {
        /// <summary>
        /// 关联ServerID
        /// </summary>
        public int ServerID { get; set; }
        /// <summary>
        /// 该寄存器的地址编码
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// 改参数对应的仪表编号
        /// </summary>
        public string MeterCode { get; set; }

        /// <summary>
        /// 寄存器表示的参数
        /// </summary>
        public string ParamCode { get; set; }
        /// <summary>
        /// 该参数对应的数据类型：Float
        /// </summary>
        public Models.MDataType DataType { get; set; }

        /// <summary>
        /// 参数对应的word类型寄存器个数1表示16位，2表示32位
        /// </summary>
        public int WordCount { get; set; }
    }
}
