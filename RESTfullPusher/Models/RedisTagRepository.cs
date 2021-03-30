using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace RESTfullPusher
{
    internal interface IRedisTagRepository
    {
        void CallDevUpdate(string deviceName, IDictionary<string, string> tags);
    }
    internal class RedisTagRepository : IRedisTagRepository
    {
        private IDatabase _connection;

        public RedisTagRepository(IDatabase connection)
        {
            _connection = connection;
        }

        public void CallDevUpdate(string deviceName, IDictionary<string, string> tags)
        {
            var JSONString = JsonConvert.SerializeObject(tags, Formatting.Indented);
            _connection.HashSet("Agregates", deviceName, JSONString);
        }
    }
}