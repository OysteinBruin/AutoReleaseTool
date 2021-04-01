# AutoReleaseTool   
[![Build status](https://ci.appveyor.com/api/projects/status/g809ivwcvb7896qy?svg=true)](https://ci.appveyor.com/project/OysteinBruin/autoreleasetool) [![The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#active)
<br/>

AutoReleaseTool assists in removing the manual process of updating and deploying new releases for desktop applications using [Squirrel - An installation and update framework for Windows desktop apps](https://github.com/Squirrel/Squirrel.Windows) 


##### Table of Contents  
[What problem does it solve](#what)  
[How to use it manually](#how)  
[Complete example of a CI/CD setup](#complete)  
 
## What problem does it solve
Squirrel is a great tool to manage both installation and updates of Windows desktop applications,
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

AutoReleaseTool takes care of all of the above steps and uses Squirrel.exe internally. 

With AutoRelease used in a complete CI/CD pipeline configuration, all it takes to create a new release for your desktop application and deploy it to its users is as simple as:

Push your changes to a defined github release branch - and github fires a webhook which kicks of the build process in an [appveyor](https://www.appveyor.com/) WM. 
See the complete example [below](#complete-example-of-a-CI/CD-setup).

## How to use it manually
The step to create a new release with AutoReleaseTool manually:

- Run AutoReleaseTool.exe with 3 parameters 1. build directory, 2. app id(name off application), 3. version number :

 `path/to/AutoRelease.exe "path/to/release/build/directory" "MyApp" "1.0.0"`

Example usage in a cake file used as one of the steps to automate the whole relese process:

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

## Complete example of a CI/CD setup 

The process requires use of several tools and technologies:
- Azure Storage Account - for hosting the release files, required CI/CD tools etc
- Squirrel - the installation and updating framework
- Appveyor - the CI/CD service
- Git and Github - version control and remote
- Cake - build automation system
- PowerShell - for running the cake file

 ----  Work in progress, coming soon .. ---

#### Azure Storage Account

#### Squirrel installer and updater

#### appveyor.yml file

#### build.cake


