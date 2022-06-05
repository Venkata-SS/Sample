using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Interfaces;
using TransactionProcessor.Repository.Interfaces.Models;

namespace FileProcessor.Impl
{
    public class AAAOrderFileProcessor : OrderFileProcessor
    {
        public AAAOrderFileProcessor(IMetadataRepository metadataRepository, IConfiguration configuration) 
                :base(metadataRepository, configuration)
        {
           
        }
        public override string GetOMSName()
        {
            return "AAA";
        }

        protected override string ComputeOrderLine(string line, Security? security, Portfolio? portfolio)
        {
            if (!string.IsNullOrWhiteSpace(line) &&
                _omsSetting != null &&
                !string.IsNullOrWhiteSpace(_omsSetting.Delimiter))
            {
                var nominal = Helper.GetColumnValue(_appSettings.FileSettings, line, "Nominal");
                var transactioType = Helper.GetColumnValue(_appSettings.FileSettings, line, "TransactionType");
                if (!string.IsNullOrWhiteSpace(nominal) && 
                    !string.IsNullOrWhiteSpace(transactioType) &&
                    !string.IsNullOrWhiteSpace(portfolio?.PortfolioCode) &&
                    !string.IsNullOrWhiteSpace(security?.ISIN))
                {
                     return $"{security.ISIN}{_omsSetting.Delimiter}{portfolio.PortfolioCode}{_omsSetting.Delimiter}{nominal}{_omsSetting.Delimiter}{transactioType}";
                }
            }
            return string.Empty;
        }
       
    }
}

