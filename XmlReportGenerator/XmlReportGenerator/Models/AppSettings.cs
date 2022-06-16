using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlReportGenerator.Models
{
    public class AppSettings
    {
        public string InputDirectory { get; set; }

        public string OutputDirectory { get; set; }

        public string OutputFileAppender { get; set; }

        public string ProcessedDirectory { get; set; }

        public string ExceptionDirectory { get; set; }

        public int MaxFilesInSingleRead { get; set; }
    }
}
