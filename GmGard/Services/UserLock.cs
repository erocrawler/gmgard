using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GmGard.Services
{
    /// <summary>
    /// 按用户名串行化"先查后写"的操作(每日签到、每人一次的抽奖、补签券扣减等),
    /// 防止同一个用户并发请求同时通过检查而重复领取。
    /// 只在单进程内有效;如果以后部署多个实例,需要改成数据库唯一约束或分布式锁。
    /// </summary>
    public static class UserLock
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public static async Task<IDisposable> EnterAsync(string username)
        {
            var sem = Locks.GetOrAdd(username ?? string.Empty, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Releaser(sem);
        }

        private sealed class Releaser : IDisposable
        {
            private SemaphoreSlim _sem;
            public Releaser(SemaphoreSlim sem) { _sem = sem; }
            public void Dispose()
            {
                var s = Interlocked.Exchange(ref _sem, null);
                s?.Release();
            }
        }
    }
}
