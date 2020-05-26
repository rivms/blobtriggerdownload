using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sbconsumersvc
{
    /// <summary>
    /// Helper that executes a PowerShell script. AzCopy cli tool is used to copy the requested blob to
    /// a local folder
    /// </summary>
    public class AzCopy
    {
        private string script; 

        /// <summary>
        /// Powershell script content is read from the supplied path
        /// </summary>
        /// <param name="scriptPath"></param>
        public AzCopy(Uri scriptPath) : this(File.ReadAllText(scriptPath.LocalPath))
        {       
        }

        /// <summary>
        /// Script supplied will be executed on copy
        /// </summary>
        /// <param name="scriptContents"></param>
        public AzCopy(string scriptContents)
        {
            this.script = scriptContents;
        }

        public async Task Copy(string blobUrl, string sas, string fileDestination)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("BlobUrl", blobUrl);
            parameters.Add("SaSToken", sas);
            parameters.Add("Destination", fileDestination);

            await RunScript(script, parameters);
        }

        /// <summary>
        /// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output. 
        /// </summary>
        /// <param name="scriptContents">The script file contents.</param>
        /// <param name="scriptParameters">A dictionary of parameter names and parameter values.</param>
        private async Task RunScript(string scriptContents, Dictionary<string, object> scriptParameters)
        {
            // create a new hosted PowerShell instance using the default runspace.
            // wrap in a using statement to ensure resources are cleaned up.

            using (PowerShell ps = PowerShell.Create())
            {
                // specify the script code to run.
                ps.AddScript(scriptContents);

                // specify the parameters to pass into the script.
                ps.AddParameters(scriptParameters);

                ps.Streams.Debug.DataAdded += (s, e) => WritePSEventToConsole("Debug Output", s, e);
                ps.Streams.Verbose.DataAdded += (s, e) => WritePSEventToConsole("Verbose Output", s, e);
                ps.Streams.Error.DataAdded += (s, e) => WritePSEventToConsole("Error Output", s, e);
                ps.Streams.Warning.DataAdded += (s, e) => WritePSEventToConsole("Warning Output", s, e);
                ps.Streams.Information.DataAdded += (s, e) => WritePSEventToConsole("Information Output", s, e);

                // execute the script and await the result.
                var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);

                // print the resulting pipeline objects to the console.
                foreach (var item in pipelineObjects)
                {
                    Console.WriteLine(item.BaseObject.ToString());
                }
            }
        }

        private void WritePSEventToConsole(string eventType, object sender, DataAddedEventArgs e)
        {
            var streamObjectsReceived = sender as PSDataCollection<InformationRecord>;
            var currentStreamRecord = streamObjectsReceived[e.Index];

            Console.WriteLine($"{eventType}: {currentStreamRecord.MessageData}");
        }
    }
}
