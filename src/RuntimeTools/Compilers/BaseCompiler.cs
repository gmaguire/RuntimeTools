using System.IO;

namespace HardcoreDebugging.Compilers
{
    public abstract class BaseCompiler
    {
        protected static string ReadCodeFile(string filename)
        {
            using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(filestream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}