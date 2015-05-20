# SimpleHttpServer
A simple HTTP REST server implementation for C#. For use as an embedded HTTP server for small projects.

Basic how this works snippet: :+1:

```csharp
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
```

Helpful gist https://gist.github.com/aksakalli/9191056
