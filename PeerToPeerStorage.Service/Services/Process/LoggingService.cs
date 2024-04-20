using Grpc.Net.Client;
using GRPC_Server;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PeerToPeerStorage.Service.Services.Process
{
    public class LoggingService
    {
        private GrpcChannel channel;
        private Logging.LoggingClient client;

        public LoggingService()
        {
            this.channel = GrpcChannel.ForAddress("https://localhost:5001");
            this.client = new Logging.LoggingClient(channel);
        }

        public void InfoLog(string message)
        {
            var input = new LoggingRequest
            {
                Message = message + " --- > "+DateTime.Now,
            };

            var reply = client.StartLogging(input);
        }
    }
}
