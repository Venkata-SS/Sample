using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Impl
{
    public class Helper
    {
        public static async Task<bool> CreateFile(string filename, string header)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                using (FileStream fs = File.Create(filename))
                {
                    if (!string.IsNullOrWhiteSpace(header))
                    {
                        using (var writer = new StreamWriter(fs))
                        {
                            await writer.WriteLineAsync(header);
                            await writer.FlushAsync();
                        }
                    }
                    fs.Close();
                }
                return true;
            }
            return false;
        }

        public static async Task<bool> WriteFile(string filename, string orderLine)
        {
            if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(orderLine))
            {
                using (var file = File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (var writer = new StreamWriter(file))
                    {
                        await writer.WriteLineAsync(orderLine);
                        await writer.FlushAsync();
                    }
                }
                return true;
            }
            return false;
        }

        public static int ConvertToInt(string columnvalue)
        {
            if (!string.IsNullOrWhiteSpace(columnvalue))
            {
                var returnValue = 0;
                if (int.TryParse(columnvalue, out returnValue))
                {
                    return returnValue;
                }
            }
            return 0;
        }

        public static string GetColumnValue( FileSetting[] fileSettings, string line, string columnName)
        {
            if (!string.IsNullOrWhiteSpace(line) && fileSettings?.Count() > 0)
            {
                var strSplit = line.Split(",");
                if (strSplit?.Length > 0)
                {
                    var setting = fileSettings.FirstOrDefault(f => f.ColumnName == columnName);
                    if (setting != null && setting.ColumnOrder > 0)
                    {
                        var index = setting.ColumnOrder - 1;
                        if (!string.IsNullOrWhiteSpace(strSplit.ElementAtOrDefault(index)))
                        {
                            return strSplit[index];
                        }
                    }
                }
            }
            return string.Empty;
        }

        public static string GetFileName(string? outputFilePath, string defaultFolder, string fileExtension)
        {
            var path = outputFilePath;

            //Default folder if absent form config
            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultFolder);

            //Create missing directory
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //Maintain uniqueness of the file name 
            var filename = $"{System.Guid.NewGuid()}{fileExtension}";
            var filenameWithPath = Path.Combine(path, filename);
            return filenameWithPath;
        }
    }
}
