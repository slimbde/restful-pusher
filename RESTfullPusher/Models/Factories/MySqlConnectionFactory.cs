using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data.MySqlClient;

namespace RESTfullPusher
{
    public class MySqlConnectionFactory
    {
        public IDbConnection Get(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
    }
}