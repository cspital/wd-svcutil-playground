using System;
using UWD.Lib;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Threading;
using System.Diagnostics;

namespace WDTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var root = new RootCommand("WDTest attempts to use the UWD.Lib")
            {
                new Option<string>("-u", description: "Workday username") { IsRequired = true },
                new Option<string>("-p", description: "Workday password") { IsRequired = true },
                new Option<string>("--url", () => "https://wd5-impl-services1.workday.com/ccx/service/uw11/Resource_Management/v35.0/", description: "Workday root url (optional)")
                {
                    IsRequired = false
                }
            };

            root.Handler = CommandHandler.Create<string, string, string>(async (u, p, url) =>
            {
                var binding = new BasicHttpsBinding();
                var endpoint = new EndpointAddress(url);
                var client = new Resource_ManagementPortClient(binding, endpoint);
                client.Endpoint.EndpointBehaviors.Add(new AuthenticationBehavior(new WorkdayCredentials
                {
                    Username = u,
                    Password = p
                }));
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(30);

                Console.WriteLine("Starting test...");
                var sw = Stopwatch.StartNew();
                try
                {
                    var resp = await client.Get_Invoice_TypesAsync(new Workday_Common_HeaderType(), new Get_Invoice_Types_RequestType());
                    Console.Write($"Success! {sw.Elapsed}");
                }
                catch (FaultException fe)
                {
                    Console.WriteLine($"FAULT {fe.GetFriendlyType()} after {sw.Elapsed}");
                    Console.WriteLine(fe.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR {e.GetFriendlyType()} after {sw.Elapsed}");
                    Console.WriteLine(e.ToString());
                }
            });

            await root.InvokeAsync(args);
        }
    }
}
