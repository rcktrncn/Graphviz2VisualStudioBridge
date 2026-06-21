using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Graphviz2VisalStudioBridge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("no arguments. please specify dot file path.");
                return;
            }
            var filePath = args[0].Trim();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"file does not exists. Path: {filePath}");
                return;
            }

            var exePath = Assembly.GetEntryAssembly().Location;
            var confPath = Path.Combine(Path.GetDirectoryName(exePath), "conf.json");

            var jsonText = File.ReadAllText(confPath);
            var config = JsonSerializer.Deserialize<Conf>(jsonText);

            var viewerPath = config.viewer_path;
            var outputPrefix = config.output_prefix;

            var graphvizPath = config.graphviz_path;
            if (!graphvizPath.ToLower().EndsWith("\\dot.exe"))
            {
                Console.WriteLine($"Please specify 'dot.exe' in graphviz_path. Path: {graphvizPath}");
                return;
            }
            else if (!File.Exists(graphvizPath))
            {
                Console.WriteLine($"Graphviz does not exists. Path: {graphvizPath}");
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{outputPrefix}{fileName}.png");

            var procInfo = new ProcessStartInfo()
            {
                FileName = graphvizPath,
                Arguments = $"-Tpng \"{filePath}\" -o \"{outputFilePath}\"",
                CreateNoWindow = true,
            };
            var proc = Process.Start(procInfo);
            proc.WaitForExit();

            if (File.Exists(viewerPath))
            {
                if (File.Exists(outputFilePath))
                {
                    var viewInfo = new ProcessStartInfo()
                    {
                        FileName = viewerPath,
                        Arguments = outputFilePath,
                        CreateNoWindow = true,
                    };
                    Process.Start(viewInfo);
                }
                else
                {
                    Console.WriteLine($"no output file.");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"no viewer. Path: {viewerPath}");
                return;
            }
        }
    }
}
