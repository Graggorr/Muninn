using System.Text;

namespace Muninn
{
    /// <summary>
    /// Configuration for the server connection for <see cref="IMuninnClient"/>
    /// </summary>
    public class MuninnConfiguration
    {
        /// <summary>
        /// Name of the default encoding for values
        /// </summary>
        public string EncodingName { get; set; } = Encoding.UTF8.EncodingName;

        /// <summary>
        /// Host name of the muninn server
        /// </summary>
        public string HostName { get; set; } = string.Empty;
    }
}
