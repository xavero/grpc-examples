using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new Greet.Greeter.GreeterClient(channel);

            var response = await client.SayHelloAsync(new Greet.HelloRequest { Name = "Grpc User" });

            Console.WriteLine($"Message: {response.Message}");

            Console.WriteLine("Finish...");
        }
    }
}
