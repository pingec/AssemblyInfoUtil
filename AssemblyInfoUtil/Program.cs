using Fclp;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssemblyInfoUtil
{
    internal class Program
    {
        private static string AssemblyVersion(string changeLogPath)
        {
            //RevisionNumberFromChangeLog := <solution generation>.<major version>.<minor version>.
            //RowNumbersInChangeLog := <number of changes since beginning>
            //Result := RevisionNumberFromChangeLog.RowNumbersInChangeLog
            return $"{RevisionNumberFromChangeLog(changeLogPath)}.{RowNumbersInChangeLog(changeLogPath)}";
        }

        private static int Main(string[] args)
        {
            Console.WriteLine("https://github.com/pingec/AssemblyInfoUtil");

            var p = new FluentCommandLineParser();

            string changelogPath = "";
            string assemblyInfoPath = "";

            p.Setup<string>(CaseType.CaseInsensitive, new string[] { "c", "changelog" })
                .Callback(path =>
                {
                    changelogPath = Path.GetFullPath(path);
                })
                .Required()
                .WithDescription("Path to the changelog");

            p.Setup<string>(CaseType.CaseInsensitive, new string[] { "a", "assemblyFile" })
                .Callback(path =>
                {
                    assemblyInfoPath = Path.GetFullPath(path);
                })
                .Required()
                .WithDescription("Path to the AssemblyInfo.cs (or similar) to modify");

            p.SetupHelp("?", "help")
                .Callback(text => Console.WriteLine(text));

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
                return 1;
            }

            ReplaceAssemblyVersion(assemblyInfoPath, changelogPath);
            return 0;
        }

        private static void ReplaceAssemblyVersion(string assemblyInfoPath, string changeLogPath)
        {
            var assemblyVersion = AssemblyVersion(changeLogPath);
            Console.WriteLine($"Calculated version: {assemblyVersion}");
            var assemblyInfoUpdated = false;


            var lines = File.ReadAllLines(assemblyInfoPath).Select(l =>
            {
                if (l.Contains("AssemblyVersion") && !l.Contains(assemblyVersion))
                {
                    assemblyInfoUpdated = true;
                    return $"[assembly: AssemblyVersion(\"{AssemblyVersion(changeLogPath)}\")]";
                }
                else
                    return l;
            }).ToList();

            if (assemblyInfoUpdated)
            {
                Console.WriteLine($"Modifying {assemblyInfoPath} versin to {assemblyVersion} (calculated from {changeLogPath})");
                File.WriteAllLines(assemblyInfoPath, lines);
            }
            {
                Console.WriteLine($"No changes required");
            }
        }
        private static string RevisionNumberFromChangeLog(string changeLogPath)
        {
            string contents = File.ReadAllText(changeLogPath);
            MatchCollection matches = Regex.Matches(contents, @"^\d+\.\d+\.\d+", RegexOptions.Multiline);
            return matches[0].Value;
        }

        private static int RowNumbersInChangeLog(string changeLogPath)
        {
            return File.ReadAllLines(changeLogPath).Length;
        }
    }
}