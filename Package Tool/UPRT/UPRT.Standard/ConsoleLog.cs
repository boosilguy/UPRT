using System.Diagnostics;

namespace UPRT.Standard
{
    public class ConsoleLog
    {
        static TextWriter output;

        public static void Create(TextWriter targetLog)
        {
            ConsoleLog.output = targetLog;
        }

        public static void Log(string message)
        {
            output?.WriteLine(message);
            Debug.WriteLine(message);
        }
    }
}