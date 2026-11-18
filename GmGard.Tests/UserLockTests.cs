using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GmGard.Services;
using Xunit;

namespace GmGard.Tests
{
    public class UserLockTests
    {
        [Fact]
        public async Task Same_user_serializes_read_modify_write()
        {
            const string user = "lock-serial-user";
            const int workers = 20;

            int counter = 0;        // 非原子计数器:读 -> 延迟 -> 写
            int holders = 0;        // 当前持锁的任务数
            int maxHolders = 0;

            var tasks = new List<Task>();
            for (int i = 0; i < workers; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (await UserLock.EnterAsync(user))
                    {
                        int now = Interlocked.Increment(ref holders);
                        InterlockedMax(ref maxHolders, now);

                        int read = counter;
                        await Task.Delay(5);
                        counter = read + 1;

                        Interlocked.Decrement(ref holders);
                    }
                }));
            }

            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(workers, counter);
            Assert.Equal(1, maxHolders);
        }

        [Fact]
        public async Task Different_users_do_not_block_each_other()
        {
            var a = await UserLock.EnterAsync("lock-user-A");
            try
            {
                // A 持锁期间,B 应立刻拿到锁
                var bTask = UserLock.EnterAsync("lock-user-B");
                var finished = await Task.WhenAny(bTask, Task.Delay(TimeSpan.FromSeconds(2)));

                Assert.Same(bTask, finished);
                using (await bTask)
                {
                }
            }
            finally
            {
                a.Dispose();
            }
        }

        [Fact]
        public async Task Username_is_case_insensitive()
        {
            var first = await UserLock.EnterAsync("Alice-case");
            try
            {
                var second = UserLock.EnterAsync("alice-case");

                // 大小写不同但同一用户:必须等待第一个释放
                var finished = await Task.WhenAny(second, Task.Delay(200));
                Assert.NotSame(second, finished);

                first.Dispose();
                first = null;

                var got = await second.WaitAsync(TimeSpan.FromSeconds(2));
                got.Dispose();
            }
            finally
            {
                first?.Dispose();
            }
        }

        [Fact]
        public async Task Dispose_twice_does_not_throw_or_over_release()
        {
            const string user = "lock-double-dispose";

            var first = await UserLock.EnterAsync(user);
            first.Dispose();
            first.Dispose(); // 第二次必须是空操作

            // 如果第二次 Dispose 多释放了一次信号量,下面两个人就能同时持锁
            var holder = await UserLock.EnterAsync(user);
            try
            {
                var second = UserLock.EnterAsync(user);
                var finished = await Task.WhenAny(second, Task.Delay(200));
                Assert.NotSame(second, finished);

                holder.Dispose();
                var got = await second.WaitAsync(TimeSpan.FromSeconds(2));
                got.Dispose();
            }
            finally
            {
                holder.Dispose();
            }
        }

        private static void InterlockedMax(ref int target, int value)
        {
            int snapshot;
            do
            {
                snapshot = Volatile.Read(ref target);
                if (value <= snapshot) return;
            } while (Interlocked.CompareExchange(ref target, value, snapshot) != snapshot);
        }
    }
}
