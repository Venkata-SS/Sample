using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlReportGenerator.Helpers;
using XmlReportGenerator.Interfaces;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Impl
{
    public class XmlFile : IXmlFile
    {
        public async ValueTask<bool> WriteToXML(GenerationOutput generationOutput, string filename)
        {
            GeneratorHelper.ParseGenerationOutput(generationOutput, filename);
            return await ValueTask.FromResult(true);
        }
    }
}
