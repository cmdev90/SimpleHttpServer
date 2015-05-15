# SimpleHttpServer
A simple HTTP REST server implementation for C#. For use as an embedded HTTP server for small projects.

Basic how this works snippet: :+1:

```csharp
WebServer app = new WebServer(3001);

app.get("/test/hello", (HttpListenerRequest request) =>
{
    return "<XML><MESSAGE>It works!</MESSAGE></XML>";
});

app.post("/test/hello", (HttpListenerRequest request) =>
{
    string param = request.QueryString["query"];
    return string.Format("<XML><MESSAGE>Your query is below.</MESSAGE><QUERY>{0}</QUERY></XML>", param);
});

app.Start();
```

Helpful gist https://gist.github.com/aksakalli/9191056
