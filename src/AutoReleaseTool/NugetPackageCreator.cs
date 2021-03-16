using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NuGet.Packaging;
using NuGet.Versioning;

namespace AutoReleaseTool
{
    public class NugetPackageCreator : INugetPackageCreator
    {
        private string _buildDirPath;
        private string _appId;
        private string _version;
        private readonly string _packageDirectory = "./package/";
        private string _versionRegExp { get => @"\d+(?:\.\d+)+"; }


        public NugetPackageCreator(string buildDirPath, string appId, string version)
        {
            _buildDirPath = buildDirPath;
            _appId = appId;
            _version = version;
        }
        public string CreatePackage()
        {
            Console.WriteLine("\n--- Creating nuget package ---");
            var regex = new Regex(_versionRegExp, RegexOptions.IgnoreCase);

            if (_buildDirPath.Length == 0 || _appId.Length == 0 /*|| regex.IsMatch(_version)*/)
            {
                Console.WriteLine("Failed to create package");

                if (_buildDirPath.Length == 0)
                    Console.WriteLine("\tBuild directory path is empty.");

                if (_appId.Length == 0)
                    Console.WriteLine("\tApp Id is empty.");

                if (regex.IsMatch(_version))
                    Console.WriteLine("\tVersion is not valid.");

                return string.Empty;
            }

            IEnumerable<string> authors = new List<string> { "no_name" };

            var metadata = new ManifestMetadata()
            {
                Id = _appId,
                Authors = authors,
                Version = new NuGetVersion(_version),
                Description = $"{_appId}-{_version}",
                Title = $"{_appId}-{_version}"
            };

            var builder = new PackageBuilder();
            builder.Populate(metadata);

            //Squirrel convention: put everything in lib/net45 folder
            const string directoryBase = "/lib/net45";

            var files = new List<ManifestFile>();

            foreach (var file in GetFiles(_buildDirPath))
            {
                var manifest = new ManifestFile()
                {
                    Source = file
                };

                string newFilePath = file.Remove(0, _buildDirPath.Length);
                string target = directoryBase + "/" + newFilePath;

                manifest.Target = target;

                files.Add(manifest);
            }

            builder.PopulateFiles("", files.ToArray());

            DirectoryInfo directoryInfo = Directory.CreateDirectory(_packageDirectory);
            var nugetPath = $"{directoryInfo.FullName}{_appId}.{_version}.nupkg";

            using (FileStream stream = File.Open(nugetPath, FileMode.OpenOrCreate))
            {
                builder.Save(stream);
            }

            Console.WriteLine($"\nNuget package created: {nugetPath}");
            return nugetPath;
        }

        private List<string> GetFiles(string directoryPath, string discardedFileExt = ".pdb")
        {
            List<string> filePathList = new List<string>();

            foreach (var subdirectory in Directory.GetDirectories(directoryPath))
            {
                List<string> subDirFileList = GetFiles(subdirectory);
                foreach (var filePath in subDirFileList)
                {
                    if (!filePath.EndsWith(discardedFileExt))
                        filePathList.Add(filePath);
                }
            }

            foreach (var filePath in Directory.GetFiles(directoryPath))
            {
                if (!filePath.EndsWith(discardedFileExt))
                    filePathList.Add(filePath);
            }

            return filePathList;
        }
    }
}
