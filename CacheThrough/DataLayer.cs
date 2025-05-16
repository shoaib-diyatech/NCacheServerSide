namespace ServerSide.CacheThrough;

using Microsoft.Data.SqlClient;
using log4net;

public class DataLayer
{
    private string _connectionString;

    private SqlConnection _connection;

    private static readonly ILog log;

    public DataLayer(ILog log, string connectionString)
    {
        log = log;
        _connectionString = connectionString;
        _VERSION = Configuration.GetFileVersion();
        log.Debug($"{_VERSION} DataLayer: Constructor invoked");
    }

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        try
        {
            log.Debug($"{_VERSION} DataLayer: Connect, called with connection string: {_connectionString}");
            if (!string.IsNullOrEmpty(_connectionString))
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _connection = connection;
                    _connection.Open();
                    IsConnected = true;
                    log.Info($"{_VERSION} DataLayer: Connected to database");
                }
            }
            else
            {
                log.Error($"{_VERSION} DataLayer: Connection string is null or empty");
            }
        }
        catch (Exception ex)
        {
            log.Error($"{_VERSION} DataLayer: Connect failed with exception: {ex.Message}");
        }
    }

    public Subscriber LoadSubscriber(string msisdn)
    {
        log.Debug($"{_VERSION} DataLayer: LoadSubscriber, called with msisdn: {msisdn}");
        Subscriber subscriber = null;

        try
        {
            if (IsConnected)
            {
                string query = "SELECT * FROM Subscribers WHERE MSISDN = @msisdn";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@msisdn", msisdn);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null)
                        {
                            log.Error($"{_VERSION} DataLayer: LoadSubscriber failed, reader is null");
                            return null;
                        }
                        if (reader.HasRows == false)
                        {
                            log.Error($"{_VERSION} DataLayer: LoadSubscriber failed, no rows found");
                            return null;
                        }
                        if (reader.Read())
                        {
                            subscriber = new Subscriber
                            {
                                MSISDN = reader["MSISDN"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                DateOfBirth = DateOnly.Parse(reader["DateOfBirth"].ToString()),
                                Id = Convert.ToInt64(reader["Id"]),
                            };
                        }
                    }
                }
            }
            else
            {
                log.Error($"{_VERSION} DataLayer: Not connected to database");
            }
        }
        catch (Exception ex)
        {
            log.Error($"{_VERSION} DataLayer: LoadSubscriber failed with exception: {ex.Message}");
        }
        return subscriber;
    }

    public void Disconnect()
    {
        log.Debug($"{_VERSION} DataLayer: Disconnect, called");
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            IsConnected = false;
            log.Info($"{_VERSION} DataLayer: Disconnected from database");
        }
        else
        {
            log.Error($"{_VERSION} DataLayer: Connection is null");
        }
    }

    public void Dispose()
    {
        log.Debug($"{_VERSION} DataLayer: Dispose, called");
        Disconnect();
    }
}

