using CollectionData.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CollectionData
{
    public class MySQLHelper
    {
        public List<Communication.MServer> GetCommunicationServers(string connectionString)
        {
            List<Communication.MServer> lookup = new List<Communication.MServer>();
            using (IDbConnection conn = DapperConfig.GetSqlConnection(connectionString))
            {
                string querySql = @"
                                    SELECT 
                                    MServer.ID,MServer.Name,MServer.Address,MServer.Port,MServer.SlaveID ,
                                    MPacket.ServerID,MPacket.StartAddress,MPacket.TotalCount
                                    FROM EnergyDB.MServer 
                                    INNER JOIN MPacket on MServer.ID = MPacket.ServerID
                                    ";

                var list = conn.Query<Communication.MServer, Communication.MPacket, Communication.MServer>(querySql,
                    (server, packet) => {
                        Communication.MServer tmp;
                        if (lookup.Find(l => l.ID == server.ID) == null)
                        {

                            tmp = server;
                            tmp.Packets = new List<Communication.MPacket>();
                            lookup.Add(tmp);
                        }
                        else
                        {
                            lookup[lookup.FindIndex(l => l.ID == server.ID)].Packets.Add(packet);
                        }
                        server.Packets.Add(packet);
                        return server;
                    }, splitOn: "ServerID").ToList();

                return lookup;

            }
        }

        public List<Application.MRegister> GetRegisters(int serverId,string connectionString)
        {

            using (IDbConnection conn = DapperConfig.GetSqlConnection(connectionString))
            {
                string querySql = @"
                                   SELECT * FROM MRegister
                                   WHERE ServerID = @ServerID
                                   ";

                return conn.Query<Application.MRegister>(querySql, new { ServerID = serverId }).ToList();
            }
        }
    }
}
