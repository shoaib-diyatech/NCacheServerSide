namespace ServerSide.CacheThrough;

using Microsoft.Data.SqlClient;
using log4net;
using NCacheClient;


public class DataLayer
{
    private string _connectionString;

    private SqlConnection _connection;

    private static ILog log;

    public DataLayer(ILog ilog, string connectionString)
    {
        log = ilog;
        _connectionString = connectionString;
        log.Debug($" DataLayer: Constructor invoked");
    }

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        try
        {
            log.Debug($" DataLayer: Connect, called with connection string: {_connectionString}");
            if (!string.IsNullOrEmpty(_connectionString))
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _connection = connection;
                    _connection.Open();
                    IsConnected = true;
                    log.Info($" DataLayer: Connected to database");
                }
            }
            else
            {
                log.Error($" DataLayer: Connection string is null or empty");
            }
        }
        catch (Exception ex)
        {
            log.Error($" DataLayer: Connect failed with exception: {ex.Message}");
        }
    }

    public Subscriber LoadSubscriber(string msisdn)
    {
        log.Debug($" DataLayer: LoadSubscriber, called with msisdn: {msisdn}");
        Subscriber subscriber = null;

        try
        {
            if (IsConnected)
            {
                string query = "SELECT * FROM Subscribers WHERE MSISDN = @msisdn";
                using (var command = new SqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@msisdn", msisdn);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader == null)
                        {
                            log.Error($" DataLayer: LoadSubscriber failed, reader is null");
                            return null;
                        }
                        if (reader.HasRows == false)
                        {
                            log.Error($" DataLayer: LoadSubscriber failed, no rows found");
                            return null;
                        }
                        if (reader.Read())
                        {
                            subscriber = new Subscriber
                            {
                                Msisdn = reader["MSISDN"].ToString(),
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
                log.Error($" DataLayer: Not connected to database");
            }
        }
        catch (Exception ex)
        {
            log.Error($" DataLayer: LoadSubscriber failed with exception: {ex.Message}");
        }
        return subscriber;
    }

    public void Disconnect()
    {
        log.Debug($" DataLayer: Disconnect, called");
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            IsConnected = false;
            log.Info($" DataLayer: Disconnected from database");
        }
        else
        {
            log.Error($" DataLayer: Connection is null");
        }
    }

    public void Dispose()
    {
        log.Debug($" DataLayer: Dispose, called");
        Disconnect();
    }
}

