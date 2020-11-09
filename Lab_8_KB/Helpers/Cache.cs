using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;

namespace Lab_8_KB.Helpers
{
    public static class Cache
    {
        private static MemoryCache memoryCache = MemoryCache.Default;
        private const int CacheDuration = 100;
        public static void Add(string key, object value)
        {
            memoryCache.Add(key, value, DateTime.Now.AddSeconds(CacheDuration));
        }

        public static bool Contains(string key)
        {
            return memoryCache.Contains(key);
        }

        public static T Get<T>(string key) where T: class{
            if (Contains(key))
                return memoryCache.Get(key) as T;

            return null;
        }

        public static T GetOrCreate<T>(string key, Func<T> createFunc) where T: class
        {
            var obj = Get<T>(key);
            if (obj == null)
            {
                obj = createFunc();
                Add(key, obj);
            }
            return obj;
        }
    }
}