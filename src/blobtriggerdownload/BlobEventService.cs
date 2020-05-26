using log4net.Repository.Hierarchy;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Logging;

namespace sbconsumersvc
{
    public sealed class BlobEventService 
    {
        readonly LogWriter logger = HostLogger.Get<BlobEventService>();
        private CancellationTokenSource source = new CancellationTokenSource();
        private BlobEventConfig Config;
        private BlobEventConnectionStrings ConnectionStrings;
        private IQueueClient queueClient;


        public BlobEventService(IOptions<BlobEventConfig> blobEventConfig, IOptions<BlobEventConnectionStrings> connectionStrings)
        {
            ConnectionStrings = connectionStrings.Value;
            Config = blobEventConfig.Value;
        }

        public bool Start()
        {
            queueClient = new QueueClient(ConnectionStrings.ServiceBus, Config.QueueName);
            RegisterOnMessageHandlerAndReceiveMessages();
            return true;
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages.
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            try
            {
                var body = Encoding.UTF8.GetString(message.Body);
                // Process the message.
                Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{body}");

                var be = BlobEvent.Parse(body);

                if (be != null)
                {
                    Console.WriteLine($"Downloading blob: {be.Url}");

                    var location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        Config.PowershellScript);

                    var az = new AzCopy(new Uri("file:///" + location));
                    await az.Copy(be.Url, ConnectionStrings.BlobSaSTokenQueryString, Config.DestinationFolder);
                }
                else
                {
                    Console.WriteLine("Unexpected event");
                }



                // Complete the message so that it is not received again.
                // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
                await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
                // to avoid unnecessary exceptions.
            } catch (Exception ex)
            {
                Console.WriteLine($"Error handling message: {ex}");
            }
        }

        // Use this handler to examine the exceptions received on the message pump.
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public bool Stop()
        {
            source.Cancel();
            return true;
        }
    }
}
