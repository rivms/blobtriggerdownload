using System;
using System.Collections.Generic;
using System.Text;

namespace sbconsumersvc
{
    public class BlobEventConfig
    {
        public string PowershellScript { get; set; }
        public string QueueName { get; set; }

        public string DestinationFolder { get; set; }
    }
}
