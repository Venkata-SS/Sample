using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XmlReportGenerator.Helpers;
using XmlReportGenerator.Interfaces;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Impl
{
    public class FileProcessor : IFileProcessor
    {
        private readonly IEnumerable<IXmlParser> _xmlParsers;
        protected readonly AppSettings? _appSettings;
        private readonly ILogger<FileProcessor> _logger;
        private readonly IOutputGenerator _outputGenerator;

        public FileProcessor(IEnumerable<IXmlParser> xmlParsers , IConfiguration configuration, 
                             ILogger<FileProcessor> logger, IOutputGenerator outputGenerator)
        {
            _appSettings = configuration?.GetSection("AppSettings")?.Get<AppSettings>();
            _xmlParsers = xmlParsers;
            _logger = logger;
            _outputGenerator = outputGenerator;
        }

        public async Task ProcessXmlFilesAsync(CancellationTokenSource cancellationTokenSource)
        {
            Validate();

            while (true)
            {
                //Execute the main Program
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    //Get max 10 files at a time
                    var maxFiles = _appSettings.MaxFilesInSingleRead > 0 ? _appSettings.MaxFilesInSingleRead : 10;
                    var files = Directory.GetFiles(_appSettings.InputDirectory).Take(maxFiles).ToArray();

                    var result = await ProcessFile(files);
                    if (!result)
                    {
                        _logger.LogError("Exception Processing the files.");
                        break;
                    }
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation("User stopped the program");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError( ex, "Exception running the program ");
                    break;
                }
                //Short Pause
                await Task.Delay(5000);
            }
        }

        public async ValueTask<bool> ProcessXmlFilesAsync(string[] filenames, CancellationTokenSource cancellationTokenSource)
        {
            Validate();
            //Execute the main Program
            try
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                var result = await ProcessFile(filenames);
                if (!result)
                {
                    _logger.LogError("Exception Processing the files.");
                    return false;
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation("User stopped the program");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception running the program ");
                return false;
            }
            return true;
        }

        public async ValueTask<bool> ProcessFile(string[] filenames)
        {
            _logger.LogInformation("Started Processing files");
            try
            {
                if (filenames == null || filenames.Count() == 0)
                {
                    _logger.LogInformation("No files to Process");
                    return true;
                }
                else
                    return await ProcessXmlFiles(filenames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files");
                return false;
            }
            finally
            {
                _logger.LogInformation("Ended Processing file");
            }
        }

        private void Validate()
        {
            if(_xmlParsers == null)
                throw new Exception("XmlParsers are null");

            if (_appSettings == null)
                throw new Exception("AppSettings is null");

            if (_outputGenerator == null)
                throw new Exception("OutputGenerator is null");

            if (string.IsNullOrWhiteSpace(_appSettings.InputDirectory))
                throw new Exception("InputDirectory hasn't been configured");
            
            if(!Directory.Exists(_appSettings.InputDirectory))
                throw new Exception("InputDirectory doesn't exists");

            if (string.IsNullOrWhiteSpace(_appSettings.OutputDirectory))
                throw new Exception("OutputDirectory hasn't been configured");

            if (!Directory.Exists(_appSettings.OutputDirectory))
                throw new Exception("OutputDirectory doesn't exists");

            if (string.IsNullOrWhiteSpace(_appSettings.ProcessedDirectory))
                throw new Exception("ProcessedDirectory hasn't been configured");

            if (!Directory.Exists(_appSettings.ProcessedDirectory))
                throw new Exception("ProcessedDirectory doesn't exists");

            if (string.IsNullOrWhiteSpace(_appSettings.ExceptionDirectory))
                throw new Exception("ExceptionDirectory hasn't been configured");

            if (!Directory.Exists(_appSettings.ExceptionDirectory))
                throw new Exception("ExceptionDirectory doesn't exists");

            if (string.IsNullOrWhiteSpace(_appSettings.OutputFileAppender))
                throw new Exception("OutputFileAppender hasn't been configured");
        }

        private async ValueTask<bool> ProcessXmlFiles(string[] filenames)
        {

            foreach (var filename in filenames)
            {
                _logger.LogInformation($"Started Processing file : {filename}");
                var fileInfo = new FileInfo(filename);                
                //Reset the Collection
                _outputGenerator.Reset();
                
                try
                {
                    if (fileInfo.Extension != ".xml")
                        throw new NotSupportedException($"File extension not supported : {fileInfo.Extension }");

                    var result = await ProcessXmlFile(filename);
                    if (result)
                    {
                        if (_outputGenerator != null)
                        {
                            //Construct the output path
                            var outputfilePath = fileInfo.Name.Replace(fileInfo.Extension, String.Empty) + _appSettings.OutputFileAppender + ".xml";
                            var outputFile = Path.Combine(_appSettings.OutputDirectory, outputfilePath);
                            _outputGenerator.WriteToFile(outputFile);

                            //Move the Processed file                        
                            var processedFile = Path.Combine(_appSettings.ProcessedDirectory, fileInfo.Name);
                            File.Move(filename, processedFile, true);

                            //Reset the Collection
                            _outputGenerator.Reset();
                        }
                        else
                        {
                            _logger.LogError($"Unable to process file : {filename}");
                            //Move the  file to Exception folder
                            MoveToExceptionFolder(filename, fileInfo);
                        }
                    }
                    else
                    {
                        _logger.LogError($"Unable to process file : {filename}");
                        //Move the  file to Exception folder
                        MoveToExceptionFolder(filename, fileInfo);
                    }
                }
                catch (Exception ex)
                {
                    //Reset the Collection
                    _outputGenerator.Reset();

                    _logger.LogError(ex, $"Error processing file : {filename}");

                    //Move the  file to Exception folder
                    MoveToExceptionFolder(filename, fileInfo);
                }
                _logger.LogInformation($"Ended Processing file : {filename}");
            }
            return true;
        }

        private void MoveToExceptionFolder(string filename, FileInfo fileInfo)
        {
            var exceptionFile = Path.Combine(_appSettings.ExceptionDirectory, fileInfo.Name);
            File.Move(filename, exceptionFile, true);
        }

        private async ValueTask<bool> ProcessXmlFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException($"Unable to read file : {filename}");

            if ( !string.IsNullOrWhiteSpace(filename))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                bool isProcessed = false;

                using (Stream stream = File.OpenRead(filename))
                {
                    using (XmlReader reader = XmlReader.Create(stream, settings))
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                var parser = GetXmlParser(reader.Name);
                                if ( parser != null)
                                {
                                    if (parser is WindGeneratorParser)
                                    {
                                        var windGenerator = await parser.Parse<WindGenerator>(reader);
                                        await _outputGenerator.AddToOuput(windGenerator);
                                    }
                                    else if (parser is GasGeneratorParser)
                                    {
                                        var gasGenerator = await parser.Parse<GasGenerator>(reader);
                                        await _outputGenerator.AddToOuput(gasGenerator);
                                    }
                                    else if (parser is CoalGeneratorParser)
                                    {
                                        var coalGenerator = await parser.Parse<CoalGenerator>(reader);
                                        await _outputGenerator.AddToOuput(coalGenerator);
                                    }
                                    isProcessed = true;
                                }
                            }
                        }
                    }
                }
                return isProcessed;
            }
            return false;
        }

        private IXmlParser? GetXmlParser(string parserName)
        {
            return this._xmlParsers?.Where( x => x.GetParserName() == parserName).FirstOrDefault();
        }
    }
}
