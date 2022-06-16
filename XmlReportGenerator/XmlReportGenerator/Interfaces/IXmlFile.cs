using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Interfaces
{
    public interface IXmlFile
    {
        ValueTask<bool> WriteToXML(GenerationOutput generationOutput, string filename);
    }
}
