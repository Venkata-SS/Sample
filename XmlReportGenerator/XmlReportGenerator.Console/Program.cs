using XmlReportGenerator.Console;
using XmlReportGenerator.Interfaces;


try
{
    //Get the processor from Builder
    var consoleBuilder = new ConsoleBuilder();
    var processor = consoleBuilder.GetFileProcessor(args);
    
    //Execute the Program
    var cancellationToken = new CancellationTokenSource();
    await processor.ProcessXmlFilesAsync(cancellationToken);

    //To cancel the Event
    Console.CancelKeyPress += (s, e) =>
    {
        cancellationToken.Cancel();
    };
}
catch (OperationCanceledException ex)
{
    Console.WriteLine("User stopped the program ");
}
catch (Exception ex)
{
    Console.WriteLine("Exception starting the program " + ex.StackTrace);
}
