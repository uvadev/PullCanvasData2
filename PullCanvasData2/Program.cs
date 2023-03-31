using System;
using System.Threading.Tasks;
using AppUtils;
using Tomlyn.Syntax;

namespace PullCanvasData2; 

internal static class Program {
    public static async Task Main(string[] args) {
        var home = new AppHome("PullCanvasData2");
            
        Console.WriteLine($"Using config path: {home.ConfigPath}");
            
        if (!home.ConfigPresent()) {
            Console.WriteLine("Need to generate a config file.");

            home.CreateConfig(new DocumentSyntax {
                Tables = {
                    new TableSyntax("auth") {
                        Items = {
                            {"id", "PUT_ID_HERE"},
                            {"secret", "PUT_SECRET_HERE"},
                            {"base_url", "https://api-gateway.instructure.com"}
                        }
                    }
                }
            });

            Console.WriteLine("Created a new config file. Please go put in your client id and secret.");
            return;
        }

        Console.WriteLine("Found config file.");

        var config = home.GetConfig();
        var auth = config!.GetTable("auth");

        var api = new CanvasData(
            auth.Get<string>("base_url"),
            auth.Get<string>("id"),
            auth.Get<string>("secret")
        );

        await api.Authenticate();
    }
}