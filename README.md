# AutoReleaseTool   
[![Build status](https://ci.appveyor.com/api/projects/status/g809ivwcvb7896qy?svg=true)](https://ci.appveyor.com/project/OysteinBruin/autoreleasetool)
[![The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#inactive)
[![NuGet](https://img.shields.io/nuget/v/AutoReleaseTool.svg?label=NuGet&style=flat)](https://www.nuget.org/packages/AutoReleaseTool/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AutoReleaseTool.svg)](https://www.nuget.org/packages/AutoReleaseTool/)
<br/>

AutoReleaseTool assists in removing the manual process of updating and deploying new releases for desktop applications using [Squirrel - An installation and update framework for Windows desktop apps](https://github.com/Squirrel/Squirrel.Windows) 


##### Table of Contents  
[What problem does it solve](#problem)  
[How to use AutoReleaseTool manually](#manually)  
[Complete example of a CI/CD setup](#example)  

<a name="problem"/>

## What problem does it solve
Squirrel is a great tool to provide installers and manage updates for Windows desktop applications,
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

With AutoReleaseTool configured in a CI/CD pipeline, all of the above steps is automated. 

All it takes to create a new release for your desktop application and deploy it to its users is as simple as:

Push the changes to a defined github release branch - and github fires a webhook which kicks of the build process in an [appveyor](https://www.appveyor.com/) Virtual Machine. 
See the complete example below - [Complete example of a CI/CD setup](#example).

<a name="manually"/>

## How to use AutoReleaseTool manually
AutoReleaseTool is made to be used as part of a complete CI/CD pipeline, see [Complete example of a CI/CD setup](#example) below, but it can be used standalone to create updated nuget package and release files. Run AutoReleaseTool.exe with 3 required arguments:
- app build directory - `"path/to/release/build/directory"`
- app id(name off application) - `"MyAppName"`
- version number - `"1.0.0"`

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
See example project [AutoDeployedWpfDemo's build.cake file](https://github.com/OysteinBruin/AutoDeployedWpfDemo/blob/master/build.cake)

<a name="example"/>

## Complete example of a CI/CD setup 

The process of a setting of a fully automated CI/CD pipeline requires use of several tools and technologies. 
I have an example project repo, [AutoDeployedWpfDemo](https://github.com/OysteinBruin/AutoDeployedWpfDemo), which demonstrates this using:

- Squirrel - the installation and updating framework
- Azure Storage Account - for hosting the release files, required CI/CD tools etc
- Appveyor - the CI/CD service
- Git and Github - version control and remote
- Cake - build automation system
- PowerShell - for running the cake file

There are several providers of the different services that can be used, e.g: [Azure storage](https://azure.microsoft.com/nb-no/services/storage/) could be replaced with [Amazon S3](https://aws.amazon.com/s3/) and [Appveyor](https://www.appveyor.com/) could probably be replaced with [TeamCity](https://www.jetbrains.com/teamcity/).

### 1. Setup

#### Squirrel
As mentioned above, Squirrel lets you create the installer file and manages the nuget package files for a new version.

Add Squirrel as a reference to your application 

![](https://github.com/OysteinBruin/AutoReleaseTool/blob/master/doc/images/squirrel_nuget.png?raw=true)

Add an CheckForUpdates() method in startup or mainwindow class in your application:


```csharp
private async Task CheckForUpdates()
{
    string urlOrPath = @"path/to/release/files/"; // See Azure Storage Account below
    
    using (var manager = new UpdateManager(urlOrPath))
    {
        await manager.UpdateApp();
    }
}
```
Examples from some of my other projects:
- [AutoDeployedWpfDemo - MainWindow codebehind](https://github.com/OysteinBruin/AutoDeployedWpfDemo/blob/master/src/AutoDeployedWpfDemo/MainWindow.xaml.cs)
- [PlcCom - ShellViewModel](https://github.com/OysteinBruin/PlcCom/blob/main/PlcCom/PlcComUI/ViewModels/ShellViewModel.cs)

You can find more in depth documentation here: [github.com/Squirrel/Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows/blob/develop/docs/getting-started/1-integrating.md)


#### Azure Storage Account
The application installer and updates need to be deploy to an online storage account. This example is using Azure Sorage Account

See Microsoft documentation for how to create an Azure Storage account and create a container for your files: [Create a storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal)

Screen shots from the storage account for the example project
Storage accounts overview:

![](https://github.com/OysteinBruin/AutoReleaseTool/blob/master/doc/images/azure_storage_accounts.png?raw=true)

Create a container inside the storage account for the relase files. The container for my demo app is named releases:
![](https://github.com/OysteinBruin/AutoReleaseTool/blob/master/doc/images/azure_storage_container.png?raw=true)

The link to be used in the CheckForUpdates() method can be found in the properties page:
![](https://github.com/OysteinBruin/AutoReleaseTool/blob/master/doc/images/azure_storage_properties.png?raw=true)




#### Appveyor, Cake, git and Github
Appveyor is the CI/CD service that runs the build and deployment of a new release, depending on what it is configured to do.
When you commit code and merge it to the defined release branch and push to GitHub, a webhook for AppVeyor is triggered to kick off the continuous integration build. Appveyor starts a new VM an copies the source files from github, and run the steps defined in the build configuration (appveyor.yml and build.ps1/build.cake files). 
[Register an account](https://www.appveyor.com/) at appveyor.com, and read more about configuring AppVeyor to work with GitHub [here](https://www.appveyor.com/docs/).

The 'appveyor.yml' and build.cake config files is located in the root of the source code for [AutoDeployedWpfDemo](https://github.com/OysteinBruin/AutoDeployedWpfDemo/blob/master/appveyor.yml):

_appveyor.yml setup for this example_
  - general configuration:
      - set version path nr to current build 
      - only run pipeline if the commit is on the master branch.
  - environment configuration: 
      - increase the version number and update it in AssemblyInfo.cs
  -build configuration:
      - build.ps1 starts build.cake with the required arguments for running build and various tasks:
          - Addin directive used to get AutoReleaseTool from nuget.org
          - Build application - Nuget Restore and MSBuild 
          - Download previous release files from Azure storage account (if exists), to let Squirrel create a delta release
          - Create "releases" folder and finally run AutoReleaseTool to create the release files in the release folder
  - after build: list all files in releases folder as artifacts - more info here: [packaging-artifacts](https://www.appveyor.com/docs/packaging-artifacts/)
  - deploy: upload all artifacts to the Azure storage account container - more info here: [Deploying to Azure blob storage](https://www.appveyor.com/docs/deployment/azure-blob/)

appveyor.yml :
```
#---------------------------------#
#      general configuration      #
#---------------------------------#

version: 1.0.{build}

branches:
  only:
   - master

#---------------------------------#
#    environment configuration    #
#---------------------------------#

clone_depth: 1

image: Visual Studio 2019

assembly_info:
  patch: true
  file: AssemblyInfo.*
  assembly_version: '{version}'
  assembly_file_version: '{version}'
  
#---------------------------------#
#       build configuration       #
#---------------------------------#

platform: Any CPU

configuration: Release

build_script:
  - ps: .\build.ps1 --solutionPath="./src/AutoDeployedWpfDemo.sln" --buildPath="./src/AutoDeployedWpfDemo/bin/Release" --appId="AutoDeployedWpfDemo" --appVersion=$env:APPVEYOR_BUILD_VERSION


#---------------------------------#
#      artifacts configuration    #
#---------------------------------#

after_build:
  - ps: Get-ChildItem .\releases | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
  
#---------------------------------#
#     deployment configuration    #
#---------------------------------#

deploy:
-  provider: AzureBlob
   storage_account_name: autodeployedwpfdemo
   storage_access_key:
     secure: a90Xur2gLr+DEtTH0U5Yp7QE2CSbBsmdOdmrI1KxuFt3Yfg0ga9Hd+tlnXsJjVdhlrKswQdiFtimjDyMFNjjTGMKHeOe4pCSp8ZuhYEHbzDwjP1p5qF2PczfuY6o3XJI
   container: releases
   remove_files: true

# 
# on_finish:
#  - ps: $blockRdp = $true; iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/appveyor/ci/master/scripts/enable-rdp.ps1'))    
```


Cake (C# Make) is a cross-platform build automation system with a C# DSL for tasks such as compiling code, copying files and folders, running unit tests, compressing files and building NuGet packages.

build.cake :
```
#addin nuget:?package=AutoReleaseTool&version=1.0.2

// ARGUMENTS

var target = Argument("target", "Default");
var solutionPath = Argument<string>("solutionPath");
var buildPath = Argument<string>("buildPath");
var appId  = Argument<string>("appId");
var appVersion = Argument<string>("appVersion");


///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

// BUILD

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory(buildPath);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solutionPath, settings =>
        settings.SetConfiguration("Release"));
});

// TEST - TODO: add test task here
Information("No Unit Tests are added yet.");

//
Task("DownloadPreviousReleaseFiles")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Downloading previous release files");
    
    FilePath azcopyPath = "./tools/Addins/AutoReleaseTool.1.0.2/lib/net45/Tools/azcopy.exe";
    StartProcess(azcopyPath, new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append("copy")
            .Append("https://autodeployedwpfdemo.blob.core.windows.net/releases/")
            .Append("./")
            .Append("--recursive")
    });
});

// PREPARE 

Task("Package")
    .IsDependentOn("DownloadPreviousReleaseFiles")
    .Does(() => 
{
    if (!DirectoryExists("./releases"))
    {
        CreateDirectory("./releases");
    }
    FilePath autoReleasePath = "./tools/Addins/AutoReleaseTool.1.0.2/lib/net45/AutoReleaseTool.exe";
    StartProcess(autoReleasePath, new ProcessSettings {
    Arguments = new ProcessArgumentBuilder()
        .Append(buildPath)
        .Append(appId)
        .Append(appVersion)
    });
});


// EXECUTION
Task("Default").IsDependentOn("Package");

RunTarget(target);
```

### 2. Create a new release   

Commit changes and push - thats it.

If you use a separate branch for development and production (or pull requests):
- commit all changes to yur dev branch
- ```git checkcout master```
- ```git merge dev```
- ```git push```

