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
    public class CCCOrderFileProcessor : OrderFileProcessor
    {
        public CCCOrderFileProcessor(IMetadataRepository metadataRepository, IConfiguration configuration) : base(metadataRepository, configuration)
        {

        }
        public override string GetOMSName()
        {
            return "CCC";
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
                    !string.IsNullOrWhiteSpace(security?.Ticker))
                {

                    var finalTrasType = transactioType.Substring(0, 1);
                    return $"{portfolio.PortfolioCode}{_omsSetting.Delimiter}{security.Ticker}{_omsSetting.Delimiter}{nominal}{_omsSetting.Delimiter}{finalTrasType}";
                }
            }
            return string.Empty;
        }
    }
}
