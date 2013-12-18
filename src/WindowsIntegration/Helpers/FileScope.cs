using System;
using System.IO;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    public class FileScope : IDisposable
    {
        private readonly string _FileName;

        public FileScope(string fileName)
        {
            _FileName = fileName;
        }

        public string FileName
        {
            get { return _FileName; }
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
            }
            catch
            {

            }
        }
    }
}