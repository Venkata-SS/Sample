using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XmlReportGenerator.Models
{
    [XmlType("Generator")]
    public class GeneratorTotal
    {
        public string Name { get; set; }

        public decimal Total { get; set; }
    }
}
