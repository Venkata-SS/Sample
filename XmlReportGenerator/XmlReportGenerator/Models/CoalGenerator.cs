using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlReportGenerator.Models
{
    public class CoalGenerator : EmissionGenerator
    {
        public CoalGenerator()
        {

        }
        public decimal TotalHeatInput { get; set; }

        public decimal ActualNetGeneration { get; set; }
        
    }
}
