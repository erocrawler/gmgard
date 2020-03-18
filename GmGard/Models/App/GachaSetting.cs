using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public abstract class GachaSetting
    {
        virtual public int[] RarityDistribution => new[] { 30, 57, 82, 97, 100 };
        [JsonConverter(typeof(StringEnumConverter), true)]
        virtual public GachaPool.PoolName PoolName { get; }
        virtual public DateTime? StartTime { get; }
        virtual public DateTime? EndTime { get; }
    }
}
