using AppModels;
using Handlers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Handlers.Delegates;

namespace Handlers.MySql
{
    public class MySqlDataHandler : IDataHandler
    {
        IEnumerable<Destination> destinations;
        int requestTimeout;
        bool enableTrace;
        string lastErrorMsg;    // to not choke EventLog with redundant repeating errors

        public event LogHandler OnStatusLog;




        public Task<dynamic> Handle(dynamic data)
        {
            Initialize();

            IDictionary<string, bool> sentResult = new Dictionary<string, bool>();

            ScreenCapMultipartInfo info = data as ScreenCapMultipartInfo;

            foreach (Destination dest in destinations)
            {
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dest.TargetUrl);
                request.Timeout = requestTimeout;
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Method = "POST";
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;

                try
                {
                    byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");        // Разделитель параметров в байтах
                    byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");   // Закрывающий разделитель в байтах

                    WriteContentToRequest(request, info, boundaryBytes, endBoundaryBytes);
                    string received = ReceiveResponse(request);

                    sentResult.Add($"{dest.TargetName} [{dest.TargetUrl}]", true);
                }
                catch (Exception ex)
                {
                    string message = $"{ex.Message}{(ex.InnerException != null ? $"\nInnerException: {ex.InnerException.Message}" : "")}";
                    if (message != lastErrorMsg)
                    {
                        lastErrorMsg = message;
                        OnStatusLog?.Invoke($"[MySqlDataHandler HandleException]: {message}", EventLogEntryType.Error);
                    }

                    return Task.Factory.StartNew(() => { return sentResult as dynamic; });
                }
            }

            return Task.Factory.StartNew(() => { return sentResult as dynamic; }); ;
        }



        /// <summary>
        /// This moved to a separate action to allow reading config for every single request
        /// Convinient immediate config reading
        /// </summary>
        void Initialize()
        {
            try
            {
                NameValueCollection appSettings = ConfigurationManager.AppSettings;

                requestTimeout = int.Parse(appSettings["RequestTimeout"]) * 1000;
                enableTrace = bool.Parse(appSettings["EnableTrace"]);

                IEnumerable<Destination> destinationCfgs = ConfigurationManager.GetSection("Destinations") as IEnumerable<Destination>;
                destinations = destinationCfgs.Select(d => new Destination() { TargetUrl = d.TargetUrl, TargetName = d.TargetName });
            }
            catch (Exception ex) { OnStatusLog?.Invoke($"[MySqlDataHandler InitializeException]: {ex.Message}", EventLogEntryType.Error); }
        }



        /// <summary>
        /// Осуществляет запись в поток запроса
        /// </summary>
        /// <param name="request">запрос</param>
        /// <param name="boundary">разделитель</param>
        /// <param name="files">прикрепляемые файлы</param>
        /// <returns>Ожидание выполнения операции</returns>
        void WriteContentToRequest(HttpWebRequest request, ScreenCapMultipartInfo info, byte[] boundaryBytes, byte[] endBoundaryBytes)
        {
            var requestStreamTask = Task.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, request);

            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            var complete = requestStreamTask.Wait(requestTimeout, ct);
            if (!complete && (requestStreamTask.Status != TaskStatus.RanToCompletion))
            {
                cts.Cancel();
                throw new Exception("Сервер не отвечает");
            }

            using (var requestStream = requestStreamTask.Result)
            {
                int extensionIndex = info.Filename.LastIndexOf(".");
                string filename = info.Filename.Substring(0, extensionIndex);

                byte[] formItemBytes = GetDisposition(filename);
                byte[] contentBytes = info.FileContents;
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                requestStream.Write(contentBytes, 0, contentBytes.Length);

                requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                requestStream.Close();
            }
        }


        /// <summary>
        /// Получает ответ от сервера на осуществленный запрос
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string ReceiveResponse(HttpWebRequest request)
        {
            WebResponse responseObject = Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request).Result;

            using (var responseStream = responseObject.GetResponseStream())
            using (var sr = new StreamReader(responseStream))
                return sr.ReadToEnd();
        }


        /// <summary>
        /// Возвращает заголовок с описанием прикрепляемого параметра запроса в байтах.
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <returns></returns>
        byte[] GetDisposition(string name)
        {
            string disposition = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{0}.png\"\r\nContent-Type: application/octet-stream\r\n\r\n";
            return Encoding.UTF8.GetBytes(string.Format(disposition, name));
        }
    }
}