using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Impl;

namespace TransactionProcessor.Repository.Tests
{
    public class CsvMetadataRepositoryTests
    {
        private CsvContext csvContext;
        private CsvMetadataRepository repository;
        [SetUp]
        public void Setup()
        {
            csvContext = new CsvContext();
            repository = new CsvMetadataRepository(csvContext);
        }

        [Test]
        public void InvalidSecuritiesFile_throwFileNotFoundException()
        {
            var exception = Assert.Throws<FileNotFoundException>( () => { csvContext = new CsvContext("test.csv", "test1.csv"); } );
            Assert.IsTrue(exception.Message.Contains("Unable to find file from path"));
        }

        [Test]
        public void InvalidPortfolioFile_throwFileNotFoundException()
        {
            var exception = Assert.Throws<FileNotFoundException>(() => { csvContext = new CsvContext("securities.csv", "test1.csv"); });
            Assert.IsTrue(exception.Message.Contains("Unable to find file from path"));
        }

        [Test]
        public void SecuritiesFile_throwParseException()
        {
            var exception = Assert.Throws<Exception>(() => { csvContext = new CsvContext("ErrorSecurities1.csv", "test1.csv"); });
            Assert.IsTrue(exception.Message.Contains("Unable to parse securityId from"));
        }

        [Test]
        public void SecuritiesFIle_throwInvalidFileException()
        {
            var exception = Assert.Throws<Exception>(() => { csvContext = new CsvContext("ErrorSecurities2.csv", "test1.csv"); });
            Assert.AreEqual("Invalid securities csv file",exception.Message);
        }

        [Test]
        public void PortfolioFile_throwParseException()
        {
            var exception = Assert.Throws<Exception>(() => { csvContext = new CsvContext("securities.csv","ErrorPortfolio1.csv"); });
            Assert.IsTrue(exception.Message.Contains("Unable to parse portfolioId from"));
        }

        [Test]
        public void PortfolioFIle_throwInvalidFileException()
        {
            var exception = Assert.Throws<Exception>(() => { csvContext = new CsvContext("securities.csv","ErrorPortfolio2.csv"); });
            Assert.AreEqual("Invalid portfolio csv file", exception.Message);
        }

        [Test]
        public async Task SecuritiesFile_returnsData()
        {
            var result = await repository.GetSecurityAsync(1);

            Assert.AreEqual(1, result.SecurityId);
            Assert.AreEqual("ISIN11111111", result.ISIN);
            Assert.AreEqual("s1", result.Ticker);
            Assert.AreEqual("CUSIP0001", result.Cusip);
        }

        [Test]
        public async Task PortfolioFile_returnsData()
        {
            var result = await repository.GetPortfolioAsync(1);

            Assert.AreEqual(1, result.PortfolioId);
            Assert.AreEqual("p1", result.PortfolioCode);
        }

        [Test]
        public async Task GetTotalSecurities_returnsData()
        {
            var result = await repository.GetTotalSecuritiesAsync();
            Assert.IsTrue(result > 0 );
        }

        [Test]
        public async Task GetTotalPortfolio_returnsData()
        {
            var result = await repository.GetTotalPortfoliosAsync();
            Assert.IsTrue(result > 0);
        }
    }
}