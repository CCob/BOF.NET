#if NET451_OR_GREATER

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BOFNET.Bofs.WebServer {
    public class WebServer : BeaconObject {

        HttpListener listener;
        string url;
        string pageTemplate;

        public WebServer(BeaconApi api) : base(api) {
            var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("BOFNET.Bofs.WebServer.Files.htm"));
            pageTemplate = stream.ReadToEnd();
        }

        async Task SendNotFound(HttpListenerResponse response) {
            // Write the response info
            byte[] data = Encoding.UTF8.GetBytes("The requested file cannot be found");
            response.StatusCode = 404;
            response.ContentType = "text/plain";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }

        async Task SendVirtualFile(HttpListenerResponse response, string fileName) {

            var vf = VFS.VirtualFiles[fileName];
            byte[] data = vf.Data;
            response.StatusCode = 200;

            if (string.IsNullOrEmpty(vf.ContentType))
                response.ContentType = "application/octet-stream";
            else
                response.ContentType = vf.ContentType;

            response.ContentLength64 = data.LongLength;
            response.AddHeader("Content-Disposition", $@"attachment; filename=""{fileName}""");
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();

        }

        async Task SendList(HttpListenerResponse response, string requestPath) {

            string fileName = Path.GetFileName(requestPath);
            if (!string.IsNullOrEmpty(fileName) && requestPath.Contains("/delete/")) {
                VFS.VirtualFiles.Remove(fileName);
            }

            StringBuilder sb = new StringBuilder();

            foreach (var virtualFile in VFS.VirtualFiles.Keys) {
                sb.Append("<tr>");
                sb.Append($@"<td data-label=""{virtualFile}""><a href=""{virtualFile}"">{virtualFile}</a></td>");
                sb.Append($@"<td data-label=""Size"">{VFS.VirtualFiles[virtualFile].Data.Length / 1024.0 / 1024.0:0.00}MB</td>");
                sb.Append($@"<td data-label=""{virtualFile}""><a href=""/delete/{virtualFile}"">Delete</a></td>");
                sb.Append("</tr>");
            }

            // Write the response info
            byte[] data = Encoding.UTF8.GetBytes(pageTemplate.Replace("{template}", sb.ToString()));
            response.StatusCode = 200;
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }

        async Task HandleIncomingConnections() {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer) {

                try {

                    // Will wait here until we hear from a connection
                    HttpListenerContext ctx = await listener.GetContextAsync();
                    HttpListenerRequest req = ctx.Request;

                    BeaconConsole.WriteLine($"[=] Incoming request {req.HttpMethod} {req.Url.AbsolutePath} from {req.RemoteEndPoint.Address}");

                    string fileName = Path.GetFileName(req.Url.AbsolutePath);

                    // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                    if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown")) {
                        runServer = false;
                    } else if (string.IsNullOrEmpty(req.Url.AbsolutePath) || req.Url.AbsolutePath == "/" || req.Url.AbsolutePath.Contains("/delete/")) {
                        await SendList(ctx.Response, req.Url.AbsolutePath);
                    } else if (VFS.VirtualFiles.ContainsKey(fileName)) {
                        await SendVirtualFile(ctx.Response, fileName);
                    } else {
                        await SendNotFound(ctx.Response);
                    }
                } catch (Exception e) {
                    BeaconConsole.WriteLine($"[!] Error {e.Message} raised during handling of client connection");
                }
            }
        }

        public override void Go(string[] args) {

            if (args.Length == 0) {
                BeaconConsole.WriteLine("[!] No listen URL supplied, e.g. http://localhost:12345/");
                return;
            }

            url = args[0];

            if(!url.EndsWith("/"))
                url += "/";

            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            BeaconConsole.WriteLine("[+] Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
            BeaconConsole.WriteLine("[+] HTTP Server Shutdown");
        }
    }
}

#endif
