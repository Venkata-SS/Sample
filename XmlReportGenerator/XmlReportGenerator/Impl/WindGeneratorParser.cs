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
    public class WindGeneratorParser : XmlParser
    {
        private readonly XmlSerializer _serializer;

        public WindGeneratorParser()
        {
            _serializer = new XmlSerializer(typeof(WindGenerator));
        }
        public override string GetParserName()
        {
            return "WindGenerator";
        }

        public async override ValueTask<WindGenerator> Parse<WindGenerator>(XmlReader reader)
        {
            var result = ParseXml<WindGenerator>(reader);
            return await ValueTask.FromResult(result);
        }

        protected override XmlSerializer GetXmlSerializer()
        {
            return _serializer;
        }
    }
}
