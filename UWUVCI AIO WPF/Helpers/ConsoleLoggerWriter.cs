using System;
using System.IO;
using System.Text;

namespace UWUVCI_AIO_WPF.Helpers
{
    public class ConsoleLoggerWriter : TextWriter
    {
        private TextWriter originalConsoleOut; // Keep track of the original console output

        public ConsoleLoggerWriter()
        {
            originalConsoleOut = Console.Out;  // Save the original Console output
        }

        public override Encoding Encoding => originalConsoleOut.Encoding;  // Maintain the same encoding

        public override void WriteLine(string message)
        {
            // Write to the original Console output
            originalConsoleOut.WriteLine(message);

            // Log the message using your Logger
            Logger.Log(message);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            string formattedMessage = string.Format(format, arg);

            // Write to the original Console output
            originalConsoleOut.WriteLine(formattedMessage);

            // Log the formatted message
            Logger.Log(formattedMessage);
        }
    }

}
