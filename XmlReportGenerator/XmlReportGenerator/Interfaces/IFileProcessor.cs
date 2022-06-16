using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlReportGenerator.Interfaces
{
    public interface IFileProcessor
    {
        /// <summary>
        /// Uses the Polling mechanism to get files from Directory based on the app settings
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        Task ProcessXmlFilesAsync(CancellationTokenSource cancellationTokenSource);

        ValueTask<bool> ProcessXmlFilesAsync(string[] filenames,CancellationTokenSource cancellationTokenSource);
    }
}
