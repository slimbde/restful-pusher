using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfullPusher
{
    internal class MySQLDestination : Destination
    {
        private IMySqlTagRepository repo;

        public MySQLDestination(IMySqlTagRepository repo)
        {
            this.repo = repo;
        }

        public override void Send(string device, IDictionary<string, string> tags)
        {
            repo.CallDevUpdate(device, tags);
        }
    }
}