using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Util
{
    /// <summary>
    /// 一次图数据查询器
    /// </summary>
    public class GraphQueryHelper
    {
        private Dictionary<int, List<Models.RealTimeValue>> m_ValueDictionary;

        public List<Models.RealTimeValue> GetCurrentValue()
        {
           string currentvlaue =  RedisHelper.StringGet("Current");
            m_ValueDictionary = JsonConvert.DeserializeObject<Dictionary<int, List<Models.RealTimeValue>>>(currentvlaue);

            List<Models.RealTimeValue> list = new List<Models.RealTimeValue>();

            foreach (int key  in m_ValueDictionary.Keys)
            {
                list.AddRange(m_ValueDictionary[key]);
            }

            return list;
        }

    }
}
