using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Interfaces
{
    public interface IOutputGenerator
    {
        ValueTask<bool> AddToOuput(WindGenerator windGenerator);

        ValueTask<bool> AddToOuput(GasGenerator gasGenerator);

        ValueTask<bool> AddToOuput(CoalGenerator coalGenerator);

        void Reset();

        void WriteToFile(string filename);
    }
}
