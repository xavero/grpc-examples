using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Polly;
using System.Net.Http;
using System.Net;
using Grpc.Core;
using System.Linq;

namespace GrpcClientHosted
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) =>
               {
                   var configuration = hostContext.Configuration;

                   services.AddGrpcClient<GrpcDemo.Demo.DemoClient>(options => {
                       options.Address = new Uri("https://localhost:5001");
                   })
                   .AddPolicyHandler(GetRetryPolicy())
                   ;

                   services.AddHostedService<GrpcClientWorker>();
               });


        static Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> GetRetryPolicy()
        {
            var serverErrors = new HttpStatusCode[] {
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.InternalServerError,
                HttpStatusCode.TooManyRequests,
                HttpStatusCode.RequestTimeout
            };

            var gRpcErrors = new StatusCode[] {
                StatusCode.DeadlineExceeded,
                StatusCode.Internal,
                StatusCode.NotFound,
                StatusCode.ResourceExhausted,
                StatusCode.Unavailable,
                StatusCode.Unknown
            };

            static StatusCode? GetStatusCode(HttpResponseMessage response)
            {
                var headers = response.Headers;

                if (!headers.Contains("grpc-status") && response.StatusCode == HttpStatusCode.OK)
                    return StatusCode.OK;

                if (headers.Contains("grpc-status"))
                    return (StatusCode)int.Parse(headers.GetValues("grpc-status").First());

                return null;
            }

            return (request) =>
            {
                return Policy.HandleResult<HttpResponseMessage>(r => {

                    var grpcStatus = GetStatusCode(r);
                    var httpStatusCode = r.StatusCode;

                    return (grpcStatus == null && serverErrors.Contains(httpStatusCode)) ||
                           (httpStatusCode == HttpStatusCode.OK && gRpcErrors.Contains(grpcStatus.Value)); 
                })
                .WaitAndRetryAsync(3, (input) => TimeSpan.FromSeconds(3 + input), (result, timeSpan, retryCount, context) =>
                {
                    var grpcStatus = GetStatusCode(result.Result);
                    Console.WriteLine($"Request failed with {grpcStatus}. Retry");
                });
            };
        }
    }
}
