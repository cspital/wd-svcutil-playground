using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace UWDiff
{
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new RootCommand("UWDiff detects (and eventually corrects) differences between xscgen and dotnet-svcutil classes.")
            {
                new Option<string>("-d", description: "The project directory."),
            };

            handler.Handler = CommandHandler.Create<string>(d =>
            {
                var extractor = new ClientExtractor();

                var client = extractor.Extract(MustPath(d, "Service/Reference.cs"));

                Console.WriteLine(client);
            });

            handler.Invoke(args);
        }

        static string MustPath(string dir, string relative)
        {
            var comb = Path.Combine(dir, relative);
            if (!File.Exists(comb))
            {
                throw new FileNotFoundException("Could not find file", comb);
            }
            return comb;
        }
    }
}
