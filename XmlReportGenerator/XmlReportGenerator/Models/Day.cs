using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlReportGenerator.Models
{
    public class Day
    {
        public Day()
        {

        }

        public DateTime Date { get; set; }
        public decimal Energy { get; set; }
        public decimal Price { get; set; }

    }
}
