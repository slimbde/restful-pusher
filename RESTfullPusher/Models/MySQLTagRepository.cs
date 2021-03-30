using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using Dapper;

namespace RESTfullPusher
{
    internal interface IMySqlTagRepository
    {
        void CallDevUpdate(string deviceName, IDictionary<string, string> tags);
    }

    internal class MySqlRepository : IDisposable, IMySqlTagRepository
    {
        private IDbConnection _connection;

        public MySqlRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void CallDevUpdate(string parameter)
        {
            _connection.Query("dev_update", new { param1 = parameter }, commandType: CommandType.StoredProcedure);
        }

        public void CallDevUpdate(string deviceName, IDictionary<string, string> tags)
        {                
            var execString = deviceName + "~" + string.Join("|", tags.Select(t => t.Key + "=" + t.Value)) + "|";
            CallDevUpdate(execString);
        }

        public Dictionary<string, string> Select(string deviceName)
        {
            var qrystring = "SELECT * FROM dev_state WHERE Device = (SELECT device FROM dev_catalog WHERE description = @deviceName)";
            return _connection.Query<Tag>(qrystring, new { deviceName }).ToList().ToDictionary(k => k.dev_tag, v => v.dev_value);
        }

        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            _connection.Dispose();
        }
    }
}
