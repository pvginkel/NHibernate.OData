using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using NHttp;

namespace NHibernate.OData.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var stream = typeof(Program).Assembly.GetManifestResourceStream(typeof(Program).Namespace + ".LogConfiguration.xml"))
            {
                log4net.Config.XmlConfigurator.Configure(stream);
            }

            using (var database = new Database())
            using (var server = new HttpServer())
            {
                Console.Write("Starting HTTP server...");

                server.EndPoint = new IPEndPoint(IPAddress.Loopback, 0);

#if DEBUG
                server.EndPoint = new IPEndPoint(IPAddress.Loopback, 12875);
#endif

                server.Start();

                var ns = (XNamespace)("http://" + server.EndPoint + "/OData/");

                server.RequestReceived += (s, e) => RequestReceived(database, e, ns);

                Console.WriteLine(" done");

#if !DEBUG
                Process.Start("http://" + server.EndPoint);
#endif

                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
            }
        }

        private static void RequestReceived(Database database, HttpRequestEventArgs e, XNamespace ns)
        {
            if (e.Request.Path.StartsWith("/odata", StringComparison.OrdinalIgnoreCase))
                ProcessODataRequest(database, e, ns);
            else
                ProcessStaticRequest(e);
        }

        private static void ProcessODataRequest(Database database, HttpRequestEventArgs e, XNamespace ns)
        {
            string[] parts = e.Request.RawUrl.Split(new[] { '?' }, 2);

            string path = parts[0];
            string filter = parts.Length == 2 ? parts[1] : null;

            path = path.Substring(6).TrimStart('/');

            ODataRequest request;

            using (var session = database.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.FlushMode = FlushMode.Never;

                request = new ODataRequest(database, session, path, filter, ns);

                request.GetResponse().Save(e.Response.OutputStream);

                session.Flush();
                transaction.Commit();
            }

            e.Response.ContentType = "application/xml;charset=utf-8";
            e.Response.Headers["DataServiceVersion"] = request.DataServiceVersion;
        }

        private static void ProcessStaticRequest(HttpRequestEventArgs e)
        {
            string page = e.Request.Path.TrimStart('/');

            if (ProcessPage(page, e))
                return;

            if (ProcessPage(page.TrimEnd('/') + "/index.html", e))
                return;

            e.Response.Status = "404 Not Found";
        }

        private static bool ProcessPage(string page, HttpRequestEventArgs e)
        {
            string resource = typeof(Program).Namespace + ".Site." + page.TrimStart('/').Replace('/', '.');

            using (var stream = typeof(Program).Assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    stream.CopyTo(e.Response.OutputStream);
                    return true;
                }
            }

            return false;
        }
    }
}
