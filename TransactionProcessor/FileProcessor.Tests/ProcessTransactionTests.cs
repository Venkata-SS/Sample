using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FileProcessor.Impl;
using FileProcessor.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;

namespace FileProcessor.Tests
{
    [TestFixture]
    public class ProcessTransactionTests
    {
        private IConfiguration _configuration;
        private List<IOrderFileProcessor> _orderFileProcessors;
        private Mock<ILogger<ProcessTransaction>> _mockLogger;
        private Mock<IOrderFileProcessor> _mockBBB;


        [SetUp]
        public void Setup()
        {

            var _mockAAA = new Mock<IOrderFileProcessor>();
            _mockAAA.Setup(x => x.GetOMSName()).Returns("AAA");
            _mockAAA.Setup(x => x.IsMandatoryConfigExistsAsync()).Returns(ValueTask.FromResult(true));
            _mockAAA.Setup(x => x.CreateFileAsync()).Returns(ValueTask.FromResult("filename.aaa"));
            _mockAAA.Setup(x => x.WriteLineAsync(It.IsAny<string>())).Returns(ValueTask.FromResult(true));

            _mockBBB = new Mock<IOrderFileProcessor>();
            _mockBBB.Setup(x => x.GetOMSName()).Returns("BBB");
            _mockBBB.Setup(x => x.IsMandatoryConfigExistsAsync()).Returns(ValueTask.FromResult(true));
            _mockBBB.Setup(x => x.CreateFileAsync()).Returns(ValueTask.FromResult("filename.bbb"));
            _mockBBB.Setup(x => x.WriteLineAsync(It.IsAny<string>())).Returns(ValueTask.FromResult(true));

            var _mockCCC = new Mock<IOrderFileProcessor>();
            _mockCCC.Setup(x => x.GetOMSName()).Returns("CCC");
            _mockCCC.Setup(x => x.IsMandatoryConfigExistsAsync()).Returns(ValueTask.FromResult(true));
            _mockCCC.Setup(x => x.CreateFileAsync()).Returns(ValueTask.FromResult("filename.ccc"));
            _mockCCC.Setup(x => x.WriteLineAsync(It.IsAny<string>())).Returns(ValueTask.FromResult(true));

            _orderFileProcessors = new List<IOrderFileProcessor>
            {
                {_mockAAA.Object },
                {_mockBBB.Object },
                {_mockCCC.Object }
            };

            _mockLogger = new Mock<ILogger<ProcessTransaction>>();

            _configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
        }

        [Test]
        public async Task ProcessFile_EmptyFile_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile(string.Empty);

            Assert.IsTrue(!result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "fileWithPath is empty"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessStream_EmptyStream_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFileStream(null);

            Assert.IsTrue(!result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "File or Stream is empty"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_EmptyConfig_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, null, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(!result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to load config"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_EmptyOrderProcessors_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(null, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(!result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to load Order file processors"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_NonCSVFileLine_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsNonCsv.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line is not in csv format")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_LessColumns_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissing.csv");
                       
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have necessary columns")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_MissingOmsConfig_Returns_False_And_Logs()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsWithNoOMS.json").Build();
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(!result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "ValidOmsProcessor is missing in the config"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_UnsupportedOms_MoveToException_Logs()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsWithInvalidOMS.json").Build();
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unsuppoted OMS : BBB")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            var filename = Directory.GetFiles(path).First();
            var actualLine = File.ReadAllLines(filename).Skip(1).First();
            Assert.IsTrue(File.Exists(filename));
            Assert.AreEqual("2,2,20,BBB,SELL", actualLine);
           
        }

        [Test]
        public async Task ProcessFile_NoOmsProcessor_MoveToException_Logs()
        {
            _mockBBB.Setup(x => x.GetOMSName()).Returns("BBB1");

            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get processor for OMS : BBB"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            var filename = Directory.GetFiles(path).First();
            var actualLine = File.ReadAllLines(filename).Skip(1).First();
            Assert.IsTrue(File.Exists(filename));
            Assert.AreEqual("2,2,20,BBB,SELL", actualLine);
        }

        [Test]
        public async Task ProcessFile_MandatoryConfig_Missing_MoveToException_Logs()
        {
            _mockBBB.Setup(x => x.IsMandatoryConfigExistsAsync()).Returns(ValueTask.FromResult(false));

            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Necessary configuration or metadata is missing for OMS : BBB"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            var filename = Directory.GetFiles(path).First();
            var actualLine = File.ReadAllLines(filename).Skip(1).First();
            Assert.IsTrue(File.Exists(filename));
            Assert.AreEqual("2,2,20,BBB,SELL", actualLine);
        }

        [Test]
        public async Task ProcessFile_UnableTo_CreateOrder_MoveToException_Logs()
        {
            _mockBBB.Setup(x => x.WriteLineAsync(It.IsAny<string>())).Returns(ValueTask.FromResult(false));

            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to create order for OMS : BBB and line : 2,2,20,BBB,SELL"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            var filename = Directory.GetFiles(path).First();
            var actualLine = File.ReadAllLines(filename).Skip(1).First();
            Assert.IsTrue(File.Exists(filename));
            Assert.AreEqual("2,2,20,BBB,SELL", actualLine);
        }

        [Test]
        public async Task ProcessFile_UnhandledException_MoveToException_Logs()
        {
            _mockBBB.Setup(x => x.WriteLineAsync(It.IsAny<string>())).Throws(new Exception());

            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactions.csv");

            Assert.IsTrue(result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error processing Line : 2,2,20,BBB,SELL"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            var filename = Directory.GetFiles(path).First();
            var actualLine = File.ReadAllLines(filename).Skip(1).First();
            Assert.IsTrue(File.Exists(filename));
            Assert.AreEqual("2,2,20,BBB,SELL", actualLine);
        }

        [Test]
        public async Task ProcessStream_UnhandledException_Terminates_Logs()
        {

            Stream fileStream = File.OpenRead("transactions.csv");
            fileStream.Close();
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFileStream(fileStream);

            Assert.IsTrue(!result);

            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error processing file"),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }

        [Test]
        public async Task ProcessFile_MissingSecurity_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissingMandatory.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have securityId")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_MissingPortfolioId_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissingMandatory.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have portfolioId")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_MissingNominal_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissingMandatory.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have nominal")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_MissingOMS_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissingMandatory.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have oms")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_MissingTransactionType_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissingMandatory.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have TransactionType")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [Test]
        public async Task ProcessFile_CorrectTransactionType_Returns_False_And_Logs()
        {
            var processTransaction = new ProcessTransaction(_orderFileProcessors, _configuration, _mockLogger.Object);
            var result = await processTransaction.ProcessFile("transactionsMissingMandatory.csv");
            _mockLogger.Verify(x => x.Log(
                                It.Is<LogLevel>(l => l == LogLevel.Error),
                                It.IsAny<EventId>(),
                                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Line doesn't have correct TransactionType")),
                                It.IsAny<Exception>(),
                                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        }

        [TearDown]

        public void TearDown()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
            foreach (string fileName in Directory.GetFiles(path))
                File.Delete(fileName);
        }

    }
}
