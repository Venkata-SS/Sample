using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlReportGenerator.Models
{
    public class GenerationOutput
    {
        public GenerationOutput()
        {

        }

        public GeneratorTotal[] Totals { get; set; }

        public DayEmission[] MaxEmissionGenerators { get; set; }

        public ActualHeatRate[] ActualHeatRates { get; set; }
    }
}
