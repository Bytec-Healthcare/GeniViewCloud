using GeniView.Cloud.Models;
using GeniView.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace RenityArtemis.Web.Common
{
    /// <summary>
    /// For the Artemis to cache data. 
    /// User can insert data to cache and set the expired time, default is 5 seconds.
    /// </summary>
    public class MemCacheHelper
    {
        private static ObjectCache _cache = MemoryCache.Default;

        /// <summary>
        /// Set data to cache, default expired time is 5 seconds.
        /// </summary>
        /// <typeparam name="T">Class name</typeparam>
        /// <param name="key">key of cache</param>
        /// <param name="dataObject">data to save in cache</param>
        /// <param name="expiredSecond">expired time</param>
        public void SetCache<T>(string key, T dataObject, int expiredSecond = 5)
        {
            var policy = new CacheItemPolicy();

            if (expiredSecond >=0)
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiredSecond);
            }

            _cache.Set(key, dataObject, policy);
        }

        /// <summary>
        /// Get the data by key.
        /// </summary>
        /// <typeparam name="T">Class name</typeparam>
        /// <param name="key">key of cache</param>
        /// <returns></returns>
        public T GetCache<T>(string key)
        {
            if (_cache.Contains(key) == true)
            {
                var data = _cache.Get(key);
                return (T)data;
            }
            else
            {
                return default(T);
            }    
        }

        public List<CommandResult> GetLogRateResult(List<string> ids)
        {
            var ret = GetCache<ConcurrentDictionary<string, CommandResult>>("LogRateResult");
            var result = new List<CommandResult>();

            if (ids != null || ids.Any() == true)
            {
                foreach (var item in ids)
                {
                    if (ret != null && ret.TryGetValue(item, out CommandResult log) == true) //Faster than Linq
                    {
                        result.Add(log);
                    }
                    else
                    {
                        //Doesn't exist.
                        var unknow = new CommandResult(item);
                        unknow.Guid = Guid.Empty;
                        unknow.DateTimeUTC = DateTime.MinValue.ToString();
                        result.Add(unknow);
                    }
                }
            }
            else
            {
                if (ret != null)
                {
                    result.AddRange(ret.Values);
                }
            }

            return result;
        }

        public List<CommandResult> GetOTAResult(List<string> ids)
        {
            var ret = GetCache<ConcurrentDictionary<string, CommandResult>>("OTAResult");
            var result = new List<CommandResult>();

            if (ids != null || ids.Any() == true)
            {
                foreach (var item in ids)
                {
                    if (ret != null && ret.TryGetValue(item, out CommandResult log) == true) //Faster than Linq
                    {
                        result.Add(log);
                    }
                    else
                    {
                        //Doesn't exist.
                        var unknow = new CommandResult(item);
                        unknow.Guid        = Guid.Empty;
                        unknow.DateTimeUTC = DateTime.MinValue.ToString();
                        result.Add(unknow);
                    }
                }
            }
            else
            {
                if (ret != null)
                {
                    result.AddRange(ret.Values);
                }
            }

            return result;
        }

        public List<CommandResult> GetNTPResult(List<string> ids)
        {
            var ret = GetCache<ConcurrentDictionary<string, CommandResult>>("NTPResult");
            var result = new List<CommandResult>();

            if (ids != null || ids.Any() == true)
            {
                foreach (var item in ids)
                {
                    if (ret != null && ret.TryGetValue(item, out CommandResult log) == true) //Faster than Linq
                    {
                        result.Add(log);
                    }
                    else
                    {
                        //Doesn't exist.
                        var unknow = new CommandResult(item);
                        unknow.Guid = Guid.Empty;
                        unknow.DateTimeUTC = DateTime.MinValue.ToString();
                        result.Add(unknow);
                    }
                }
            }
            else
            {
                if (ret != null)
                {
                    result.AddRange(ret.Values);
                }
            }

            return result;
        }

    }
}