using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Interfaces
{
    public interface IOrderFileProcessor
    {
        /// <summary>
        /// Get the Order management sysetm name.
        /// </summary>
        /// <returns></returns>
        string GetOMSName();
        /// <summary>
        /// Check whether all necessary metdata is availabe to start processing the file.
        /// </summary>
        /// <returns></returns>
        ValueTask<bool> IsMandatoryConfigExistsAsync();
        /// <summary>
        /// Create the new file and returens the file name
        /// </summary>
        /// <returns></returns>
        ValueTask<string> CreateFileAsync();
        /// <summary>
        /// Write(append) line to the  file name specified
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        ValueTask<bool> WriteLineAsync(string line);
        /// <summary>
        /// Get the File name 
        /// </summary>
        /// <returns></returns>
        string GetFileName();
    }
}
