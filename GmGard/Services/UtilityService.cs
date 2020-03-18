using FluentScheduler;
using GmGard.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public abstract class UtilityService
    {
        protected BlogContext _db;
        protected UsersContext _udb;
        protected readonly IMemoryCache _cache;

        public UtilityService(BlogContext db, UsersContext udb, IMemoryCache cache)
        {
            _db = db;
            _udb = udb;
            _cache = cache;
        }
    }
}
