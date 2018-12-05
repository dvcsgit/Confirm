using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Utility
{
    public static class CacheHelper
    {
        public static T GetOrInsert<T>(this Cache Cache, string Key, Func<T> Generator)
        {
            var result = Cache[Key];

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                result = (Generator != null) ? Generator() : default(T);

                Cache.Insert(Key, result, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

                return (T)result;
            }
        }
    }
}
