using Continuum.Common.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnluacNET;

namespace Continuum.Core.Utilities
{
    public class UnluacUtility
    {
        public static void DecompileFolder(string folder)
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                var info = new FileInfo(file);

                if (info.Extension != ".lua")
                    continue;

                var tempPath = Path.Combine(info.DirectoryName, $"__temp__{info.Name}");
                Decompile(file, tempPath);
                File.Move(tempPath, file, true);
            }
        }

        public static void Decompile(string inputPath, string outputPath)
        {
            Logger.Log($"Decompiling file {inputPath} with Unluac.Net", LogSeverity.Info);

            LFunction lMain = null;

            try
            {
                lMain = FileToFunction(inputPath);
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR - UNLUAC.NET]: {ex}", LogSeverity.Error);
                throw;
            }

            var d = new Decompiler(lMain);
            d.Decompile();

            try
            {
                using (var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false)))
                {
                    d.Print(new Output(writer));
                    writer.Flush();

                    Logger.Log($"Successfully decompiled to '{outputPath}'", LogSeverity.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR - UNLUAC.NET]: {ex}", LogSeverity.Error);
                throw;
            }
        }

        private static LFunction FileToFunction(string fn)
        {
            using (var fs = File.Open(fn, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var header = new BHeader(fs);
                return header.Function.Parse(fs, header);
            }
        }
    }
}
