# README #

这里是绅士之庭（gmgard.com）的网站代码。

### What is this repository for? ###

* Quick summary

绅士之庭网站主要用于分享2次元资源。网站本身不存储任何版权文件。所有文字、图片均由网友上传。

* Version

网站的具体版本请参见网站上的[版本更新历史](http://gmgard.com/Home/Suggestions)
网站最初创立于2013年，基于ASP.NET MVC 4创建。V0.70版本对网站代码进行了较大的重写，并迁移至ASP.NET Core。当前版本已升级至ASP.NET Core 10，支持跨平台部署。

* 运行平台

网站使用ASP.NET Core 10编写，支持跨平台部署。当前已成功部署于Linux平台。数据库支持SQL Server 2012+和PostgreSQL。网站可在Windows IIS或Linux（使用Kestrel/Nginx）上运行。

### How do I get set up? ###

* Summary of set up

推荐安装使用Visual Studio 2025或Visual Studio Code。需要安装[.NET 10 SDK](https://dotnet.microsoft.com/download)。随后加载项目即可直接运行。项目支持在Windows、Linux和macOS上开发和运行。

* Configuration

配置文件为appsettings.json，在本地测试运行时，可创建appsettings.Development.json覆盖默认配置。

* Database configuration

项目使用Entity Framework Core 10进行数据库管理。推荐使用PostgreSQL作为生产环境数据库，也支持SQL Server用于Windows环境。
使用GmGardMigrations项目创建和管理数据库迁移。

在appsettings.Development.json中配置数据库连接：
```
{
  "ConnectionStrings": {
    // PostgreSQL（推荐用于Linux/跨平台部署）:
    "GmGardUser": "Host=localhost;Database=gmgard_user;Username=postgres;Password=yourpassword",
    "GmGardData": "Host=localhost;Database=gmgard_data;Username=postgres;Password=yourpassword"
    // SQL Server示例（Windows环境）:
    // "GmGardUser": "Data Source=.\\SQLEXPRESS;Initial Catalog=MyMvcWebUser;Integrated Security=SSPI;MultipleActiveResultSets=True;",
    // "GmGardData": "Data Source=.\\SQLEXPRESS;Initial Catalog=MyMvcWebData;Integrated Security=SSPI;MultipleActiveResultSets=True;"
  },
  "ApplicationSettings": {
    "UploadSecret": "上传图片至static所需的密码",
	"SearchBackendType": "SqlServer"
  }
}
```
高级搜索功能有两种模式可选。可选择`SqlServer`或`ElasticSearch`
如要在本地环境测试，推荐使用SqlServer模式。
ElasticSearch需要单独搭建ES服务器。可通过GmGardMigrations > FillElasticSearch从数据库进行数据填充。

### Contribution guidelines ###

* Source Overview

本系统由以下主要项目组成：

-  GmGard：gmgard.com的主站源码。使用ASP.NET Core 10编写，支持跨平台部署。
-  GmGard.Client：基于Blazor WebAssembly的客户端应用。使用.NET 10和Tailwind CSS构建现代化前端界面。通讯API参见API.md。
-  GmGard.Models：数据库模型对象和Entity Framework相关配置。
-  GmApps：app.gmgard.com的源码。由Angular编写并与主站通过JSON通讯。即将被GmGard.Client取代。
-  GmGardMigrations：用于执行Entity Framework数据迁移工具，以及临时数据填充或修复任务。


### Who do I talk to? ###

* Repo owner or admin

Duo: admin@gmgard.com

* Other community or team contact

Discord group: https://discord.gg/sF9EGYCG
