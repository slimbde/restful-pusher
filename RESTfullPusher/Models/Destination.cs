using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfullPusher
{
    public interface IDestination
    {
        string Name { get; }
        void Send(string device, IDictionary<string, string> tags);

    }

    public abstract class Destination : IDestination
    {
        public string Name { get; set; }
        public abstract void Send(string device, IDictionary<string, string> tags);
    }
}