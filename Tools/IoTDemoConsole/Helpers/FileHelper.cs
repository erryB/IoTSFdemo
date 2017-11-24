using System.IO;
using System.Threading.Tasks;

namespace IoTDemoConsole.Helpers
{

    /// <summary>
    /// Class FileHelper.
    /// </summary>
    public static class FileHelper
    {

        /// <summary>
        /// read all text as an asynchronous operation.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public static async Task<string> ReadAllTextAsync(string fullFileName)
        {
            string result = null;
            using (var reader = File.OpenText(fullFileName))
            {
                result = await reader.ReadToEndAsync();
            }
            return result;
        }

        /// <summary>
        /// write all text as an asynchronous operation.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <param name="content">The content.</param>
        /// <returns>Task.</returns>
        public static async Task WriteAllTextAsync(string fullFileName,string content)
        {
            using (var reader = File.CreateText(fullFileName))
            {
                await reader.WriteAsync(content);
            }
        }
    }
}
