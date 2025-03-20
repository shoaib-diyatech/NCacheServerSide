namespace ServerSide.CacheThrough;

using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

public class ReadThrough : IReadThruProvider
{

    private static readonly ILog log = LogManager.GetLogger(typeof(ReadThrough));
    private ICache _cache;
    private string _connectionString;
    private string _logFilePath;
    private string _logLevel;

    private string _VERSION;

    public ReadThrough()
    {
        log.Info($"{_VERSION} ReadThrough: Constructor invoke");
    }

    public void Init(IDictionary parameters, string cacheId)
    {
        try
        {
            _VERSION = Configuration.GetFileVersion();
            log.Info($"{_VERSION} ReadThrough: Init, called with {parameters.Count} parameters");
            _connectionString = parameters.Contains("ConnectionString") ? parameters["ConnectionString"] as string : null;
            _logFilePath = parameters.Contains("LogFilePath") ? parameters["LogFilePath"] as string : null;
            _logLevel = parameters.Contains("LogLevel") ? parameters["LogLevel"] as string : "Debug";
            // Setting the log file path of logger
            if (_logFilePath != null)
            {
                Configuration.ConfigureLogging(_logFilePath, logLevel: _logLevel);
                log.Info($"{_VERSION} _logFilePath: {_logFilePath}, _logLevel: {_logLevel}");
                // Get the configured log level from the logger
                if (log.IsDebugEnabled)
                {
                    log.Debug($"{_VERSION} Debug log level enabled");
                }
                if (log.IsInfoEnabled)
                {
                    log.Info($"{_VERSION} Info log level enabled");
                }
            }
            else
            {
                log.Info($"{_VERSION} _logFilePath not found");
            }

            if (_connectionString != null)
            {
                log.Info($"{_VERSION} _connectionString: {_connectionString}");
            }
            else
            {
                log.Info($"{_VERSION} _connectionString not found");
            }
            _cache = CacheManager.GetCache(cacheId);
            log.Info($"{_VERSION} Initialized Cache: {cacheId}");
        }
        catch (System.Exception exp)
        {
            log.Error($"{_VERSION} Error initializing ReadThrough: {exp.Message}");
        }
    }
    public ProviderDataTypeItem<IEnumerable> LoadDataTypeFromSource(string key, DistributedDataType dataType)
    {
        try
        {
            log.Info($"LoadDataTypeFromSource called with key: {key}");
            IEnumerable value = null;
            ProviderDataTypeItem<IEnumerable> dataTypeItem = null;
            switch (dataType)
            {
                case DistributedDataType.List:
                    value = new List<object>()
                {
                    LoadFromDataSource(key)
                };
                    dataTypeItem = new ProviderDataTypeItem<IEnumerable>(value);
                    break;
                case DistributedDataType.Dictionary:
                    value = new Dictionary<string, object>()
                    {
                    { key ,  LoadFromDataSource(key) }
                    };
                    dataTypeItem = new ProviderDataTypeItem<IEnumerable>(value);
                    break;
                case DistributedDataType.Counter:
                    dataTypeItem = new ProviderDataTypeItem<IEnumerable>(1000);
                    break;
                default:
                    var data = LoadFromSource(key);
                    _cache.Add(key, data);
                    break;
            }
            return dataTypeItem;
        }
        catch (Exception exp)
        {
            log.Error($"Error fetching data from data source: {exp.Message}");
            return null;
        }
    }
    public ProviderCacheItem LoadFromSource(string key)
    {
        try
        {
            log.Info($"LoadFromSource called with key: {key}");

            // LoadFromDataSource loads data from data source
            object value = LoadFromDataSource(key);
            var cacheItem = new ProviderCacheItem(value);
            return cacheItem;
        }
        catch (Exception exp)
        {
            log.Error($"Error fetching data from data source: {exp.Message}");
            return null;
        }
    }
    public IDictionary<string, ProviderCacheItem> LoadFromSource(ICollection<string> keys)
    {
        log.Info($"LoadFromSource called with {keys.Count} keys");
        var dictionary = new Dictionary<string, ProviderCacheItem>();
        try
        {
            foreach (var key in keys)
            {
                var value = LoadFromDataSource(key);
                dictionary.Add(key, new ProviderCacheItem(value));
            }
            log.Info($"{_VERSION} LoadFromSource: Fetched {dictionary.Count} items from data source");
            return dictionary;
        }
        catch (Exception exp)
        {
            log.Error($"Error fetching data from data source: {exp.Message}");
        }
        return dictionary;
    }

    /// <summary>
    /// Simulates loading key from data source
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private object LoadFromDataSource(string key)
    {
        var value = "FetchedFromDataSource_" + key;
        log.Info($"Value fetched from datasource: {value}");
        return value;
    }
    public void Dispose()
    {
        log.Info($"{_VERSION} ReadThrough: Dispose invoke");
    }
}