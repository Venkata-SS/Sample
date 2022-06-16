using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using XmlReportGenerator.Interfaces;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Impl
{
    public abstract class XmlParser : IXmlParser
    {
        public abstract string GetParserName();
        public abstract ValueTask<T> Parse<T>(XmlReader reader);
        protected abstract XmlSerializer GetXmlSerializer();

        protected virtual T ParseXml<T>(XmlReader reader)
        {
            var xmlSerializer = GetXmlSerializer();
            if (xmlSerializer != null && reader != null)
            {
                 return (T)xmlSerializer.Deserialize(reader);

            }
            return default(T);
        }
    }
}
