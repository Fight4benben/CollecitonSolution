using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData
{
    public class RedisHelper
    {
        private static ConnectionMultiplexer m_Redis;

        static RedisHelper()
        {
            if (m_Redis == null || !m_Redis.IsConnected)
            {
                m_Redis = ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["redis"].ConnectionString);
            }
        }

        /// <summary>
        /// 设置指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StringSet(string key, string value)
        {
            return m_Redis.GetDatabase().StringSet(key, value);
        }

        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string StringGet(string key)
        {
            return m_Redis.GetDatabase().StringGet(key);
        }

        public static bool KeyDelete(string key)
        {
            return m_Redis.GetDatabase().KeyDelete(key);
        }
    }
}
