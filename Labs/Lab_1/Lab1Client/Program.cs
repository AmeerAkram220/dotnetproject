using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Client started. Attempting to contact the service...");
        using HttpClient client = new HttpClient();

        try
        {
            string serviceUrl = "http://localhost:5124/message"; 

            string response = await client.GetStringAsync(serviceUrl);

            Console.WriteLine("Success! The service responded with:");
            Console.WriteLine($"---> {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to communicate: {ex.Message}");
        }
    }
}