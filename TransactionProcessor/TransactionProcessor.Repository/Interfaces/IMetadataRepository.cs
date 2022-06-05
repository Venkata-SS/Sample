using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Interfaces.Models;

namespace TransactionProcessor.Repository.Interfaces
{
    public interface IMetadataRepository
    {
        /// <summary>
        /// Get the Security 
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns></returns>
        ValueTask<Security?> GetSecurityAsync(int securityId);
        /// <summary>
        /// Get the Portfolio
        /// </summary>
        /// <param name="portfolioId"></param>
        /// <returns></returns>
        ValueTask<Portfolio?>  GetPortfolioAsync(int portfolioId);
        /// <summary>
        /// Get the Total Securities
        /// </summary>
        /// <returns></returns>
        ValueTask<int> GetTotalSecuritiesAsync();
        /// <summary>
        /// Get the total Portfolio
        /// </summary>
        /// <returns></returns>
        ValueTask<int> GetTotalPortfoliosAsync();

    }
}
