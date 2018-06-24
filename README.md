crossql
============

A Portable, cross-platform, light weight, opinionated ORM designed to work across multiple Databases 

----

### Status ##

| Release | Pre-Release |
| _______ | :_________: |
|         | [![Build status](https://ci.appveyor.com/api/projects/status/25stvaknw7vrpjhc?svg=true)](https://ci.appveyor.com/project/ChaseFlorell/crossql) |

### Current Support ###

*note: we take pull requests if you'd like to support more ;)* 

| Platform      | Sqlite | SQL Server (2012) | PostgreSQL  | MySQL   |
| ------------- | -----: | ----------------: | ----------: | ------: |
| Windows       | yes    |  yes              | one day     | one day |
| Android       | yes    |  no               | no          | no      |
| iOS           | yes    |  no               | no          | no      |

### That sounds good, how do I use it? ###

<!--
We're working on building out the [Wiki](https://github.com/crossql/crossql/wiki), so more information will be found there. 

To get started, can either compile the project yourself. Simply clone the project to your Windows computer (we're currently using MSBuild and Powershell for our builds), and run the `build.ps1` script. From there you can either grab the dll's out of the `build-artifacts\output` directory, or scoop the nupkg out of the `build-artifacts` directory and drop it in your local nuget package source. 

Or you can just grab it from Nuget.org

    > Install-Package crossql

To give you a taste of what it looks like, here are some examples of CRUD operations.

-->

**Create**

    var jill = new StudentModel
        {
            Id = StudentJillId,
            FirstName = "Jill",
            LastName = "",
            Email = JillEmail,
            Courses = new List<CourseModel> { englishCourse },
        };

    _dbProvider.Create( jill );

**Read**

    IList<StudentModel> allStudents = _dbProvider.Query<StudentModel>().Select().ToList();
    StudentModel jill = _dbProvider.Query<StudentModel>().Where(s => s.Email == "JillEmail").Select().FirstOrDefault();

**Update**

    jill.Email = "JillNewEmail";
    _dbProvider.Update<StudentModel>(jill);

**Delete**  
*note: this is still a work in progress*

    // first flavor
    _dbProvider.Query<StudentModel>().Where( s => s.Email == "JillNewEmail" ).Delete();

    // second flavor
    _dbProvider.Delete<StudentModel>(s => s.Email == "JillNewEmail");

### Meta ###

We absolutely welcome contributions/suggestions/bug reports from you (the people using this package). If you have any ideas on how to improve it, please post an [issue](https://github.com/crossql/crossql/issues) with steps to reproduce, or better yet, submit a Pull Request.  

_*crossql* is forked from [futurestate.appcore.data](https://github.com/futurestatemobile/appcore.data). Commit history is in tact._
