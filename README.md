# SimpleHttpServer
A simple HTTP REST server implementation for C#. For use as an embedded HTTP server for small projects.

Basic how this works snippet: :+1:

```csharp
WebServer app = new WebServer(3001);

app.get("/test/hello", (HttpListenerRequest request)=> {
	return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", param);
});

app.post("/test/hello", (HttpListenerRequest request) => {
	return string.Format("<HTML><BODY>My web page</BODY></HTML>", param);
});

app.Start();
```

Helpful gist https://gist.github.com/aksakalli/9191056
