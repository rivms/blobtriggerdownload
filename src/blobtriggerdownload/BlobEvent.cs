using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace sbconsumersvc
{
    public class BlobEvent
    {
        private const string Blob_Created = "Microsoft.Storage.BlobCreated";
        public static BlobEvent Parse(string json)
        {
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                var root = document.RootElement;
                var eventType = root.GetProperty("eventType").GetString();

                if (string.Compare(eventType, Blob_Created, true)==0)
                {
                    return ParseBlobCreated(root);
                }
            }

            return null;
        }

        private static BlobEvent ParseBlobCreated(JsonElement root)
        {
            var dataElement = root.GetProperty("data");
            var blobUrl = dataElement.GetProperty("url").GetString();
            return new BlobEvent(blobUrl);
        }


        public string Url { get; private set; }

        protected BlobEvent(string url)
        {
            Url = url;
        }
    }
}
