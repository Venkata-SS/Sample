using FileProcessor.Interfaces;
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
    public abstract class OrderFileProcessor : IOrderFileProcessor
    {
        protected readonly IMetadataRepository _metadataRepository;
        protected readonly AppSettings _appSettings;
        protected readonly OmsSetting? _omsSetting;
        protected string orderFilename;

        protected string OrderFilename { get; init; }

        protected OrderFileProcessor(IMetadataRepository metadataRepository, IConfiguration configuration)
        {
            this._metadataRepository = metadataRepository;
            this._appSettings = configuration?.GetSection("AppSettings")?.Get<AppSettings>();

            if (this._appSettings?.OmsSettings?.Count() > 0)
                _omsSetting = this._appSettings.OmsSettings.FirstOrDefault(o => o.OmsName == GetOMSName());

        }

        private void SetFilename()
        {
            orderFilename = Helper.GetFileName(_omsSetting?.OutputFilePath, "Output", _omsSetting.FileExtension);           
        }

        public virtual async ValueTask<string> CreateFileAsync()
        {
            SetFilename();
            if (!string.IsNullOrWhiteSpace(orderFilename))
            {
                var result = await Helper.CreateFile(orderFilename, _omsSetting.FileHeader);
                if (result)
                    return orderFilename;
            }
            return string.Empty;
        }

        public abstract string GetOMSName();

        public virtual async ValueTask<bool> IsMandatoryConfigExistsAsync()
        {
            return await _metadataRepository.GetTotalSecuritiesAsync() > 0 &&
                   await _metadataRepository.GetTotalPortfoliosAsync() > 0 &&
                   !string.IsNullOrWhiteSpace(GetOMSName()) &&
                   !string.IsNullOrWhiteSpace(_omsSetting?.Delimiter) &&
                   !string.IsNullOrWhiteSpace(_omsSetting?.FileExtension);
        }

        public virtual async ValueTask<bool> WriteLineAsync(string line)
        {
            if (!string.IsNullOrWhiteSpace(this.orderFilename) && 
                !string.IsNullOrWhiteSpace(line) && 
                File.Exists(this.orderFilename))
            {
                var orderLine = await GetOrderLine(line);
                if (!string.IsNullOrWhiteSpace(orderLine))                    
                    return await Helper.WriteFile(this.orderFilename, orderLine);
            }
            return false;
        }

        protected virtual async ValueTask<string> GetOrderLine(string line)
        {
            if (!string.IsNullOrWhiteSpace(line) )
            {
                var securityId = Helper.ConvertToInt(Helper.GetColumnValue(_appSettings.FileSettings, line, "SecurityId"));
                var portfolioId = Helper.ConvertToInt(Helper.GetColumnValue(_appSettings.FileSettings, line, "PortfolioId"));              
                if (securityId > 0 && portfolioId > 0)
                {
                    var security = await this._metadataRepository.GetSecurityAsync(securityId);
                    var portfolio = await this._metadataRepository.GetPortfolioAsync(portfolioId);
                    return ComputeOrderLine(line, security, portfolio);
                }
            }
            return string.Empty;
        }

        protected abstract string ComputeOrderLine(string line, Security? security, Portfolio? portfolio);

        public string GetFileName()
        {
            return this.orderFilename;
        }
    }
}
