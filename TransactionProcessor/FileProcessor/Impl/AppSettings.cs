using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Impl
{
    public class AppSettings
    {
        public FileSetting[] FileSettings { get; set; }

        public OmsSetting[] OmsSettings { get; set; }

        public bool IsFirtsLineHeading { get; set; }

        public string ValidOmsProcessor { get; set; }

        public string ValidTransactionType { get; set; }

        public string ExceptionFilePath { get; set; }

    }

    public class FileSetting
    {
        public string ColumnName { get; set; }
        public int ColumnOrder { get; set; }
        public string DataType { get; set; }
        public bool IsMandatory { get; set; }

    }

    public class OmsSetting
    {
        public string OmsName { get; set; }
        public string FileExtension { get; set; }     
        public string FileHeader { get; set; }
        public string Delimiter { get; set; }
        public string OutputFilePath { get; set; }
    }
}
