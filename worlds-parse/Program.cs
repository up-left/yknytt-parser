using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
                    if (key.KeyName.StartsWith("Difficulty") && key.Value.Length > 0) { difficulties.Add(key.Value); }
                    if (key.KeyName.StartsWith("Category") && key.Value.Length > 0) { categories.Add(key.Value); }
                }

                string[] cells = {"http://knyttlevels.com/levels/" + Uri.EscapeUriString(filename), name, author, 
                                  size, String.Join(';', difficulties), String.Join(';', categories), 
                                  format, new FileInfo(fname).Length.ToString(), description,
                                  icon != null ? Convert.ToBase64String(icon) : ""};
                csvfile.WriteLine(String.Join(',', cells.Select(cell => StringToCSVCell(cell))));
            }
            Console.WriteLine("Done.");
        }
    }
}
