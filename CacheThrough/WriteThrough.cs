namespace ServerSide.CacheThrough;

using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.DatasourceProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using log4net;
using log4net.Config;

public class WriteThrough : IWriteThruProvider
{
    private static readonly ILog log = LogManager.GetLogger(typeof(WriteThrough));

    private ICache _cache;
    private string _connectionString;
    private string _logFilePath;

    private string _logLevel;

    private string _VERSION;

    public WriteThrough()
    {
        log.Info($"{_VERSION} WriteThrough: Constructor invoke");
    }

    public void Init(IDictionary parameters, string cacheName)
    {
        try
        {
            _VERSION = Configuration.GetFileVersion();
            log.Info($"{_VERSION} WriteThrough: Init, called with {parameters.Count} parameters");
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
            _cache = CacheManager.GetCache(cacheName);
            log.Info($"{_VERSION} Initialized Cache: {cacheName}");
        }
        catch (System.Exception exp)
        {
            log.Error($"{_VERSION} Error initializing SubscriberLoader: {exp.Message}");
        }
    }
    public OperationResult WriteToDataSource(WriteOperation operation)
    {
        ProviderCacheItem cacheItem = operation.ProviderItem;
        object product = cacheItem.GetValue<object>();
        switch (operation.OperationType)
        {
            case WriteOperationType.Add:
                // Insert logic for Add operation
                try
                {
                    log.Info($"{_VERSION} WriteOperationType.Add: Written to data source");
                    //Todo: Add logic to write to data source
                }
                catch (Exception ex)
                {
                    log.Error($"{_VERSION} Error writing to data source: {ex.Message}");
                    return new OperationResult(operation, OperationResult.Status.Failure, ex.Message);
                }
                break;
            case WriteOperationType.Delete:
                // Insert logic for Delete operation
                try
                {
                    log.Info($"{_VERSION} WriteOperationType.Delete: Written to data source");
                }
                catch (Exception ex)
                {
                    log.Error($"{_VERSION} WriteOperationType.Delete: Error writing to data source: {ex.Message}");
                    return new OperationResult(operation, OperationResult.Status.Failure, ex.Message);
                }
                break;
            case WriteOperationType.Update:
                // Insert logic for Update operation
                try
                {
                    log.Info($"{_VERSION} WriteOperationType.Update: Written to data source");
                }
                catch (Exception ex)
                {
                    log.Error($"{_VERSION} WriteOperationType.Update: Error writing to data source: {ex.Message}");
                    return new OperationResult(operation, OperationResult.Status.Failure, ex.Message);
                }
                break;
        }
        return new OperationResult(operation, OperationResult.Status.Success);
    }
    public ICollection<OperationResult> WriteToDataSource(ICollection<WriteOperation> operations)
    {
        var operationResult = new List<OperationResult>();
        foreach (WriteOperation operation in operations)
        {
            ProviderCacheItem cacheItem = operation.ProviderItem;
            object item = cacheItem.GetValue<object>();
            switch (operation.OperationType)
            {
                case WriteOperationType.Add:
                    // Insert logic for Add operation
                    try
                    {
                        log.Info($"{_VERSION} WriteOperationType.Add: Written to data source");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{_VERSION} WriteOperationType.Add: Error writing to data source: {ex.Message}");
                        operationResult.Add(new OperationResult(operation, OperationResult.Status.Failure, ex.Message));
                        continue;
                    }
                    break;
                case WriteOperationType.Delete:
                    // Insert logic for Delete operation
                    try
                    {
                        log.Info($"{_VERSION} WriteOperationType.Delete: Written to data source");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{_VERSION} WriteOperationType.Delete: Error writing to data source: {ex.Message}");
                        operationResult.Add(new OperationResult(operation, OperationResult.Status.Failure, ex.Message));
                        continue;
                    }

                    break;
                case WriteOperationType.Update:
                    // Insert logic for Update operation
                    try
                    {
                        log.Info($"{_VERSION} WriteOperationType.Update: Written to data source");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{_VERSION} WriteOperationType.Update: Error writing to data source: {ex.Message}");
                        operationResult.Add(new OperationResult(operation, OperationResult.Status.Failure, ex.Message));
                        continue;
                    }

                    break;
            }
            operationResult.Add(new OperationResult(operation, OperationResult.Status.Success));
        }
        return operationResult;
    }
    public ICollection<OperationResult> WriteToDataSource(ICollection<DataTypeWriteOperation> dataTypeWriteOperations)
    {
        var operationResult = new List<OperationResult>();
        foreach (DataTypeWriteOperation operation in dataTypeWriteOperations)
        {
            var list = new List<object>();
            ProviderDataTypeItem<object> cacheItem = operation.ProviderItem;
            object data = (object)cacheItem.Data;
            switch (operation.OperationType)
            {
                case DatastructureOperationType.CreateDataType:
                    // Insert logic for creating a new List
                    IList myList = new List<object>();
                    myList.Add(data);
                    break;
                case DatastructureOperationType.AddToDataType:
                    // Insert logic for any Add operation
                    list.Add(data);
                    break;
                case DatastructureOperationType.DeleteFromDataType:
                    // Insert logic for any Remove operation
                    list.Remove(data);
                    break;
                case DatastructureOperationType.UpdateDataType:
                    // Insert logic for any Update operation
                    list.Insert(0, data);
                    break;
            }
            // Write Thru operation status can be set according to the result.
            operationResult.Add(new OperationResult(operation, OperationResult.Status.Success));
        }
        return operationResult;
    }
    public void Dispose()
    {
        log.Info($"{_VERSION} Disposing WriteThrough Provider");
    }
}