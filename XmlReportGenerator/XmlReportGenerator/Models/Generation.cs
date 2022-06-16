using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XmlReportGenerator.Models
{
    public class Generation
    {
        public Generation()
        {

        }

        [XmlElement("Day")]
        public Day[] Days { get; set; }
    }
}
