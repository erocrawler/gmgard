using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Extensions
{
    public class SqlStringComparer : IEqualityComparer<string>
    {
        public static readonly int LCID = CultureInfo.GetCultureInfo("zh-CN").LCID;

        public bool Equals(string x, string y)
        {
            SqlString a = new SqlString(x, LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
            SqlString b = new SqlString(y, LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
            return a.Equals(b);
        }

        public int GetHashCode(string x)
        {
            SqlString a = new SqlString(x, LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
            return a.GetHashCode();
        }

        private SqlStringComparer() { }

        public static readonly SqlStringComparer Instance = new SqlStringComparer();
    }
}
