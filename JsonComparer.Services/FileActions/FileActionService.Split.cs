﻿namespace BigEgg.Tools.JsonComparer.Services.FileActions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;
    using BigEgg.Progress;

    public partial class FileActionService
    {
        /// <summary>
        /// Splits the file.
        /// </summary>
        /// <param name="fileName">The JSON file name.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="progress">The progress update provider.</param>
        /// <returns></returns>
        /// <exception cref="System.UnauthorizedAccessException">Access is denied.</exception>
        /// <exception cref="System.ArgumentNullException">Data is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// fileName is an empty string ("") or null.
        /// or
        /// fileName contains the name of a system device (com1, com2, and so on).
        /// or
        /// outputPath is an empty string ("") or null.
        /// or
        /// nodeName is an empty string ("") or null.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="System.IO.IOException">Path includes an incorrect or invalid syntax for file name, directory name, or volume label syntax.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.NotSupportedException">
        /// Cannot read JSON file
        /// or
        /// Cannot find JSON node
        /// or
        /// Cannot Split JSON node
        /// </exception>
        public async Task SplitFile(string fileName, string outputPath, string nodeName, IProgress<IProgressReport> progress = null)
        {
            await SplitFile(fileName, outputPath, nodeName, string.Empty, progress);
        }

        /// <summary>
        /// Splits the file.
        /// </summary>
        /// <param name="fileName">The JSON file name.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="nodeName">The node name.</param>
        /// <param name="outputFileNamePattern">The output file name pattern.</param>
        /// <param name="progress">The progress update provider.</param>
        /// <returns></returns>
        /// <exception cref="System.UnauthorizedAccessException">Access is denied.</exception>
        /// <exception cref="System.ArgumentNullException">Data is null.</exception>
        /// <exception cref="System.ArgumentException">
        /// fileName is an empty string ("") or null.
        /// or
        /// fileName contains the name of a system device (com1, com2, and so on).
        /// or
        /// outputPath is an empty string ("") or null.
        /// or
        /// nodeName is an empty string ("") or null.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="System.IO.IOException">Path includes an incorrect or invalid syntax for file name, directory name, or volume label syntax.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.NotSupportedException">
        /// Cannot read JSON file
        /// or
        /// Cannot find JSON node
        /// or
        /// Cannot Split JSON node
        /// </exception>
        public async Task SplitFile(string fileName, string outputPath, string nodeName, string outputFileNamePattern, IProgress<IProgressReport> progress = null)
        {
            Preconditions.NotNullOrWhiteSpace(fileName, "fileName");
            Preconditions.NotNullOrWhiteSpace(outputPath, "outputPath");
            Preconditions.NotNullOrWhiteSpace(nodeName, "nodeName");

            Trace.Indent();
            Trace.TraceInformation($"Start split JSON data in file {fileName}");

            JObject jsonFile = (JObject)jsonDocumentService.ReadJsonFile(fileName);
            if (jsonFile == null) { throw new NotSupportedException($"Cannot read JSON file {fileName}"); }

            var node = jsonDocumentService.GetNode(jsonFile, nodeName);
            if (node == null) { throw new NotSupportedException($"Cannot find JSON node {nodeName} in {fileName}"); }

            switch (node.Type)
            {
                case JTokenType.Array:
                    await SplitArray((JArray)node, outputPath, outputFileNamePattern, nodeName, progress);
                    break;
                case JTokenType.Object:
                    await SplitObject((JObject)node, outputPath, outputFileNamePattern, nodeName, progress);
                    break;
                default:
                    throw new NotSupportedException($"Cannot Split JSON node {nodeName} in {fileName}");
            }

            Trace.TraceInformation($"Split JSON data in file {fileName} complete.");
            Trace.Unindent();
        }


        private Task SplitObject(JObject node, string outputPath, string outputFileNamePattern, string nodeName, IProgress<IProgressReport> progress)
        {
            if (string.IsNullOrWhiteSpace(outputFileNamePattern)) { outputFileNamePattern = Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER_NAME; }

            return Task.Factory.StartNew(() =>
            {
                Trace.Indent();
                Trace.TraceInformation($"Start splitting properties in '{nodeName}' node");

                var properties = node.Properties().ToList();
                var length = properties.Count;
                reportProgress(progress, new ProgressReport(0, length));

                if (!Directory.Exists(outputPath)) { Directory.CreateDirectory(outputPath); }
                for (int i = 0; i < length; i++)
                {
                    reportProgress(progress, new ProgressReport(i + 1, length));
                    if (properties[i].Value.Type != JTokenType.Object) { continue; }

                    var fileName = outputFileNamePattern
                            .Replace(Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER_NAME, properties[i].Name)
                            .Replace(Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER_INDEX, (i + 1).ToString());
                    jsonDocumentService.WriteJsonFile((JObject)(properties[i].Value), $"{outputPath}/{fileName}.json");
                }

                Trace.TraceInformation($"Split Complete");
                Trace.Unindent();
            });
        }

        private Task SplitArray(JArray node, string outputPath, string outputFileNamePattern, string nodeName, IProgress<IProgressReport> progress)
        {
            if (string.IsNullOrWhiteSpace(outputFileNamePattern)) { outputFileNamePattern = Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER_INDEX; }

            return Task.Factory.StartNew(() =>
            {
                Trace.Indent();
                Trace.TraceInformation($"Start splitting array items in '{nodeName}' node");

                var children = node.Children().ToList();
                var length = children.Count;
                reportProgress(progress, new ProgressReport(0, length));

                if (!Directory.Exists(outputPath)) { Directory.CreateDirectory(outputPath); }
                for (int i = 0; i < length; i++)
                {
                    if (children[i].Type != JTokenType.Object) { continue; }

                    var fileName = outputFileNamePattern
                            .Replace(Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER_NAME, string.Empty)
                            .Replace(Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER_INDEX, (i + 1).ToString());
                    jsonDocumentService.WriteJsonFile((JObject)(children[i]), $"{outputPath}/{fileName}.json");
                    reportProgress(progress, new ProgressReport(i + 1, length));
                }

                Trace.TraceInformation($"Split Complete");
                Trace.Unindent();
            });
        }
    }
}
