using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using DevExpress.ExpressApp.Utils;
using Microsoft.CSharp;
using Vestris.ResourceLib;
using Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.ResourceManagers
{
    public class RuntimeImageResourceManager
    {
        private readonly string _basePath;

        private readonly Dictionary<string, int> ImageIndexes = new Dictionary<string, int>();

        public RuntimeImageResourceManager(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
                basePath = Path.GetDirectoryName(typeof(TaskbarJumpListWindowController).Assembly.Location);

            _basePath = basePath;
        }

        public string AutomaticImageAssemblyName { get; set; }

        public string WriteImageResouces(IEnumerable<string> imageNames)
        {
            var assemblyPath = GenerateAssemblyPath();
            var compilerErrors = GenerateImageAssembly(assemblyPath);

            if (compilerErrors == null)
            {
                if (File.Exists(assemblyPath)) //Second instance running
                    return assemblyPath;
                return null;
            }


            var imageTempPaths = GenerateTempImages(imageNames, _basePath);

            using (new FilesScope(imageTempPaths.Select(m => m.Item1)))
            {
                uint c = 100;
                uint id = 1;

                foreach (var fileTuple in imageTempPaths)
                {
                    var rc = new IconDirectoryResource
                    {
                        Name = new ResourceId(c),
                        Language = GetCurrentLangId()
                    };

                    var iconFile = new IconFile(fileTuple.Item1);

                    foreach (var icon in iconFile.Icons)
                    {
                        rc.Icons.Add(new IconResource(icon, new ResourceId(id), rc.Language));
                    }

                    rc.SaveTo(assemblyPath);

                    ImageIndexes[fileTuple.Item2] = Convert.ToInt32(id) - 1;

                    c++;
                    id++;
                }
            }

            Assembly.Load(compilerErrors.CompiledAssembly.FullName);

            return compilerErrors.CompiledAssembly.Location;
        }

        private CompilerResults GenerateImageAssembly(string assemblyPath)
        {
            var codeProvider = new CSharpCodeProvider();
            var icc = codeProvider.CreateCompiler();
            var parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = assemblyPath;

            var results = icc.CompileAssemblyFromDom(parameters, new CodeCompileUnit());

            if (results.Errors.HasErrors)
                return null;

            return results;
        }

        private string GenerateAssemblyPath()
        {
            var assemblyPath = Path.Combine(_basePath, AutomaticImageAssemblyName);
            return assemblyPath;
        }

        private static List<Tuple<string, string>> GenerateTempImages(IEnumerable<string> imageNames, string basePath)
        {
            var imageTempPaths = new List<Tuple<string, string>>();

            foreach (var imageName in imageNames)
            {
                var info = ImageLoader.Instance.GetImageInfo(imageName);
                var infoLarge = ImageLoader.Instance.GetImageInfo(imageName);

                if (info.Image == null || infoLarge.Image == null)
                    continue;

                var writer = new IconFileWriter();

                writer.Images.Add(info.Image);
                writer.Images.Add(infoLarge.Image);

                var imagePath = Path.Combine(basePath, imageName + ".ico");

                writer.Save(imagePath);

                imageTempPaths.Add(Tuple.Create(imagePath, imageName));
            }
            return imageTempPaths;
        }

        public int GetImageIndex(string imageName)
        {
            int keyValue = -1;

            ImageIndexes.TryGetValue(imageName, out keyValue);

            return keyValue;
        }

        private ushort GetCurrentLangId()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            int pid = PRIMARYLANGID(currentCulture.LCID);
            int sid = SUBLANGID(currentCulture.LCID);
            return (ushort)MAKELANGID(pid, sid);
        }

        public static int MAKELANGID(int primary, int sub)
        {
            return (((ushort)sub) << 10) | ((ushort)primary);
        }

        public static int PRIMARYLANGID(int lcid)
        {
            return ((ushort)lcid) & 0x3ff;
        }

        public static int SUBLANGID(int lcid)
        {
            return ((ushort)lcid) >> 10;
        }

    }
}