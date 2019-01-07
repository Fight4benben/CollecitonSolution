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
                                    WHERE IsUse = 1
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

        public List<Application.MRegister> GetRegisters(int serverId, string connectionString)
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

        public List<Models.TopologicalGraph> GetAllGraph(string connectionString)
        {
            using (IDbConnection conn = DapperConfig.GetSqlConnection(connectionString))
            {
                string querySql = @"SELECT * FROM EnergyDB.TopologicalGraph;";

                return conn.Query<Models.TopologicalGraph>(querySql).ToList();
            }
        }

        public string GetSvgPath(int id, string connectionString)
        {
            using (IDbConnection conn = DapperConfig.GetSqlConnection(connectionString))
            {
                string querySql = @"SELECT Path FROM EnergyDB.TopologicalGraph 
                                    WHERE ID=@ID;";

                List<string> result =  conn.Query<string>(querySql, new { ID = id }).ToList();

                if (result.Count > 0)
                    return result[0];
                else
                    return "error";
            }
        }

        public List<CollectionData.Models.TopologicalRegister> GetTopologicalRegisters(int id, string connectionString)
        {
            using (IDbConnection conn = DapperConfig.GetSqlConnection(connectionString))
            {
                string querySql = @"SELECT MeterCode,ParamCode FROM EnergyDB.TopologicalRegister
                                    WHERE ID=@ID;";

                return conn.Query<CollectionData.Models.TopologicalRegister>(querySql, new { ID = id }).ToList();
            }
        }
    }
}
