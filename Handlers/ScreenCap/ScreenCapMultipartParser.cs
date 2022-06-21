using AppModels;
using Handlers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Handlers.Delegates;

namespace Handlers.ScreenCap
{
    public class ScreenCapMultipartParser : IDataHandler
    {
        string delimiter;
        bool enableTrace;
        string lastErrorMsg;    // to not choke EventLog with redundant repeating errors
        Encoding defaultEncoding = Encoding.UTF8;

        public event LogHandler OnStatusLog;



        public Task<dynamic> Handle(dynamic data)
        {
            ScreenCapMultipartInfo result = new ScreenCapMultipartInfo()
            {
                Success = false,
                ContentType = "",
                Filename = "",
                FileContents = null,
            };


            try
            {
                Initialize();

                byte[] requestData = ToByteArray(data as Stream);

                // Copy to a string for header parsing  
                string content = defaultEncoding.GetString(requestData);

                // The first line should contain the delimiter  
                int delimiterEndIndex = content.IndexOf("--");
                if (delimiterEndIndex > -1)
                {
                    // Look for Content-Type  
                    Regex re = new Regex(@"(?<=Content\-Type: )(.*?)(?=\r\n\r\n)");
                    Match contentTypeMatch = re.Match(content);

                    // Look for filename  
                    re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                    Match filenameMatch = re.Match(content);

                    // Did we find the required values?  
                    if (contentTypeMatch.Success && filenameMatch.Success)
                    {
                        // Set properties  
                        result.ContentType = contentTypeMatch.Value.Trim();
                        result.Filename = filenameMatch.Value.Trim();

                        // Get the start & end indexes of the file contents  
                        int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;
                        byte[] delimiterBytes = defaultEncoding.GetBytes("\r\n" + delimiter);
                        int endIndex = IndexOf(requestData, delimiterBytes, startIndex);
                        int contentLength = endIndex - startIndex;

                        // Extract the file contents from the byte array  
                        byte[] fileData = new byte[contentLength];
                        Buffer.BlockCopy(requestData, startIndex, fileData, 0, contentLength);

                        result.FileContents = fileData;
                        result.Success = true;
                    }
                }

            }
            catch (Exception ex)
            {
                string message = $"{ex.Message}{(ex.InnerException != null ? $"\nInnerException: {ex.InnerException.Message}" : "")}";
                if (message != lastErrorMsg)
                {
                    lastErrorMsg = message;
                    OnStatusLog?.Invoke($"[ScreenCapMultipartParser HandleException]: {message}", EventLogEntryType.Error);
                }
            }

            return Task.Factory.StartNew(() => { return result as dynamic; });
        }



        void Initialize()
        {
            try
            {
                NameValueCollection appSettings = ConfigurationManager.AppSettings;

                enableTrace = bool.Parse(appSettings["EnableTrace"]);
                delimiter = appSettings["ScreenCapDelimiter"].ToString();
            }
            catch (Exception ex) { OnStatusLog?.Invoke($"[ScreenCapMultipartParser InitializeException]: {ex.Message}", EventLogEntryType.Error); }
        }
        int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        ++index;
                        if (index == serachFor.Length) return startPos;
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1) return -1;

                        index = 0;
                    }
                }
            }

            return -1;
        }
        byte[] ToByteArray(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0) return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }



}