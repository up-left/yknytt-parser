using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using YKnyttLib;
using IniParser.Parser;

namespace ParseWorlds
{
    class Program
    {
        public static string StringToCSVCell(string str)
        {
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        private static byte[] compress(byte[] file)
        {
            var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                new MemoryStream(file).CopyTo(gzip);
            }
            var result = output.ToArray();
            return result.Length > file.Length ? file : result;
        }

        static void Main(string[] args)
        {
            using System.IO.StreamWriter csvfile = new System.IO.StreamWriter("../worlds.csv");
            foreach (var fname in Directory.GetFiles("../worlds/"))
            {
                Console.WriteLine($"Processing {fname}");

                using var f = File.Open(fname, FileMode.Open);
                var world = new KnyttBinWorldLoader(f);
                var icon = world.GetFile("Icon.png");
                var buffer = world.GetFile("World.ini");
                var content = new ASCIIEncoding().GetString(buffer, 0, buffer.Length);
                var ini_parser = new IniDataParser();
                ini_parser.Configuration.AllowDuplicateKeys = true;
                ini_parser.Configuration.AllowDuplicateSections = true;
                ini_parser.Configuration.SkipInvalidLines = true;
                ini_parser.Configuration.CaseInsensitive = true;
                var ini = ini_parser.Parse(content);

                var filename = Path.GetFileName(fname);
                var name = ini["World"]["Name"];
                var author = ini["World"]["Author"];
                var size = ini["World"]["Size"];
                var description = ini["World"]["Description"];
                var format = ini["World"]["Format"];

                var difficulties = new HashSet<String>();
                var categories = new HashSet<String>();
                foreach (var key in ini["World"])
                {
                    if (key.KeyName.ToLower().StartsWith("difficulty") && key.Value.Length > 0) { difficulties.Add(key.Value); }
                    if (key.KeyName.ToLower().StartsWith("category") && key.Value.Length > 0) { categories.Add(key.Value); }
                }

                HashSet<string> endings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<string> cutscenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var section in ini.Sections)
                {
                    if (!section.SectionName.ToLower().StartsWith('x')) { continue; }

                    if (section.Keys.ContainsKey("Ending"))
                    {
                        var ending = section.Keys["Ending"];
                        if (world.GetFile($"{ending}/scene1.png") != null)
                        {
                            endings.Add(section.Keys["Ending"]);
                        }
                    }

                    foreach (var key in section.Keys)
                    {
                        if (key.KeyName.ToLower().StartsWith("shiftcutscene") && 
                            key.Value.ToLower() != "ending" &&
                            world.GetFile($"{key.Value}/scene1.png") != null)
                        {
                            cutscenes.Add(key.Value);
                        }
                    }
                }
                if (world.GetFile("ending/scene1.png") != null) { endings.Add("Ending"); }

                using MemoryStream ms = new MemoryStream();
                if (icon != null)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.FileName = "/usr/bin/pngquant";
                    startInfo.Arguments = "--quality 40-80 -";

                    Process process = new Process();
                    process.StartInfo = startInfo;
                    process.Start();

                    process.StandardInput.BaseStream.Write(icon);
                    process.StandardOutput.BaseStream.CopyTo(ms);
                }

                string[] cells = {"http://knyttlevels.com/levels/" + Uri.EscapeUriString(filename), name, author, 
                                  size, String.Join(';', difficulties), String.Join(';', categories), 
                                  format, new FileInfo(fname).Length.ToString(), description,
                                  String.Join(';', endings), String.Join(';', cutscenes),
                                  icon != null ? Convert.ToBase64String(compress(ms.ToArray())) : ""};
                csvfile.WriteLine(String.Join(',', cells.Select(cell => StringToCSVCell(cell))));
            }
            Console.WriteLine("Done.");
        }
    }
}
