using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Interfaces
{
    public interface IProcessTransaction
    {
        /// <summary>
        /// Process the filename specified with path
        /// </summary>
        /// <param name="fileWithPath"></param>
        ValueTask<bool> ProcessFile(string fileWithPath);

        /// <summary>
        /// Porcess the file stream
        /// </summary>
        /// <param name="fileStream"></param>
        ValueTask<bool> ProcessFileStream(Stream fileStream);
    }
}
