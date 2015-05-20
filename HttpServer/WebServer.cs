using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace HttpServer
{
    public class WebServerMethod
    {
        public static int GET = 0;
        public static int POST = 1;
    }

    public class WebServerResponse
    {

        private HttpListenerResponse Response;

        public WebServerResponse (HttpListenerContext context)
        {
            this.Response = context.Response;
        }

        public void send(string message)
        {
            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(message);
                this.Response.ContentLength64 = buf.Length;
                this.Response.OutputStream.Write(buf, 0, buf.Length);
                Console.WriteLine(string.Format("[OUT] Sending {0}b", this.Response.ContentLength64));
            }
            catch { } // suppress any exceptions
            finally
            {
                this.Response.OutputStream.Close(); // always close the stream
            }
        }

        public void setMIMEtype(string MIMEtype)
        {
            this.Response.ContentType = MIMEtype;
        }

        public HttpListenerResponse getMethods()
        {
            return this.Response;
        }
    }

    public class WebServerRequest
    {
        private HttpListenerRequest Request;

        public WebServerRequest (HttpListenerContext context)
        {
            this.Request = context.Request;
        }

        public string param(string key)
        {
            return this.Request.QueryString[key];
        }

        public HttpListenerRequest getMethods()
        {
            return this.Request;
        }
    }

    public class WebServer
    {
        private HttpListener httpSocket = new HttpListener();
        private Dictionary<string, Action<WebServerRequest, WebServerResponse>>[] ActionRoutes = new Dictionary<string, Action<WebServerRequest, WebServerResponse>>[2];
        private string prefix;

        public WebServer() : this(8080) { }

        public WebServer(int port)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example "http://localhost:8080/index/".
            this.prefix = string.Format("http://*:{0}/", port);
            httpSocket.Prefixes.Add(prefix);
        }


        public void get(string path, Action<WebServerRequest, WebServerResponse> method)
        {
            this.AddRoute(WebServerMethod.GET, path, method);
        }

        public void post(string url, Action<WebServerRequest, WebServerResponse> method)
        {
            this.AddRoute(WebServerMethod.POST, url, method);
        }

        public void Start()
        {
            httpSocket.Start();
            Console.WriteLine(string.Format("Server listenting on {0}", this.prefix));

            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (httpSocket.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((context) =>
                        {
                            var httpContext = context as HttpListenerContext;

                            Console.WriteLine(string.Format("[IN] {0} {1} ({2})", httpContext.Request.HttpMethod, httpContext.Request.Url, httpContext.Request.ContentLength64));

                            Action<WebServerRequest, WebServerResponse> activeMethod = null;

                            switch (httpContext.Request.HttpMethod)
                            {
                                case "GET":
                                    activeMethod = this.FindRouteFunction(WebServerMethod.GET, httpContext.Request.Url.AbsolutePath);
                                    break;
                                case "POST":
                                    activeMethod = this.FindRouteFunction(WebServerMethod.POST, httpContext.Request.Url.AbsolutePath);
                                    break;
                                default:
                                    activeMethod = MethodNotImplemented;
                                    throw new Exception();
                            }

                            // Call the active method
                                activeMethod(new WebServerRequest(httpContext), new WebServerResponse(httpContext));

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

        private void AddRoute(int httpMethod, string route, Action<WebServerRequest, WebServerResponse> method)
        {
            if (this.ActionRoutes[httpMethod] == null)
            {
                this.ActionRoutes[httpMethod] = new Dictionary<string, Action<WebServerRequest, WebServerResponse>>();
            }

            this.ActionRoutes[httpMethod].Add(route, method);
        }

        private Action<WebServerRequest, WebServerResponse> FindRouteFunction(int httpMethod, string uri)
        {
            Dictionary<string, Action<WebServerRequest, WebServerResponse>> _routes = this.ActionRoutes[httpMethod];

            if (_routes.ContainsKey(uri))
                return _routes[uri];

            return NotFoundResponse;
        }

        private static void NotFoundResponse(WebServerRequest request, WebServerResponse response)
        {
            response.send(string.Format("<HTML><BODY><h1>404 URL Not Found!</h1><hr/><i>{0}</i></BODY></HTML>", request.getMethods().Url.OriginalString));
        }

        private static void MethodNotImplemented(WebServerRequest request, WebServerResponse response)
        {
            response.send(string.Format("<HTML><BODY><h1>401 Not Implemented!</h1><hr/><i>{0}</i></BODY></HTML>", request.getMethods().Url.OriginalString));
        }
    }
}
