using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XmlReportGenerator.Models
{
    [XmlType("Day")]
    public class DayEmission
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Emission { get; set; }
    }
}
