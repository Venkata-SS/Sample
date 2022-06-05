using FileProcessor.Impl;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Interfaces;
using TransactionProcessor.Repository.Interfaces.Models;

namespace FileProcessor.Tests
{
    [TestFixture]
    public class OrderFileProcessorTests
    {
        private OrderFileProcessor _orderFileProcessor;
        private Mock<IMetadataRepository> _mockMetadataRepository;
        private IConfiguration _configuration;
        private string _filename;
        private Security _security;
        private Portfolio _portfolio;

        [OneTimeSetUp]
        
        public void OneTimeSetUp()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
        }

        [SetUp]
        
        public void Setup()
        {
            _security = new Security(1, "ISIN11111111", "s1", "CUSIP0001");
            _portfolio = new Portfolio(1, "p1");

            _mockMetadataRepository =  new Mock<IMetadataRepository>();

            _mockMetadataRepository.Setup(m => m.GetTotalPortfoliosAsync()).Returns(ValueTask.FromResult(2));
            _mockMetadataRepository.Setup(m => m.GetTotalSecuritiesAsync()).Returns(ValueTask.FromResult(2));

            _mockMetadataRepository.Setup(m => m.GetSecurityAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_security));
            _mockMetadataRepository.Setup(m => m.GetPortfolioAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_portfolio));
        }

        private void SetOrderFileProcessor(string omsname)
        {
            if (omsname == "AAA")
                _orderFileProcessor = new AAAOrderFileProcessor(_mockMetadataRepository.Object, _configuration);
            else if (omsname == "BBB")
                _orderFileProcessor = new BBBOrderFileProcessor(_mockMetadataRepository.Object, _configuration);
            if (omsname == "CCC")
                _orderFileProcessor = new CCCOrderFileProcessor(_mockMetadataRepository.Object, _configuration);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public void GetOMSName_Returns_Result(string omsName)
        {
            SetOrderFileProcessor(omsName);            
            var result = _orderFileProcessor.GetOMSName();
            Assert.AreEqual(omsName, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task IsMandatoryConfigExistsAsync_Returns_Result(string omsName)
        {
            SetOrderFileProcessor(omsName);
            var result = await _orderFileProcessor.IsMandatoryConfigExistsAsync();
            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task NoSecurities_MandatoryConfigExists_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _mockMetadataRepository.Setup(m => m.GetTotalSecuritiesAsync()).Returns(ValueTask.FromResult(0));
            var result = await _orderFileProcessor.IsMandatoryConfigExistsAsync();
            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task NoPortfolios_MandatoryConfigExists_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _mockMetadataRepository.Setup(m => m.GetTotalPortfoliosAsync()).Returns(ValueTask.FromResult(0));
            var result = await _orderFileProcessor.IsMandatoryConfigExistsAsync();
            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task NoDelimiterConfig_MandatoryConfigExists_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _orderFileProcessor = new AAAOrderFileProcessor(_mockMetadataRepository.Object, null);
            var result = await _orderFileProcessor.IsMandatoryConfigExistsAsync();
            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task NoFileExtensionConfig_MandatoryConfigExists_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _orderFileProcessor = new AAAOrderFileProcessor(_mockMetadataRepository.Object, null);
            var result = await _orderFileProcessor.IsMandatoryConfigExistsAsync();
            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task CreateFile_Returns_Result(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            Assert.IsNotEmpty(_filename);
            Assert.IsTrue(File.Exists(_filename));
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task GetFileName_Returns_CorrectFilename(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();          
            Assert.AreEqual(_filename,_orderFileProcessor.GetFileName());
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task CreateFile_DifferentOutputPath_Returns_Result(string omsName)
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appSettingsWithOuput.json").Build();
            SetOrderFileProcessor(omsName);           
            _filename = await _orderFileProcessor.CreateFileAsync();
            Assert.IsNotEmpty(_filename);
            Assert.IsTrue(File.Exists(_filename));
            Assert.AreEqual(Path.GetDirectoryName(_filename), "D:\\Output");
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA", ".aaa")]
        [TestCase("BBB", ".bbb")]
        [TestCase("CCC", ".ccc")]
        public async Task CreateFile_Returns_CorrectFileExtension(string omsName, string fileExtension)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            var extension   = new System.IO.FileInfo(_filename).Extension;
            Assert.AreEqual(fileExtension, extension);
            File.Delete(_filename);
        }
              

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_WithNoFileName_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            var result = await _orderFileProcessor.WriteLineAsync($"1,1,10,{omsName},BUY");
            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_WithNoSecurity_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _security = null;
            _mockMetadataRepository.Setup(m => m.GetSecurityAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_security));

            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"2,1,10,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_WithNoPortfolio_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _portfolio = null;
            _mockMetadataRepository.Setup(m => m.GetPortfolioAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_portfolio));

            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,2,10,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_WithNoNominal_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,2,,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_WithNoTransactionType_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,2,10,{omsName},");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_WithNoPortfolioCode_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _portfolio = new Portfolio(1, "");
            _mockMetadataRepository.Setup(m => m.GetPortfolioAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_portfolio));

            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]        
        public async Task WriteLine_WithNoISIN_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _security = new Security(1, "", "s1", "CUSIP0001");
            _mockMetadataRepository.Setup(m => m.GetSecurityAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_security));

            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("BBB")]
        public async Task WriteLine_WithNoCusip_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _security = new Security(1, "ISIN11111111", "s1", "");
            _mockMetadataRepository.Setup(m => m.GetSecurityAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_security));

            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("CCC")]
        public async Task WriteLine_WithTicker_Returns_False(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _security = new Security(1, "ISIN11111111", "", "CUSIP0001");
            _mockMetadataRepository.Setup(m => m.GetSecurityAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(_security));

            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},BUY");
            Assert.AreEqual(false, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA")]
        [TestCase("BBB")]
        [TestCase("CCC")]
        public async Task WriteLine_Returns_Result(string omsName)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},BUY");
            Assert.AreEqual(true, result);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA", "ISIN11111111,p1,10,BUY",1)]
        [TestCase("BBB", "CUSIP0001|p1|10|BUY",1)]
        [TestCase("CCC", "p1,s1,10,B",0)]
        public async Task WriteLine_BUY_Returns_Expected_Format(string omsName, string expectedFormat, int skip)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},BUY");
            var actualFormat = File.ReadAllLines(_filename).Skip(skip).First();
            Assert.AreEqual(expectedFormat, actualFormat);
            File.Delete(_filename);
        }

        [Test]
        [TestCase("AAA", "ISIN11111111,p1,10,SELL", 1)]
        [TestCase("BBB", "CUSIP0001|p1|10|SELL", 1)]
        [TestCase("CCC", "p1,s1,10,S", 0)]
        public async Task WriteLine_SELL_Returns_Expected_Format(string omsName, string expectedFormat, int skip)
        {
            SetOrderFileProcessor(omsName);
            _filename = await _orderFileProcessor.CreateFileAsync();
            var result = await _orderFileProcessor.WriteLineAsync( $"1,1,10,{omsName},SELL");
            var actualFormat = File.ReadAllLines(_filename).Skip(skip).First();
            Assert.AreEqual(expectedFormat, actualFormat);
            File.Delete(_filename);
        }
    }
}