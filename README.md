# AutoReleaseTool   
[![Build status](https://ci.appveyor.com/api/projects/status/g809ivwcvb7896qy?svg=true)](https://ci.appveyor.com/project/OysteinBruin/autoreleasetool) [![The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#active)
<br/>

AutoReleaseTool assists in removing the manual process of integrating and deploying desktop using [Squirrel - An installation and update framework for Windows desktop apps](https://github.com/Squirrel/Squirrel.Windows)

## What problems is solves
Squirrel is a great tool to manage both installation and updating Windows desktop applications,
but the update process requires several manual steps:

1. Update AssemblyInfo.cs AssemblyVerions with new version. b
2. Switch to Release and build.
3. Open NuGet Package Explorer with the previous Nuget package
4. Update version
5. Replace release files
6. Save the new Nuget Package version
7. Run Package Manager Console in Visual Studio to create the new release
8. Copy the new release files to the defines release location
  Details here: [Squirrel Step 5. Updating](https://github.com/Squirrel/Squirrel.Windows/blob/develop/docs/getting-started/5-updating.md)

AutoReleaseTool takes care of all of the above steps and uses Squirrel.exe internally. The step to create a new relese with AutoReleaseTool:

1. Run AutoReleaseTool.exe with 3 parameters 1. build directory, 2. app id(name off application), 3.  :
   `path/to/AutoRelease.exe "path/to/release/build/directory" "MyApp" "1.0.0"'

Example from a cake file used to automate the whole process ()

```csharp
Task("Package")
    .IsDependentOn("DownloadPreviousReleaseFiles")
    .Does(() => 
{
    if (!DirectoryExists("./releases"))
    {
        CreateDirectory("./releases");
    }
    FilePath autoReleasePath = "./AutoReleaseTool/AutoReleaseTool.exe";
    StartProcess(autoReleasePath, new ProcessSettings {
    Arguments = new ProcessArgumentBuilder()
        .Append(buildPath)
        .Append(appId)
        .Append(appVersion)
    });
});
```


## CI/CD setup with Squirrel/AutoRelease, Github, Cake.build and Appveyor 

Work in progress, coming soon.
