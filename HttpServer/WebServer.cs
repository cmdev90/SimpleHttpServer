using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace HttpServer
{
    public class HttpMethods
    {
        public static int GET = 0;
        public static int POST = 1;
    }

    public class WebServer
    {
        private HttpListener httpSocket = new HttpListener();
        private Dictionary<string, Func<HttpListenerRequest, string>>[] Routes = new Dictionary<string, Func<HttpListenerRequest, string>>[2];
        
        public string prefix {
            get { return prefix; } 
            private set { } 
        }

        public WebServer() : this(8080) { }

        public WebServer(int port)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example "http://localhost:8080/index/".
            this.prefix = string.Format("http://localhost:{0}/", port);
            httpSocket.Prefixes.Add(prefix);
        }

        public void get(string path, Func<HttpListenerRequest, string> method)
        {
            this.AddRoute(HttpMethods.GET, path, method);
        }

        public void post(string url, Func<HttpListenerRequest, string> method)
        {
            this.AddRoute(HttpMethods.POST, url, method);
        }

        public void Start()
        {

            httpSocket.Start();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (httpSocket.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((context) =>
                        {
                            var httpContext = context as HttpListenerContext;
                            Func<HttpListenerRequest, string> activeMethod = null;

                            try
                            {
                                switch (httpContext.Request.HttpMethod)
                                {
                                    case "GET":
                                        activeMethod = this.FindRouteFunction(HttpMethods.GET, httpContext.Request.Url.AbsolutePath);
                                        break;
                                    case "POST":
                                        activeMethod = this.FindRouteFunction(HttpMethods.POST, httpContext.Request.Url.AbsolutePath);
                                        break;
                                    default:
                                        activeMethod = MethodNotImplemented;
                                        throw new Exception();
                                }

                                string response = activeMethod(httpContext.Request);
                                httpContext.Response.ContentType = "application/xml";
                                byte[] buf = Encoding.UTF8.GetBytes(response);
                                httpContext.Response.ContentLength64 = buf.Length;
                                httpContext.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                httpContext.Response.OutputStream.Close();
                            }
                        }, httpSocket.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            httpSocket.Stop();
            httpSocket.Close();
        }

        private void AddRoute(int httpMethod, string route, Func<HttpListenerRequest, string> method)
        {
            if (this.Routes[httpMethod] == null)
            {
                this.Routes[httpMethod] = new Dictionary<string, Func<HttpListenerRequest, string>>();
            }

            this.Routes[httpMethod].Add(route, method);
        }

        private Func<HttpListenerRequest, string> FindRouteFunction(int httpMethod, string uri)
        {
            Dictionary<string, Func<HttpListenerRequest, string>> _routes = this.Routes[httpMethod];

            if (_routes.ContainsKey(uri))
                return _routes[uri];

            return NotFoundResponse;
        }

        private static string NotFoundResponse(HttpListenerRequest request)
        {
            return string.Format("<HTML><BODY><h1>404 URL Not Found!</h1><hr/><i>{0}</i></BODY></HTML>", request.Url.OriginalString);
        }

        private static string MethodNotImplemented(HttpListenerRequest request)
        {
            return string.Format("<HTML><BODY><h1>401 Not Implemented!</h1><hr/><i>{0}</i></BODY></HTML>", request.Url.OriginalString);
        }
    }
}
