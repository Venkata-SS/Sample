using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Interfaces
{
    public interface IXmlParser
    {
        string GetParserName();

        ValueTask<T> Parse<T>(XmlReader reader);
    }
}
