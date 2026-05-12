using System.ServiceModel;
using Lab1Client;

Console.WriteLine("Client started. Attempting to contact the WCF service...");

var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
var endpoint = new EndpointAddress("http://localhost:5000/SimpleService.svc");
var factory = new ChannelFactory<ISimpleService>(binding, endpoint);
var channel = factory.CreateChannel();

try
{
    var message = channel.GetMessage();
    Console.WriteLine("Success! The service responded with:");
    Console.WriteLine($"---> {message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to communicate: {ex.Message}");
}
finally
{
    (channel as ICommunicationObject)?.Close();
    factory.Close();
}
