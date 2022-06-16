using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Helpers
{
    public static class GeneratorHelper
    {
        private static readonly Factors _factors;
        static GeneratorHelper()
        {
            _factors = GetFactor();
        }
        public static Factors GeneratorFactors => _factors;
        
        private static Factors GetFactor()
        {
            using (Stream stream = File.OpenRead(@"ReferenceData.xml"))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Factors));
                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {

                            if (reader.Name == "Factors")
                            {
                                var factors = (Factors)xmlSerializer.Deserialize(reader);
                                return factors;
                            }
                        }
                    }
                }
            }
            return new Factors();
        }

        public static void ParseGenerationOutput(GenerationOutput generationOutput, string outputfile)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GenerationOutput));
            
            //Assumption : If file exist with same name then we are reprocessing the same file. So delete and regenerate.
            if (File.Exists(outputfile))
                File.Delete(outputfile);

            using(FileStream stream = File.Create(outputfile))
            {
                xmlSerializer.Serialize(stream, generationOutput);
            }
        }
    }
}
