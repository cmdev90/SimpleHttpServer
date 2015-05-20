using System;
using System.Net;
using HttpServer;

namespace SimpleWebServer
{

    class Program
    {
        static void Main(string[] args)
        {
            WebServer app = new WebServer(3001);

            app.get("/", (WebServerRequest req, WebServerResponse res) =>
            {
                res.setMIMEtype("application/xml");
                res.send("<XML><MESSAGE>It works!</MESSAGE></XML>");
            });

            app.post("/", (WebServerRequest req, WebServerResponse res) =>
            {
                string query = req.param("query");

                res.setMIMEtype("application/xml");
                res.send(string.Format("<XML><MESSAGE>Your query is below.</MESSAGE><QUERY>{0}</QUERY></XML>", query));
            });

            app.Start();
            //Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();
            app.Stop();
        }

    }
}

