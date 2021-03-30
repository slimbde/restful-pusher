using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using StackExchange.Redis;

namespace RESTfullPusher
{
    public class RedisConnectionFactory
    {
        public IDatabase Get(string connectionString)
        {
            return ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        }
    }
}