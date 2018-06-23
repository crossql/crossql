#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin Cake.XdtTransform

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var environment = Argument<string>("environment", "local");
var artifactsDir = "./artifacts";
var notLocal = !environment.Equals("local");

Setup(context =>
{
    var binsToClean = GetDirectories("./src/**/bin/");
    var testsToClean = GetDirectories("./test/**/bin/");
    var artifactsToClean = GetDirectories(artifactsDir);

    DotNetCoreRestore();
	CleanDirectories(binsToClean);
	CleanDirectories(testsToClean);
    CreateDirectory(artifactsDir);
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

Task("Build Tests")
    .IsDependentOn("XDT Transform")
    .Does(() => {
    MSBuild("./tests/crossql.tests/crossql.tests.csproj", settings =>
        settings.SetConfiguration(configuration));
});

Task("XDT Transform")
    .WithCriteria(notLocal)
    .Does(() => {
        var directories = GetDirectories("./tests/**");
        foreach(var dir in directories)
        {
            var xslPath = dir + "/app." + environment + ".config";
            var xmlPath = dir + "/app.config";
            
            if(FileExists(xslPath))
            {
                var xsl = File(xslPath);
                var xml = File(xmlPath);
                Information("Transforming {0} into {1}", xsl, xml);
                XdtTransformConfig(xml,xsl, xml);
            } 
        }
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