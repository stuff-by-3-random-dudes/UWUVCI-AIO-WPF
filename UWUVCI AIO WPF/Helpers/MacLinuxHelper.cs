using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UWUVCI_AIO_WPF.Models;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class MacLinuxHelper
    {
        public static void WriteFailedStepToJson(string functionName, string toolName, string arguments, string currentDirectory = "")
        {
            // Get the base directory where the application is running
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string toolsJsonPath = Path.Combine(basePath, "tools.json");

            if (string.IsNullOrEmpty(currentDirectory))
                currentDirectory = basePath;

            var step = new ToolStep
            {
                ToolName = toolName,
                Arguments = arguments,
                CurrentDirectory = currentDirectory,
                Function = functionName
            };

            List<ToolStep> steps;

            if (File.Exists(toolsJsonPath))
                steps = JsonConvert.DeserializeObject<List<ToolStep>>(File.ReadAllText(toolsJsonPath)) ?? new List<ToolStep>();
            else
                steps = new List<ToolStep>();

            steps.Add(step);
            File.WriteAllText(toolsJsonPath, JsonConvert.SerializeObject(steps));
        }
    }
}
