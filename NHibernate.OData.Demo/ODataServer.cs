using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NHttp;

namespace NHibernate.OData.Demo
{
    internal class ODataServer : HttpServer
    {
        private readonly ISessionFactory _sessionFactroy;
        private ODataService _service;

        public ODataServer(ISessionFactory sessionFactroy)
        {
            if (sessionFactroy == null)
                throw new ArgumentNullException("sessionFactroy");

            _sessionFactroy = sessionFactroy;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (State == HttpServerState.Started)
            {
                _service = new ODataService(
                    _sessionFactroy,
                    String.Format("http://{0}/OData/", EndPoint),
                    "SouthWind",
                    "NHibernate.OData.Demo"
                );
            }
        }

        protected override void OnRequestReceived(HttpRequestEventArgs e)
        {
            if (e.Request.Path.StartsWith("/odata", StringComparison.OrdinalIgnoreCase))
                ProcessODataRequest(e.Context);
            else
                ProcessStaticRequest(e.Context);
        }

        private void ProcessODataRequest(HttpContext context)
        {
            string[] parts = context.Request.RawUrl.Split(new[] { '?' }, 2);

            string path = parts[0];
            string filter = parts.Length == 2 ? parts[1] : null;

            // Remove the /odata part.

            path = path.Substring(6).TrimStart('/');

            ODataRequest request;

            using (var session = _sessionFactroy.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.FlushMode = FlushMode.Never;

                request = _service.Query(session, path, filter);

                session.Flush();
                transaction.Commit();
            }

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(request.Response);
            }

            context.Response.ContentType = request.ContentType;
            context.Response.Headers["DataServiceVersion"] = request.DataServiceVersion;
        }

        private void ProcessStaticRequest(HttpContext context)
        {
            string page = context.Request.Path.TrimStart('/');

            if (ProcessPage(page, context))
                return;

            if (ProcessPage(page.TrimEnd('/') + "/index.html", context))
                return;

            context.Response.Status = "404 Not Found";
        }

        private bool ProcessPage(string page, HttpContext context)
        {
            string resource = typeof(Program).Namespace + ".Site." + page.TrimStart('/').Replace('/', '.');

            using (var stream = typeof(Program).Assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    stream.CopyTo(context.Response.OutputStream);
                    return true;
                }
            }

            return false;
        }
    }
}
