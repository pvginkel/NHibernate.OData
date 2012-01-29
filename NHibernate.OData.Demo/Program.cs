using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
            using (var server = new ODataServer(database.SessionFactory))
            {
                Console.Write("Starting HTTP server...");

                server.Start();

                Console.WriteLine(" done");

                Process.Start("http://" + server.EndPoint);

                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
            }
        }
    }
}
