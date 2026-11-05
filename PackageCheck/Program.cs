// Fails if a consumer of the packed library could hit "Could not load file or assembly".

using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using PullCanvasData2;

namespace PackageCheck;

internal static class Program {
    public static async Task<int> Main() {
        var library = typeof(CanvasData).Assembly;
        Console.WriteLine($"Checking {library.GetName().Name} {library.GetName().Version}");
        Console.WriteLine($"Types: {library.GetTypes().Length}");

        foreach (var reference in library.GetReferencedAssemblies()) {
            var loaded = Assembly.Load(reference);
            Console.WriteLine($"  {reference.Name} {reference.Version} loaded as {loaded.GetName().Version}");
        }

        try {
            await new CanvasData("http://127.0.0.1:9", "id", "secret").Authenticate();
            Console.WriteLine("FAILED: authenticating against a closed port should not succeed.");
            return 1;
        } catch (HttpRequestException) {
            Console.WriteLine("Made an HTTP request, which failed as expected.");
        }

        Console.WriteLine("OK");
        return 0;
    }
}
