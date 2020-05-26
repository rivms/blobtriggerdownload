using System;
using System.Collections.Generic;
using System.Text;

namespace sbconsumersvc
{
    public class BlobEventConnectionStrings
    {
        public string ServiceBus { get; set; }
        public string BlobSaSTokenQueryString { get; set; }
    }
}
