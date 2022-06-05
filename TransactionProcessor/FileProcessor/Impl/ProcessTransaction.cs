using FileProcessor.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Impl
{
    public class ProcessTransaction : IProcessTransaction
    {
        private readonly IEnumerable<IOrderFileProcessor> _orderFileProcessors;
        protected readonly AppSettings? _appSettings;
        private readonly ILogger<ProcessTransaction> _logger;        

        public ProcessTransaction(IEnumerable<IOrderFileProcessor> orderFileProcessors, 
                                  IConfiguration configuration ,
                                  ILogger<ProcessTransaction> logger)
        {
            this._orderFileProcessors = orderFileProcessors;
            this._appSettings = configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            this._logger = logger;            
        }

        public async ValueTask<bool> ProcessFile(string fileWithPath)
        {
            _logger.LogInformation($"Started Processing file {fileWithPath}");
            try
            {
                if (string.IsNullOrWhiteSpace(fileWithPath))
                {
                    _logger.LogError("fileWithPath is empty");
                    return false;
                }
                using (Stream fileStream = File.OpenRead(fileWithPath))
                    return await ProcessFileStream(fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file");
                return false;
            }
            finally
            {
                _logger.LogInformation($"Ended Processing file {fileWithPath}");
            }
            
        }

        public async ValueTask<bool> ProcessFileStream(Stream fileStream)
        {
            _logger.LogInformation("Started Processing Stream...");
            var result = false;
            try
            {
                if (Validate(fileStream))
                {
                    result = await ProcessFile(fileStream);
                    
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file");
                return false;
            }
            finally
            {
                _logger.LogInformation("Ended Processing Stream...");
            }
        }


        private async ValueTask<bool> ProcessFile(Stream fileStream)
        {
            try
            {
                if (fileStream != null)
                {
                    bool isfirstLine = true;
                    var exceptionFilename = string.Empty;
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        while (streamReader.Peek() > 0)
                        {
                            if (_appSettings.IsFirtsLineHeading && isfirstLine)
                            {
                                isfirstLine = false;
                                await streamReader.ReadLineAsync();
                                continue;
                            }
                            var line = await streamReader.ReadLineAsync();
                            exceptionFilename = await ProcessLine(line, exceptionFilename);
                        }
                    }
                    fileStream.Dispose();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file");
                fileStream.Dispose();
                return false;
            }
        }

        private bool IsOmsSUpported(string oms)
        {
            if(!string.IsNullOrWhiteSpace(oms) &&
               !string.IsNullOrWhiteSpace(this._appSettings?.ValidOmsProcessor))
            {
                var result = this._appSettings?.ValidOmsProcessor.Contains(oms);
                return result.GetValueOrDefault();
            }
            return false;
        }

        private async ValueTask<string> MoveToException(string exceptionFilename ,string line)
        {
            var filename = exceptionFilename;
            //Creat for the fisrt time
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = Helper.GetFileName(_appSettings.ExceptionFilePath, "Exception", ".csv");
                var header = string.Empty;
                if (_appSettings.IsFirtsLineHeading)
                {
                    var headerArray = _appSettings.FileSettings.OrderBy(f => f.ColumnOrder).Select(f => f.ColumnName).ToArray();
                    if( headerArray != null && headerArray.Length > 0 )
                    {
                        header = String.Join(',', headerArray);
                    }
                }
                var result = await Helper.CreateFile(filename, header);
            }
            await Helper.WriteFile(filename, line);
            return filename;
        }

        private async ValueTask<string> ProcessLine(string line, string exceptionFilename)
        {
            if( !string.IsNullOrWhiteSpace(line) )
            {
                try
                {
                    //Validate
                    if (!ValidateLine(line))
                    {
                        exceptionFilename = await MoveToException(exceptionFilename, line);
                        return await ValueTask.FromResult(exceptionFilename);
                    }
                    //Get oms
                    var oms = Helper.GetColumnValue(_appSettings.FileSettings, line, "OMS");
                    if (!IsOmsSUpported(oms))
                    {
                        _logger.LogError($"Unsuppoted OMS : {oms}");
                        exceptionFilename = await MoveToException(exceptionFilename, line);
                        return await ValueTask.FromResult(exceptionFilename);
                    }
                    //Get Order File Processor
                    var orderFileProcessor = GetOrderFileProcessor(oms);
                    if (orderFileProcessor == null)
                    {
                        _logger.LogError($"Unable to get processor for OMS : { oms}");
                        exceptionFilename = await MoveToException(exceptionFilename, line);
                        return await ValueTask.FromResult(exceptionFilename);
                    }

                    if (!await orderFileProcessor.IsMandatoryConfigExistsAsync())
                    {
                        _logger.LogError($"Necessary configuration or metadata is missing for OMS : {oms}");
                        exceptionFilename = await MoveToException(exceptionFilename, line);
                        return await ValueTask.FromResult(exceptionFilename);
                    }

                    //Create file if empty
                    if (string.IsNullOrWhiteSpace(orderFileProcessor.GetFileName()))
                        await orderFileProcessor.CreateFileAsync();

                    //Write to file
                    if (!await orderFileProcessor.WriteLineAsync(line))
                    {
                        _logger.LogError($"Unable to create order for OMS : {oms} and line : {line}");
                        exceptionFilename = await MoveToException(exceptionFilename, line);
                        return await ValueTask.FromResult(exceptionFilename);
                    }
                }                 
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error processing Line : { line}");
                    exceptionFilename = await MoveToException(exceptionFilename, line);
                    return await ValueTask.FromResult(exceptionFilename);
                }
            }
            return await ValueTask.FromResult(exceptionFilename);
        }

        private IOrderFileProcessor? GetOrderFileProcessor(string oms)
        {
            return this._orderFileProcessors.Where(f => f.GetOMSName() == oms).FirstOrDefault();
        }

        private bool Validate(Stream fileStream)
        {
            if (fileStream == null || fileStream?.Length == 0)
            {
                _logger.LogError("File or Stream is empty");
                return false;
            }
            else if (this._appSettings == null)
            {
                _logger.LogError("Unable to load config");
                return false;
            }
            else if (this._orderFileProcessors == null)
            {
                _logger.LogError("Unable to load Order file processors");
                return false;
            }            
            else if (string.IsNullOrWhiteSpace(this._appSettings.ValidOmsProcessor))
            {
                _logger.LogError("ValidOmsProcessor is missing in the config");
                return false;
            }
            return true;
        }

        private bool ValidateLine(string line)
        {
            if (!line.Contains(","))
            {
                _logger.LogError($"Line is not in csv format : {line}");
                return false;
            }
            var count = _appSettings.FileSettings.Where(f => f.IsMandatory).Count();
            if (line.Split(',').Count() != count)
            {
                _logger.LogError($"Line doesn't have necessary columns: {line}");
                return false;
            }

            if (IsMandatoryColumn("SecurityId"))
            {
                var securityId = Helper.GetColumnValue(_appSettings.FileSettings, line, "SecurityId");
                if (string.IsNullOrWhiteSpace(securityId))
                {
                    _logger.LogError($"Line doesn't have securityId: {line}");
                    return false;
                }
            }
            if (IsMandatoryColumn("PortfolioId"))
            { 
                var portfolioId = Helper.GetColumnValue(_appSettings.FileSettings, line, "PortfolioId");
                if (string.IsNullOrWhiteSpace(portfolioId))
                {
                    _logger.LogError($"Line doesn't have portfolioId: {line}");
                    return false;
                }
            }
            if (IsMandatoryColumn("Nominal"))
            {
                var nominal = Helper.GetColumnValue(_appSettings.FileSettings, line, "Nominal");
                if (string.IsNullOrWhiteSpace(nominal))
                {
                    _logger.LogError($"Line doesn't have nominal: {line}");
                    return false;
                }
            }

            if (IsMandatoryColumn("OMS"))
            {
                var oms = Helper.GetColumnValue(_appSettings.FileSettings, line, "OMS");
                if (string.IsNullOrWhiteSpace(oms))
                {
                    _logger.LogError($"Line doesn't have oms: {line}");
                    return false;
                }
            }

            if (IsMandatoryColumn("TransactionType"))
            {
                var transactionType = Helper.GetColumnValue(_appSettings.FileSettings, line, "TransactionType");
                if (string.IsNullOrWhiteSpace(transactionType))
                {
                    _logger.LogError($"Line doesn't have TransactionType: {line}");
                    return false;
                }
                else if (!_appSettings.ValidTransactionType.Contains(transactionType))
                {
                    _logger.LogError($"Line doesn't have correct TransactionType: {line}");
                    return false;
                }
            }
            return true;
        }

        private bool IsMandatoryColumn(string column)
        {
            if (string.IsNullOrWhiteSpace(column))
                return false;

            var filesetting = _appSettings.FileSettings.FirstOrDefault(s => s.ColumnName == column);
            return filesetting  != null && 
                   filesetting.IsMandatory ;
        }
    }
}
