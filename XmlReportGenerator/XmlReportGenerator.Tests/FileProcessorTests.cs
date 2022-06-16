using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XmlReportGenerator.Impl;
using XmlReportGenerator.Interfaces;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Tests
{
    [TestFixture]
    public class FileProcessorTests
    {
        private IConfiguration _configuration;
        private IEnumerable<IXmlParser> _xmlParsers;
        private Mock<ILogger<FileProcessor>> _mockLogger;
        private IOutputGenerator _outputGenerator;
        private CancellationTokenSource _cts;
        private GenerationOutput _generationOutput;
        private Mock<IXmlFile> mockXmlFile;

        [SetUp]
        public void Setup()
        {
            _generationOutput = new GenerationOutput();
            _mockLogger = new Mock<ILogger<FileProcessor>>();
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            _xmlParsers = new List<XmlParser>()
            {
                new WindGeneratorParser(),
                new CoalGeneratorParser(),
                new GasGeneratorParser()
            };
           

            mockXmlFile = new Mock<IXmlFile>();
            mockXmlFile.Setup(x => x.WriteToXML(It.IsAny<GenerationOutput>(), It.IsAny<string>())).Returns((GenerationOutput g, string f) =>
            {
                _generationOutput = g;
                return ValueTask.FromResult(true);
            });

            _outputGenerator = new OutputGenerator(mockXmlFile.Object);
            _cts = new CancellationTokenSource();
        }

        [Test]
        public async Task NullParsers_ThrowsException()
        {
            var fileProcessor = new FileProcessor(null, null, null, null);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("XmlParsers are null"));
        }
        [Test]
        public async Task NullAppSettings_ThrowsException()
        {
            var fileProcessor = new FileProcessor(_xmlParsers, null, null, null);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("AppSettings is null"));
        }
        [Test]
        public async Task NullOutputGenerator_ThrowsException()
        {
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, null);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("OutputGenerator is null"));
        }
        [Test]
        public async Task EmptyInputDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsMissingInputDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("InputDirectory hasn't been configured"));
        }

        [Test]
        public async Task Invalid_InputDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsInvalidInputDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("InputDirectory doesn't exists"));
        }

        [Test]
        public async Task EmptyOutputDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsMissingOutputDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("OutputDirectory hasn't been configured"));
        }

        [Test]
        public async Task Invalid_OutputDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsInvalidOutputDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("OutputDirectory doesn't exists"));
        }

        [Test]
        public async Task EmptyProcessDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsMissingProcessDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("ProcessedDirectory hasn't been configured"));
        }

        [Test]
        public async Task Invalid_ProcessDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsInvalidProcessDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("ProcessedDirectory doesn't exists"));
        }

        [Test]
        public async Task EmptyExceptionDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsMissingExpDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("ExceptionDirectory hasn't been configured"));
        }

        [Test]
        public async Task Invalid_ExceptionDirectory_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsInvalidExpDir.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("ExceptionDirectory doesn't exists"));
        }

        [Test]
        public async Task EmptyFileAppender_ThrowsException()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsMissingFileAppender.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            var ex = Assert.ThrowsAsync<Exception>(async () => await fileProcessor.ProcessXmlFilesAsync(_cts));
            Assert.That(ex.Message, Is.EqualTo("OutputFileAppender hasn't been configured"));
        }

        [Test]
        public async Task Processor_Calculates_CorrectTotalValue()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);

           
            var cts = new CancellationTokenSource();
            var result = await fileProcessor.ProcessXmlFilesAsync(new string[] { "01-Basic-Test.xml" } ,cts);


            Assert.IsTrue(result);
            Assert.IsNotNull(_generationOutput);
            Assert.AreEqual(4, _generationOutput.Totals.Count());           

            //Check Total Value
            Assert.AreEqual(1662.617445705, _generationOutput.Totals.Where( t => string.Compare("Wind[Offshore]",t.Name) ==0).First().Total  );
            Assert.AreEqual(4869.453917394, _generationOutput.Totals.Where(t => string.Compare("Wind[Onshore]", t.Name) == 0).First().Total);
            Assert.AreEqual(8512.254605520, _generationOutput.Totals.Where(t => string.Compare("Gas[1]", t.Name) == 0).First().Total);
            Assert.AreEqual(5341.716526632, _generationOutput.Totals.Where(t => string.Compare("Coal[1]", t.Name) == 0).First().Total);

        }

        [Test]
        public async Task Processor_Calculates_CorrectMaxEmissionPerDay()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);


            var cts = new CancellationTokenSource();
            var result = await fileProcessor.ProcessXmlFilesAsync(new string[] { "02-Basic-Test.xml" }, cts);


            Assert.IsTrue(result);
            Assert.IsNotNull(_generationOutput);
            Assert.AreEqual(3, _generationOutput.MaxEmissionGenerators.Count());
                      
            //Check Max Emission per day
            Assert.AreEqual(137.175004008, _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 01)).First().Emission);
            Assert.AreEqual(new DateTime(2017, 01, 01), _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 01)).First().Date.Date);

            Assert.AreEqual(136.440767624, _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 02)).First().Emission);
            Assert.AreEqual(new DateTime(2017, 01, 02), _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 02)).First().Date.Date);

            Assert.AreEqual(5.132380700, _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Gas[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 03)).First().Emission);
            Assert.AreEqual(new DateTime(2017, 01, 03), _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Gas[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 03)).First().Date.Date);

        }

        [Test]
        public async Task Processor_Calculates_CorrectActualHeatRates()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);


            var cts = new CancellationTokenSource();
            var result = await fileProcessor.ProcessXmlFilesAsync(new string[] { "03-Basic-Test.xml" }, cts);


            Assert.IsTrue(result);
            Assert.IsNotNull(_generationOutput);
            Assert.AreEqual(1, _generationOutput.ActualHeatRates.Count());
            
            //Check Heat Rate
            Assert.AreEqual(1, _generationOutput.ActualHeatRates.Where(t => string.Compare("Coal[1]", t.Name) == 0).First().HeatRate);
        }

        [Test]
        public async Task IsFile_Moved_To_ProcessedFolder_After_Processsed_Successfully()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var appSettings = _configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            PrepareFile("04-Basic-Test.xml");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await fileProcessor.ProcessXmlFilesAsync(cts);

            var processedFile = Path.Combine(appSettings.ProcessedDirectory, "04-Basic-Test.xml");
            Assert.IsTrue(File.Exists(processedFile));            
        }

        [Test]
        public async Task IsFile_Moved_To_OutputFolder_After_Processsed_Successfully()
        {
            
            _outputGenerator = new OutputGenerator(new XmlFile());
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var appSettings = _configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            PrepareFile("05-Basic-Test.xml");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await fileProcessor.ProcessXmlFilesAsync(cts);

            var processedFile = Path.Combine(appSettings.OutputDirectory, "05-Basic-Test-Result.xml");
            Assert.IsTrue(File.Exists(processedFile));
        }

        [Test]
        public async Task IsFile_Moved_To_ExceptionFolder_After_Processsed_Failed()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var appSettings = _configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            PrepareFile("ExceptionTest.txt");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await fileProcessor.ProcessXmlFilesAsync(cts);

            var processedFile = Path.Combine(appSettings.ExceptionDirectory, "ExceptionTest.txt");
            Assert.IsTrue(File.Exists(processedFile));
        }

        [Test]
        public async Task File_With_All_Invalid_Generators_Moved_To_ExceptionFolder()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var appSettings = _configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            PrepareFile("InvalidGenerator.xml");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await fileProcessor.ProcessXmlFilesAsync(cts);

            var processedFile = Path.Combine(appSettings.ExceptionDirectory, "InvalidGenerator.xml");
            Assert.IsTrue(File.Exists(processedFile));
        }

        [Test]
        public async Task IsProcessFile_contains_correct_data()
        {

            _outputGenerator = new OutputGenerator(new XmlFile());
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsTests.json").Build();
            var appSettings = _configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            var fileProcessor = new FileProcessor(_xmlParsers, _configuration, _mockLogger.Object, _outputGenerator);
            PrepareFile("06-Basic-Test.xml");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await fileProcessor.ProcessXmlFilesAsync(cts);

            var processedFile = Path.Combine(appSettings.OutputDirectory, "06-Basic-Test-Result.xml");
            Serialize(processedFile);

            Assert.IsTrue(File.Exists(processedFile));

            //Check Total Value
            Assert.AreEqual(4, _generationOutput.Totals.Count());            
            Assert.AreEqual(1662.617445705, _generationOutput.Totals.Where(t => string.Compare("Wind[Offshore]", t.Name) == 0).First().Total);
            Assert.AreEqual(4869.453917394, _generationOutput.Totals.Where(t => string.Compare("Wind[Onshore]", t.Name) == 0).First().Total);
            Assert.AreEqual(8512.254605520, _generationOutput.Totals.Where(t => string.Compare("Gas[1]", t.Name) == 0).First().Total);
            Assert.AreEqual(5341.716526632, _generationOutput.Totals.Where(t => string.Compare("Coal[1]", t.Name) == 0).First().Total);

            //Check Max Emission per day
            Assert.AreEqual(3, _generationOutput.MaxEmissionGenerators.Count());            
            Assert.AreEqual(137.175004008, _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 01)).First().Emission);
            Assert.AreEqual(new DateTime(2017, 01, 01), _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 01)).First().Date.Date);

            Assert.AreEqual(136.440767624, _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 02)).First().Emission);
            Assert.AreEqual(new DateTime(2017, 01, 02), _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Coal[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 02)).First().Date.Date);

            Assert.AreEqual(5.132380700, _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Gas[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 03)).First().Emission);
            Assert.AreEqual(new DateTime(2017, 01, 03), _generationOutput.MaxEmissionGenerators.Where(t => string.Compare("Gas[1]", t.Name) == 0 && t.Date.Date == new DateTime(2017, 01, 03)).First().Date.Date);

            //Check Heat Rate
            Assert.AreEqual(1, _generationOutput.ActualHeatRates.Count());
            Assert.AreEqual(1, _generationOutput.ActualHeatRates.Where(t => string.Compare("Coal[1]", t.Name) == 0).First().HeatRate);
        }

        private void Serialize(string filename)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GenerationOutput));

            using( var fileStream = File.OpenRead(filename))
            {
                _generationOutput =  xmlSerializer.Deserialize(fileStream) as GenerationOutput;
            }

        }

        private bool StopWhenProcesed(string filename, CancellationTokenSource cancellationTokenSource)
        {
            while (true)
            {
                var result = !File.Exists(filename);
                if (result)
                    break;
            }
            return true;
        }

        private void PrepareFile(string filename)
        {
            var appSettings = _configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            var inputFile = Path.Combine(appSettings.InputDirectory, filename);
            File.Move(filename, inputFile, true);
        }
    }
}