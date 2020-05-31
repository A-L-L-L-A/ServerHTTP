using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Server
{
    class HTTPServer
    {

        private readonly string[] _indexFiles =
        {
            "index.html",
            "index.php",
            "default.html"
        };

        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {

        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".css", "text/css"},
        {".dll", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".gif", "image/gif"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".pdf", "application/pdf"},
        {".png", "image/png"},
        {".rar", "application/x-rar-compressed"},
        {".txt", "text/plain"},
        {".xml", "text/xml"},
        {".zip", "application/zip"},
        };

        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private int _port;
        

        public HTTPServer(string path, int port = 8080)
        {
            _rootDirectory = path;
            _port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();

            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                Process(context);
            }
        }


        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine(filename);
             Console.WriteLine("Method: {0}\nRequested URL: {1}\nUser-Agent: {2}\nUser-host: {3}", context.Request.HttpMethod, context.Request.Url.OriginalString, context.Request.UserAgent, context.Request.UserHostAddress);
            Console.Write("Accept:");
            foreach (string str in context.Request.AcceptTypes)
            Console.WriteLine(str+"\n");


            filename = filename.Substring(1);
            Console.WriteLine(filename);

            if (string.IsNullOrEmpty(filename))
            {
                foreach (string indexFile in _indexFiles)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }

            filename = Path.Combine(_rootDirectory, filename);

            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

 
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString());
                    context.Response.AddHeader("Last-modified", File.GetLastWriteTime(filename).ToString());


                    byte[] buffer = new byte[10240];
                    int bytes;
                    while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, bytes);
                    input.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            Console.WriteLine("Server response:\nStatus code: {0}", context.Response.StatusCode.ToString());
            if (context.Response.StatusCode == 200)
                Console.WriteLine("Date: {0}\nLast-modified: {1}\nContent-type: {2}\nContent-length: {3}\n", context.Response.Headers.Get("Date"), context.Response.Headers.Get("Last-modified"), context.Response.ContentType, context.Response.ContentLength64.ToString());


            context.Response.OutputStream.Close();
        }


    }
}
