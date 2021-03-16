using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;


/*
https://github.com/Squirrel/Squirrel.Windows/blob/c86d3d0f19418d9f31d244f9c1d96d25a9c0dfb6/src/Update/Program.cs
        "Options:",
        { "h|?|help", "Display Help and exit", _ => {} },
        { "r=|releaseDir=", "Path to a release directory to use with releasify", v => releaseDir = v},
        { "p=|packagesDir=", "Path to the NuGet Packages directory for C# apps", v => packagesDir = v},
        { "bootstrapperExe=", "Path to the Setup.exe to use as a template", v => bootstrapperExe = v},
        { "g=|loadingGif=", "Path to an animated GIF to be displayed during installation", v => backgroundGif = v},
        { "i=|icon", "Path to an ICO file that will be used for icon shortcuts", v => icon = v},
        { "setupIcon=", "Path to an ICO file that will be used for the Setup executable's icon", v => setupIcon = v},
        { "n=|signWithParams=", "Sign the installer via SignTool.exe with the parameters given", v => signingParameters = v},
        { "s|silent", "Silent install", _ => silentInstall = true},
        { "b=|baseUrl=", "Provides a base URL to prefix the RELEASES file packages with", v => baseUrl = v, true},
        { "a=|process-start-args=", "Arguments that will be used when starting executable", v => processStartArgs = v, true},
        { "l=|shortcut-locations=", "Comma-separated string of shortcut locations, e.g. 'Desktop,StartMenu'", v => shortcutArgs = v},
        { "no-msi", "Don't generate an MSI package", v => noMsi = true},
*/


namespace AutoReleaseTool
{
    public class ReleaseCreator
    {
        private INugetPackageCreator _nugetPackageCreator;
        public ReleaseCreator(INugetPackageCreator nugetPackageCreator)
        {
            _nugetPackageCreator = nugetPackageCreator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Empty string on success, else an error message with the error code</returns>
        public string Execute()
        {
            // Create Nuget Package from package treeview.
            string nugetPackagePath = _nugetPackageCreator.CreatePackage();
            if (nugetPackagePath?.Length == 0)
            {
                return "Failed to create nuget package";
            }

            DirectoryInfo directoryInfo = Directory.CreateDirectory("./releases/");
            // Releasify
            int result = SquirrelReleasify(nugetPackagePath, directoryInfo.FullName);

            if (result != 0)
            {
                return $"Failed to create release, error code {result}";
            }

            return String.Empty;
        }

        /// <summary>
        /// Creates the the release files using Squirrel tools.
        /// </summary>
        /// <param name="nugetPackagePath">Absolute path to the nuget package</param>
        /// <param name="releaseOutputPath">Absolute path to where the release files will be placed</param>
        /// <returns>The release process exitcode: 0 indicates success, any other values indicates failure.</returns>
        private int SquirrelReleasify(string nugetPackagePath, string releaseOutputPath)
        {
            int exitCode = -1;
            
            var cmd = $@" -releasify {nugetPackagePath} -releaseDir {releaseOutputPath} -l 'Desktop'";
            string appExeDirAbsPath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            string fileName = appExeDirAbsPath + @"\Tools\Squirrel.exe";

            Console.WriteLine("\n--- Creating release ---");
            Console.WriteLine($"Cmd input:\n\t{cmd}");
            Console.WriteLine($"\nRunning process: {fileName}");


            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = fileName,
                Arguments = cmd
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit(90000);
                exitCode = process.ExitCode;
            }
            return exitCode;
        }
    }
}
