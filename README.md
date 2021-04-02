# AutoReleaseTool   
[![Build status](https://ci.appveyor.com/api/projects/status/g809ivwcvb7896qy?svg=true)](https://ci.appveyor.com/project/OysteinBruin/autoreleasetool) [![The project has reached a stable, usable state and is being actively developed.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#active)
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

AutoReleaseTool takes care of all of the above steps and uses Squirrel.exe internally. 

With AutoReleaseTool used in a complete CI/CD pipeline configuration, all it takes to create a new release for your desktop application and deploy it to its users is as simple as:

Push your changes to a defined github release branch - and github fires a webhook which kicks of the build process in an [appveyor](https://www.appveyor.com/) WM. 
See the complete example [below](#complete-example-of-a-CI/CD-setup).

<a name="manually"/>

## How to use AutoReleaseTool manually
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
    string urlOrPath = @"path/to/your/hosting/provider"; // See Azure Storage Account below
    
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




#### Appveyor
Appveyor is the CI/CD service that builds and deploys a new release. 
When you commit code and merge it to the defined release branch and push to GitHub, a webhook for AppVeyor is triggered to kick off the continuous integration build. Appveyor starts a new VM for your project and run the steps defined in the build configuration defined in the appveyor.yml [Register an account](https://www.appveyor.com/) and read more about configuring AppVeyor to work with GitHub [here](https://www.appveyor.com/docs/).

'appveyor.yml' configuration for the [AutoDeployedWpfDemo]()

```
#---------------------------------#
#      general configuration      #
#---------------------------------#

version: 1.0.{build}

branches:
  only:
   - master
 

#---------------------------------#
#    test configuration           #
#---------------------------------#
test: off
  # only assemblies to test
 # assemblies:
 #   only:
        

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
 # - ps: Push-AppveyorArtifact ./AutoReleaseTool/Tools/Update.exe

  
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

----  Work in progress, coming soon .. ---
#### Cake (C# Make)

### 2. Create a new release

