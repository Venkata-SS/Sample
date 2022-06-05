
using FileProcessorConsole;

/// <summary>
/// Code to Cleanse the Output and Exception folder(Default settings).Please comment if not needed
/// </summary>
var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exception");
if (Directory.Exists(path))
{
    foreach (string fileName in Directory.GetFiles(path))
        File.Delete(fileName);
}
path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
if (Directory.Exists(path))
{
    foreach (string fileName in Directory.GetFiles(path))
        File.Delete(fileName);
}

/// <summary>
///  Buiding host for injection
/// </summary>

var consoleBuilder = new ConsoleBuilder();



/// <summary>
/// Processing File - Default output in TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\Output
/// Processing File - Default error in TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\Exception
/// Log File - D:\Practice\TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\TransactionProcessor.log
/// </summary>
var processor = consoleBuilder.GetProcessTransaction(args);
var result = await processor.ProcessFile("transactions.csv");


/* Uncomment the follwing to test file stream and comment the Process file logic */


/// <summary>
/// Processing Stream - Default output in TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\Output
/// Processing Stream - Default error in TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\Exception
/// Log File - D:\Practice\TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\TransactionProcessor.log
/// </summary>
//var processor1 = consoleBuilder.GetProcessTransaction(args);
//using (Stream fileStream = File.OpenRead("transactions.csv"))
//{
//    var result1 = await processor1.ProcessFileStream(fileStream);
//}


/// <summary>
/// Transaction with error
/// Processing File - Default output in TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\Output
/// Processing File - Default error in TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\Exception
/// Log File - D:\Practice\TransactionProcessor\FileProcessorConsole\bin\Debug\net6.0\TransactionProcessor.log
/// </summary>
//var processor = consoleBuilder.GetProcessTransaction(args);
//var result = await processor.ProcessFile("transactionswithErrors.csv");






