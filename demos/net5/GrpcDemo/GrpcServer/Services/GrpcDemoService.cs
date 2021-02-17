using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDemo;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrpcServer
{
    public class GrpcDemoService : GrpcDemo.Demo.DemoBase
    {
        private readonly ILogger<GrpcDemoService> _logger;

        public GrpcDemoService(ILogger<GrpcDemoService> logger)
        {
            _logger = logger;
        }

        public override async Task<QueryResponse> Query(QueryRequest request, ServerCallContext context)
        {
            var response = new QueryResponse();

            response.Persons.AddRange(await CreatePersons());

            return response;
        }

        public override async Task<Empty> SendData(IAsyncStreamReader<StreamMessage> requestStream, ServerCallContext context)
        {
            await ReceiveMessages(requestStream);
            return new Empty();
        }

        public override Task Receive(Empty request, IServerStreamWriter<StreamMessage> responseStream, ServerCallContext context)
        {
            return GenerateMessages(responseStream);
        }

        public override Task Bidirectional(IAsyncStreamReader<StreamMessage> requestStream, IServerStreamWriter<StreamMessage> responseStream, ServerCallContext context)
        {
            return Task.WhenAll(new[] { 
                ReceiveMessages(requestStream),
                GenerateMessages(responseStream)
            });
        }

        public override async Task<OthersTypesMessage> OtherTypesExample(Empty request, ServerCallContext context)
        {
            return new OthersTypesMessage
            {
                Error = new Error { Code = 1000, Text = "No Soup for you" },
                // Person = new Person { FullName = "Yev Kassem" },
                Interval = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(TimeSpan.FromMinutes(60)),
                Anything = Google.Protobuf.WellKnownTypes.Any.Pack(new QueryRequest { NamePart = "anything" }),
                Image = ByteString.CopyFrom(await LoadImage()),
                ErrorCount = null,
                ErrorStack = null
            };
        }

        private Task<IEnumerable<Person>> CreatePersons()
        {
            var person1 = new Person
            {
                Id = 1,
                FullName = "John Doe",
                Balance = 5000,
                Enabled = true,
                LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            person1.Phones.Add(new PhoneNumber { Type = PhoneType.Home, Number = "9999999999" });
            person1.Phones.Add(new PhoneNumber { Type = PhoneType.Home, Number = "8888888888" });
            person1.Attributes.Add("Favorite Food", "Pasta");
            person1.Attributes.Add("Favorite Drink", "Beer");

            var person2 = new Person
            {
                Id = 2,
                FullName = "Joe Joe",
                Balance = -100,
                Enabled = false,  // LastUpdated = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            return Task.FromResult<IEnumerable<Person>>(new[] { person1, person2 });
        }

        private async Task GenerateMessages(IServerStreamWriter<StreamMessage> responseStream)
        {
            for (var i = 1; i <= 10; i++)
            {
                await responseStream.WriteAsync(new StreamMessage { 
                    MessageNumber = i, Text = $"Message from server number {i}"
                });

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        private async Task ReceiveMessages(IAsyncStreamReader<StreamMessage> requestStream)
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"{message.MessageNumber} - {message.Text}");
            }
        }

        private Task<byte[]> LoadImage()
        {
            return System.IO.File.ReadAllBytesAsync("image.png");
        }
    }
}
