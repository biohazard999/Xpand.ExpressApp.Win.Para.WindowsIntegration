using System;
using System.Collections.Generic;
using System.Linq;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    public class FilesScope : IDisposable
    {
        private readonly List<FileScope> _scopes = new List<FileScope>();

        public FilesScope(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                _scopes.Add(new FileScope(file));
            }
        }

        public IEnumerable<string> FilesNames
        {
            get { return _scopes.Select(m => m.FileName); }
        }

        public void Dispose()
        {
            foreach (var fileScope in _scopes)
            {
                fileScope.Dispose();
            }
        }
    }
}