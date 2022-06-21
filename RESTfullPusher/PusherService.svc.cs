using AppModels;
using Handlers;
using Handlers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Hosting;
using static Handlers.Delegates;


namespace RESTfullPusher
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class PusherService : IPusherService
    {
        static LogHandler Logger;           // fix logger (append log source)
        IDataHandler mysqlDatHandler;
        IDataHandler screenCapParser;
        bool enableTrace;


        public PusherService()
        {
            Logger = GetLogger();

            try
            {
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                enableTrace = bool.Parse(appSettings["EnableTrace"]);

                mysqlDatHandler = HandlerFactory.CreateMySqlDataHandler();
                mysqlDatHandler.OnStatusLog += Logger;

                screenCapParser = HandlerFactory.CreateScreenCapParser();
                screenCapParser.OnStatusLog += Logger;
            }
            catch (Exception ex) { Logger?.Invoke($"[PusherService ConstructorException]: {ex.Message}", EventLogEntryType.Error); }
        }


        public string ReceiveToDB(string msg) => Task.Factory.StartNew(() => { return msg + " test"; }).Result;

        /// <summary>
        /// Sends posted bytes to the dispatcher BD
        /// </summary>
        public void PostToDispatcher(Stream fileStream)
        {
            try
            {
                ScreenCapMultipartInfo info = screenCapParser.Handle(fileStream).Result;

                if (info.Success)
                {
                    IDictionary<string, bool> postResult = mysqlDatHandler.Handle(info).Result as IDictionary<string, bool>;
                    string msg = "Post result:\n";
                    foreach (var item in postResult)
                        msg += $"{item.Key}: {item.Value}\n";

                    if (enableTrace) Logger?.Invoke($"[PusherService PostToDispatcher]: {msg}", EventLogEntryType.Information);
                }
            }
            catch (Exception ex) { Logger?.Invoke($"[PusherService PostToDispatcherException]: {ex.Message}", EventLogEntryType.Error); }
        }


        /// <summary>
        /// Initializes app logger
        /// </summary>
        /// <returns></returns>
        static LogHandler GetLogger()
        {
            //if (Environment.UserInteractive)
            //return delegate (string message, EventLogEntryType msgType) { Console.WriteLine($"{DateTime.Now.ToString("G")} {message}"); };

            EventLog eventLog1 = new EventLog();

            if (!EventLog.SourceExists("RESTfullPusherSvcSrc"))
                EventLog.CreateEventSource("RESTfullPusherSvcSrc", "RESTfullPusherSvc");

            eventLog1.Source = "RESTfullPusherSvcSrc";
            eventLog1.Log = "RESTfullPusherSvc";

            return eventLog1.WriteEntry;
        }


        ///////////////////////////// Not used here. Just to not loose this stuff
        public static bool SaveImageFile(ScreenCapMultipartInfo info)
        {
            try
            {
                Image image;

                using (MemoryStream ms = new MemoryStream(info.FileContents))
                {
                    image = Image.FromStream(ms);
                }
                Bitmap bmp = new Bitmap(image);

                ImageCodecInfo qualityEncoder = GetEncoder(ImageFormat.Png);

                EncoderParameters quality = new EncoderParameters(1);
                EncoderParameter qualityValue = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L); // target quality 0...100L
                quality.Param[0] = qualityValue;

                string directory = HostingEnvironment.MapPath("~/uploads");
                string filePath = Path.Combine(directory, info.Filename);
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(directory);

                bmp.Save(filePath, qualityEncoder, quality);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

    }
}
