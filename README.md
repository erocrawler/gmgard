# README #

这里是绅士之庭（gmgard.com）的网站代码。

### What is this repository for? ###

* Quick summary

绅士之庭网站主要用于分享2次元资源。网站本身不存储任何版权文件。所有文字、图片均由网友上传。

* Version

网站的具体版本请参见网站上的[版本更新历史](http://gmgard.com/Home/Suggestions)
网站最初创立于2013年，基于ASP.NET MVC 4创建。V0.70版本对网站代码进行了较大的重写，并迁移至ASP.NET CORE。

* 运行平台

网站目前运行于Windows IIS（需要.Net Core 3.1+)，以及Sql Server 2012+。网站使用ASP.NET CORE编写。Dotnet Core使网站在Linux平台运行成为可能。未来可以考虑进一步迁移至Linux。

### How do I get set up? ###

* Summary of set up

推荐安装使用Visual Studio Community最新版。微软大法好。同时需要安装[.net core](https://www.microsoft.com/net/core)工具。随后加载项目即可直接运行。

* Configuration

配置文件为appsettings.json，在本地测试运行时，可创建appsettings.Development.json覆盖默认配置。如需访问Production服务器，请向我索要密钥。
没有密钥将无法上传图片至static服务器。

* Database configuration

由于Entity Framework 7尚不完善，数据库搭建仍依赖于EF6，使用GmGardMigrations项目创建数据库。
如果在PMC出现Entity Framework工具无法启动的情况，尝试设置GmGardMigrations项目为启动项。

默认参数将使用LocalDB启动网站。如需要使用SQL Server Express，或其他数据库，在appsettings.Development.json中配置：
```
{
  "ConnectionStrings": {
    "GmGardUser": "Data Source=.\\SQLEXPRESS;Initial Catalog=MyMvcWebUser;Integrated Security=SSPI;MultipleActiveResultSets=True;",
    "GmGardData": "Data Source=.\\SQLEXPRESS;Initial Catalog=MyMvcWebData;Integrated Security=SSPI;MultipleActiveResultSets=True;"
  },
  "ApplicationSettings": {
    "UploadSecret": "上传图片至static所需的密码"
	"SearchBackendType": "SqlServer"
  }
}
```
高级搜索功能有两种模式可选。可选择`SqlServer`或`ElasticSearch`
如要在本地环境测试，推荐使用SqlServer，请安装Sql Server Express with Fulltext Search。随后执行CreateFulltextIndex.sql及dbo.ContainsSearchBlog.sql即可。
ElasticSearch需要单独搭建ES服务器。可通过GmGardMigrations > FillElasticSearch从数据库进行数据填充。

### Contribution guidelines ###

* Source Overview

本系统由5个文件夹组成：

-  AspNet.Identity.EntityFramework6：由同名库Fork出，ASP.NET Identity provider for Entity Framework 6。用于连接EF6与.Net Core。
-  GmApps：app.gmgard.com的源码。由Angular编写并与主站通过JSON通讯。通讯API参见API.md。
-  GmGard：gmgard.com的源码。由.Net Core编写。
-  Gmgard.JobRunner：主站的附属程序，用于运行时后台任务处理。
-  Gmgard.Models：数据库模型对象。
-  GmgardMigrations：用于执行Entity Framework数据迁移工具，以及临时数据填充或修复任务。


### Who do I talk to? ###

* Repo owner or admin

Duo: admin@gmgard.com

* Other community or team contact

绅士qq大群: 1012997195