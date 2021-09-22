using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcDemo;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace GrpcClientHosted
{
    public class GrpcClientWorker : BackgroundService
    {
        private readonly Demo.DemoClient _client;
        private readonly ILogger<GrpcClientWorker> _logger;
        private readonly Random _random = new Random();

        public GrpcClientWorker(GrpcDemo.Demo.DemoClient client, ILogger<GrpcClientWorker> logger)
        {
            _client = client;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await FieldMaskExample(stoppingToken);

            await UnaryExample(stoppingToken);

            await Task.Delay(5000, stoppingToken);

            await ClientStreamingExample(stoppingToken);

            await Task.Delay(5000, stoppingToken);

            await ServerStreamingExample(stoppingToken);

            await Task.Delay(5000, stoppingToken);

            await BidirectionalExample(stoppingToken);

            await Task.Delay(5000, stoppingToken);

            await OthersFieldsExample(stoppingToken);
        }

        private static string NameOfField(int fieldNumber) => Person.Descriptor.FindFieldByNumber(fieldNumber).Name;

        private async Task FieldMaskExample(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("Starting FieldMaskExample"))
            {
                var fields = new string[] {
                    NameOfField(Person.FullNameFieldNumber),
                    NameOfField(Person.BalanceFieldNumber),
                    $"{NameOfField(Person.ChildFieldNumber)}.{NameOfField(Person.IdFieldNumber)}",
                    $"{NameOfField(Person.ChildFieldNumber)}.{NameOfField(Person.FullNameFieldNumber)}"
                };

                var request = new GrpcDemo.FieldMaskExampleRequest {
                    // FieldMask = FieldMask.FromFieldNumbers<Person>(Person.FullNameFieldNumber, Person.BalanceFieldNumber)
                    FieldMask = FieldMask.FromString<Person>(string.Join(',', fields))
                };

                var person = await _client.FieldMaskExampleAsync(request, cancellationToken: stoppingToken);

                _logger.LogInformation($"Person from server: {person.Id} - {person.FullName} - {person.Balance} - {person.Child.Id} - {person.Child.FullName} - {person.LastUpdated?.ToDateTime()}");
            }
        }

        /// <summary>
        /// Request/Response tradicional
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task UnaryExample(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("Starting unary call"))
            {
                var request = new GrpcDemo.QueryRequest { NamePart = "Joe" };

                var response = await _client.QueryAsync(request, cancellationToken: stoppingToken);

                foreach (var person in response.Persons)
                {
                    _logger.LogInformation($"Person from server: {person.Id} - {person.FullName} - {person.LastUpdated?.ToDateTime()}");
                    _logger.LogInformation("Attributes: {0}", string.Join(",", person.Attributes.Select(entry => $"{entry.Key} = {entry.Value}")));
                }
            }
        }

        /// <summary>
        /// Envia um stream de dados para o servidor
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task ClientStreamingExample(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("Client streaming:"))
            {
                _logger.LogInformation("Sending data");

                var clientStreaming = _client.SendData(cancellationToken: stoppingToken);

                await GenerateClientMessages(clientStreaming.RequestStream);
            }
        }

        /// <summary>
        /// Recebe um Stream de dados do servidor
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task ServerStreamingExample(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("Server streaming:"))
            {
                var request = new Google.Protobuf.WellKnownTypes.Empty();

                var serverStreaming = _client.Receive(request, cancellationToken: stoppingToken);

                await ReadServerMessages(serverStreaming.ResponseStream);
            }
        }

        /// <summary>
        /// Envia e Recebe dados ao mesmo tempo.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task BidirectionalExample(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("Bi-directional streaming:"))
            {
                using var duplexStreaming = _client.Bidirectional(cancellationToken: stoppingToken);

                await Task.WhenAll(new[] {
                    GenerateClientMessages(duplexStreaming.RequestStream),
                    ReadServerMessages(duplexStreaming.ResponseStream)
                });
            }
        }

        /// <summary>
        /// Exemplo com diversos tipos de dados não primitivos
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task OthersFieldsExample(CancellationToken stoppingToken)
        {
            using (_logger.BeginScope("OthersTypes Example"))
            {
                var request = new Google.Protobuf.WellKnownTypes.Empty();

                var response = await _client.OtherTypesExampleAsync(request, cancellationToken: stoppingToken);

                // lendo Duration (pode ser nulo)
                var interval = response.Interval?.ToTimeSpan() ?? TimeSpan.FromSeconds(1);

                // lendo campo OneOf
                switch (response.ResultCase)
                {
                    case OthersTypesMessage.ResultOneofCase.None:
                        _logger.LogInformation("OneOf Result is none");
                        break;

                    case OthersTypesMessage.ResultOneofCase.Person:
                        _logger.LogInformation("OneOf Result is Person ({0})", response.Person.FullName);
                        break;

                    case OthersTypesMessage.ResultOneofCase.Error:
                        _logger.LogInformation("OneOf Result is Error ({0})", response.Error.Text);
                        break;
                    default:
                        throw new ArgumentException("Unknown instrument type");
                }

                // lendo campos Nullables
                int? errorCount = response.ErrorCount?.Value;
                string errorStack = response.ErrorStack?.Value ?? "No Error";

                // lendo campo bytes
                using (var stream = File.OpenWrite(Path.GetTempFileName()))
                {
                    await stream.WriteAsync(response.Image.Memory, stoppingToken);
                }
                // ou byte[] image = response.Image.ToByteArray();


                // lendo campo Any
                if (response.Anything.Is(Person.Descriptor))
                {
                    var person = response.Anything.Unpack<Person>();
                }
                else if (response.Anything.Is(QueryRequest.Descriptor))
                {
                    var queryRequest = response.Anything.Unpack<QueryRequest>();
                    _logger.LogInformation("Any Field is QueryRequest ({0})", queryRequest.NamePart);
                }

            }
        }

        private async Task ReadServerMessages(IAsyncStreamReader<StreamMessage> responseStream)
        {
            await foreach (var message in responseStream.ReadAllAsync())
            {
                _logger.LogInformation($"{message.MessageNumber} - {message.Text}");
            }
        }

        private async Task GenerateClientMessages(IClientStreamWriter<StreamMessage> requestStream)
        {
            for (var i = 1; i <= 10; i++)
            {
                await requestStream.WriteAsync(new StreamMessage
                {
                    MessageNumber = i,
                    Text = $"Message from client number {i}"
                });

                await Task.Delay(RandomInterval());
            }

            await requestStream.CompleteAsync();
        }

        private TimeSpan RandomInterval() => TimeSpan.FromMilliseconds(_random.Next(100, 2000));
    }
}
