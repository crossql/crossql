#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var local = BuildSystem.IsLocalBuild;
var environment = Argument<string>("environment");
var versionParam = Argument<string>("buildVersion");
var buildNumber = Argument<string>("buildNumber", "0");
var versionParts = versionParam.Split('.');
var artifactsDir = "./artifacts";

var version = string.Format("{0}.{1}.{2}.{3}", versionParts[0],versionParts[1],versionParts[2], buildNumber);

Setup(context =>
{
    var binsToClean = GetDirectories("./src/**/bin/");
    var testsToClean = GetDirectories("./test/**/bin/");
    var artifactsToClean = GetDirectories(artifactsDir);

	CleanDirectories(binsToClean);
	CleanDirectories(testsToClean);
    CreateDirectory(artifactsDir);
    //Executed BEFORE the first task.
    Information("Building version {0}", version);
});

Task("Default").IsDependentOn("Run-Unit-Tests");

Task("Build Sql Server").Does(() => {
      MSBuild("./src/crossql.mssqlserver/crossql.mssqlserver.csproj", settings =>
        settings.SetConfiguration(configuration));
});

Task("Build Sqlite").Does(() => {
      MSBuild("./src/crossql.sqlite/crossql.sqlite.csproj", settings =>
        settings.SetConfiguration(configuration));
});

Task("Build Tests").Does(() => {
    MSBuild("./tests/crossql.tests/crossql.tests.csproj", settings =>
        settings.SetConfiguration(configuration));
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build Sql Server")
    .IsDependentOn("Build Sqlite")
    .IsDependentOn("Build Tests")
    .Does(() =>
{
    var resultsFile = artifactsDir + "/test-results.xml";

    NUnit3("./tests/**/bin/" + configuration + "/**/*.tests.dll", new NUnit3Settings {
        NoResults = false,
        Results = new[] { new NUnit3Result { FileName = resultsFile } },           
    });

    if(AppVeyor.IsRunningOnAppVeyor)
    {
        AppVeyor.UploadTestResults(resultsFile, AppVeyorTestResultsType.NUnit3);
    }
});

RunTarget(target);