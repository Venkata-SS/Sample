using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.Repository.Interfaces.Models
{
    public record Security(int SecurityId,string ISIN, string Ticker , string Cusip);

    public record Portfolio(int PortfolioId, string PortfolioCode);
}
