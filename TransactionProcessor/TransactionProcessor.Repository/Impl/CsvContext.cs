using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Interfaces.Models;

namespace TransactionProcessor.Repository.Impl
{
    public class CsvContext
    {
        private const string SECURITIES_FILENAME = "securities.csv";
        private const string PORTFOLIO_FILENAME = "portfolios.csv";

        public CsvContext() :this (SECURITIES_FILENAME, PORTFOLIO_FILENAME)
        {
           
        }

        public CsvContext(string securitiesFilePath , string portfolioFilePath) 
        {
            Securities = GetMetaData<Security>(securitiesFilePath, GetSecurity);
            Portfolios = GetMetaData<Portfolio>(portfolioFilePath, GetPortfolio);
        }

        public List<Security> Securities { get; init; }

        public List<Portfolio> Portfolios { get; init; }

        private List<T> GetMetaData<T>(string filename, Func<string, T> processLine)
        {
            /*
             * This is ok only if the file is small.Otherwsie we have to think of better solution
            */
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, filename);
            if (File.Exists(filePath))
            {
                var file = File.ReadAllLines(filePath);
                if (file?.Length > 0)
                {
                    return file.Skip(1).Select(l => processLine(l)).ToList();
                }
                else
                {
                    return new List<T>();
                }
            }            
            else
            {
                throw new FileNotFoundException($"Unable to find file from path {filePath}");
            }
            
        }

        private Security GetSecurity(string line)
        {
            var values = line.Split(",");
            if (values?.Length > 0 && values?.Length == 4)
            {
                var securityId = 0;
                if (int.TryParse(values[0], out securityId))
                {
                    return new Security(securityId, values[1], values[2], values[3]);
                }
                else
                {
                    throw new Exception($"Unable to parse securityId from {SECURITIES_FILENAME}");
                }
            }
            else
            {
                throw new Exception($"Invalid securities csv file");
            }
        }

        private Portfolio GetPortfolio(string line)
        {
            var values = line.Split(",");
            if (values?.Length == 2)
            {
                var portfolioId = 0;
                if (int.TryParse(values[0], out portfolioId))
                {
                    return new Portfolio(portfolioId, values[1]);
                }
                else
                {
                    throw new Exception($"Unable to parse portfolioId from {portfolioId}");
                }
            }
            else
            {
                throw new Exception($"Invalid portfolio csv file");
            }
        }

    }
}
