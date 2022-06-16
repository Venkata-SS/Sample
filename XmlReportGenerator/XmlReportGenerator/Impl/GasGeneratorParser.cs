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
    public class GasGeneratorParser : XmlParser
    {
        private readonly XmlSerializer _serializer;

        public GasGeneratorParser()
        {
            _serializer = new XmlSerializer(typeof(GasGenerator));
        }
        public override string GetParserName()
        {
            return "GasGenerator";
        }

        public async override ValueTask<GasGenerator> Parse<GasGenerator>(XmlReader reader)
        {
            var gasGenerator = ParseXml<GasGenerator>(reader);
            return await ValueTask.FromResult(gasGenerator);
        }

        protected override XmlSerializer GetXmlSerializer()
        {
            return _serializer;
        }
    }
}
