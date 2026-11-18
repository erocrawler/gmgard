using System;
using System.Runtime.CompilerServices;

namespace GmGard.Tests
{
    internal static class TestInit
    {
        // 与生产一致(Startup.cs):库里的时间列是 timestamp without time zone,代码里用的是本地时间的 DateTime。
        // 必须在任何 Npgsql 对象创建之前设置。
        [ModuleInitializer]
        internal static void Init()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
