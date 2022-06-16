using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Impl
{
    public class CoalGeneratorParser :XmlParser
    {
        private readonly XmlSerializer _serializer;
        

        public CoalGeneratorParser()
        {
            _serializer = new XmlSerializer(typeof(CoalGenerator));
        }
        public override string GetParserName()
        {
            return "CoalGenerator";
        }

        public async override ValueTask<CoalGenerator> Parse<CoalGenerator>(XmlReader reader)
        {
            var result = ParseXml<CoalGenerator>(reader);
            return await ValueTask.FromResult(result);
        }

        protected override XmlSerializer GetXmlSerializer()
        {
            return _serializer;
        }
    }
}
