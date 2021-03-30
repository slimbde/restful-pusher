using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace RESTfullPusher
{
    public interface IDispathcer
    {
        IList<IDestination> Outputs { get; }
        void Broadcast(string device, IDictionary<string, string> tags);
    }

    public class Dispatcher : IDispathcer
    {
        public IList<IDestination> Outputs { get; private set; }

        public Dispatcher(IList<IDestination> outputs)
        {
            Outputs = outputs;
        }

        public void Broadcast(string device, IDictionary<string, string> tags)
        {
            foreach (var output in Outputs)
            {
                output.Send(device, tags);
            }
        }
    }
}