namespace ServerSide.CacheThrough;

using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using log4net;


public class ReadThrough : IReadThruProvider
{
    private static readonly ILog log = LogManager.GetLogger(typeof(ReadThrough));
    private ICache _cache;
    private string _connectionString;
    private string _logFilePath;
    private string _logLevel;

    private string _VERSION;

    private DataLayer dataLayer;

    public ReadThrough()
    {
        log.Info($"{_VERSION} ReadThrough: Constructor invoke, Instance: {this.GetHashCode()}");
    }

    public void Init(IDictionary parameters, string cacheId)
    {
        try
        {
            _VERSION = Configuration.GetFileVersion();
            log.Info($"{_VERSION} ReadThrough: Init, called with {parameters.Count} parameters, Instance: {this.GetHashCode()}");
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
            log.Debug($"{_VERSION} ReadThrough: Init, called with {parameters.Count} parameters, Instance: {this.GetHashCode()}");
            try
            {
                // initializing sql connection
                dataLayer = new DataLayer(log, _connectionString);
                dataLayer.Connect();
            }
            catch (Exception exp)
            {
                log.Error($"{_VERSION} Error initializing DataLayer: {exp.Message}", exp);
                throw;
            }
            if (dataLayer.IsConnected)
            {
                log.Info($"{_VERSION} DataLayer connected");
            }
            else
            {
                log.Error($"{_VERSION} DataLayer not connected");
            }
        }
        catch (System.Exception exp)
        {
            log.Error($"{_VERSION} Error initializing ReadThrough: {exp.Message}");
        }
    }

    /// <summary>
    /// Responsible for loading data structures from the external data source. 
    /// </summary>
    /// <param name="key">key to fetch from data source</param>
    /// <param name="dataType">type of data structure received</param>
    /// <returns>Data structure contained in ProviderCacheItem which can be enumerated</returns>
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

    /// <summary>
    /// Responsible for loading an object from the external data source. 
    /// Key is passed as parameter.
    /// <param name="key">item identifier; probably a primary key</param>
    /// <returns>data contained in ProviderCacheItem</returns>
    public ProviderCacheItem LoadFromSource(string key)
    {
        try
        {
            log.Info($"ReadThrough: LoadFromSource called with key: {key} Instance: {this.GetHashCode()}");

            ProviderCacheItem cacheItem = new ProviderCacheItem(LoadFromDataSource(key));
            cacheItem.ResyncOptions.ResyncOnExpiration = true;
            // Resync provider name will be picked from default provider.
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
        log.Debug($"ReadThrough: LoadFromDataSource called with key: {key} Instance: {this.GetHashCode()}");
        if (dataLayer == null)
        {
            log.Error($"DataLayer is null. Make sure Init() was called before using ReadThrough. Instance: {this.GetHashCode()}");
            return null;
        }
        var value = dataLayer.LoadSubscriber(key);
        if (value == null)
        {
            log.Error($"Value not found in data source for key: {key}");
            return null;
        }
        log.Info($"Value fetched from datasource: {value}");
        return value;
    }

    /// <summary>
    ///  Perform tasks associated with freeing, releasing, or resetting resources.
    /// </summary>
    public void Dispose()
    {
        dataLayer.Dispose();
        log.Info($"{_VERSION} ReadThrough: Dispose, called");
    }
}