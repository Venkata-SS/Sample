using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Interfaces;
using TransactionProcessor.Repository.Interfaces.Models;

namespace TransactionProcessor.Repository.Impl
{
    public class CsvMetadataRepository : IMetadataRepository
    {
        private readonly CsvContext _csvContext;

        public CsvMetadataRepository(CsvContext csvContext)
        {
            this._csvContext = csvContext;
        }


        public async ValueTask<Portfolio?> GetPortfolioAsync(int portfolioId)
        {
            if (portfolioId > 0 && this._csvContext.Portfolios?.Count > 0)
            {
                return await ValueTask.FromResult(this._csvContext.Portfolios.FirstOrDefault(s => s.PortfolioId == portfolioId));
            }
            return null;
        }

        public async ValueTask<Security?> GetSecurityAsync(int securityId)
        {
            if (securityId > 0 && this._csvContext.Securities?.Count > 0)
            {
                return await ValueTask.FromResult(this._csvContext.Securities.FirstOrDefault(s => s.SecurityId == securityId));
            }
            return null;
        }

        public async ValueTask<int> GetTotalPortfoliosAsync()
        {
            if (this._csvContext.Portfolios == null)
                return await ValueTask.FromResult(0);

            return await ValueTask.FromResult(this._csvContext.Portfolios.Count);
        }

        public async ValueTask<int> GetTotalSecuritiesAsync()
        {
            if(this._csvContext.Securities == null)
                return await ValueTask.FromResult(0);
            return await ValueTask.FromResult(this._csvContext.Securities.Count);
        }
    }
                
}
