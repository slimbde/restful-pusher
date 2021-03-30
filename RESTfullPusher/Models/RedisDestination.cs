using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfullPusher
{
    internal class RedisDestination: Destination
    {
        private IRedisTagRepository repo;

        public RedisDestination(IRedisTagRepository repo)
        {
            this.repo = repo;
        }

        public override void Send(string device, IDictionary<string, string> tags)
        {
            repo.CallDevUpdate(device, tags);
        }
    }
}