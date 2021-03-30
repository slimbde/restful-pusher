using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using NLog;

namespace RESTfullPusher
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class PusherService : IPusherService
    {
        ILogger logger;
        IDispathcer dispatcher;
        public PusherService(IDispathcer dispatcher)
        {
            logger = LogManager.GetCurrentClassLogger();
            this.dispatcher = dispatcher;
        }

        public string ReceiveToDBInBody(string value)
        {
            return ReceiveToDB(value);
        }

        public string ReceiveToDB(string value)
        {
            var deviceName = value.Substring(0, value.LastIndexOf('~'));
            var tags = ParseTags(value);

            dispatcher.Broadcast(deviceName, tags);
            return value;
        }

        private IDictionary<string, string> ParseTags(string message)
        { 
            var tagPart = message.Substring(message.LastIndexOf('~') + 1);

            return tagPart.Split('|').
                Where(s => !String.IsNullOrWhiteSpace(s)).
                Select(tag => tag.Split('=')).
                ToDictionary(k => k[0], v => v[1]);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
