using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace RESTfullPusher
{
    [ServiceContract]
    public interface IPusherService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "/PostToDispatcher",
            BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json)]
        void PostToDispatcher(Stream fileStream);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "/Receive?str={value}",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        string ReceiveToDB(string value);
    }
}
