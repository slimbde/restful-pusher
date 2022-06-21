using AppModels;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace Handlers.Config
{
    public class DestinationSection : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<Destination> dests = new List<Destination>();

            foreach (XmlNode childNode in section.ChildNodes)
            {
                var attrs = childNode.Attributes;
                if (attrs != null)
                {
                    dests.Add(new Destination()
                    {
                        TargetName = attrs["targetName"].Value,
                        TargetUrl = attrs["targetUrl"].Value
                    });
                }
            }

            return dests;
        }
    }
}
