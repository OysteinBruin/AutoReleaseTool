
// ARGUMENTS

var target = Argument("target", "Default");
var solutionPath = Argument<string>("solutionPath");

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

Task("Restore-NuGet-Packages")
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


// EXECUTION
Task("Default").IsDependentOn("Build");

RunTarget(target);
