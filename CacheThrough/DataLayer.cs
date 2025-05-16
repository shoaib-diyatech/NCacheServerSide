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
        IsConnected = false;
        log.Debug($" DataLayer: Constructor invoked, IsConnected: {IsConnected} Instance: {this.GetHashCode()}");
    }

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        try
        {
            log.Debug($" DataLayer: Connect, called with connection string: {_connectionString} Instance: {this.GetHashCode()}");
            if (!string.IsNullOrEmpty(_connectionString))
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
                IsConnected = true;
                log.Debug($" DataLayer: Connected to database, IsConnected: {IsConnected}");
            }
            else
            {
                log.Error($" DataLayer: Connection string is null or empty");
            }
        }
        catch (Exception ex)
        {
            log.Error($" DataLayer: Connect failed with exception: {ex.Message}", ex);
        }
    }

    public Subscriber LoadSubscriber(string msisdn)
    {
        log.Debug($"DataLayer: LoadSubscriber, called with msisdn(key): {msisdn} Instance: {this.GetHashCode()}");
        Subscriber subscriber = null;
        try
        {
            log.Debug(" DataLayer: LoadSubscriber, called");
            log.Debug($"DataLayer: LoadSubscriber: IsConnected: {IsConnected}, _connection: {_connection}");

            if (IsConnected && _connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                string query = "SELECT * FROM MyStatus.dbo.Subscriber WHERE MSISDN = @msisdn";
                log.Debug($" DataLayer: LoadSubscriber, query: {query}");
                using (var command = new SqlCommand(query, _connection))
                {
                    if (command.Parameters == null)
                    {
                        log.Error($" DataLayer: SqlCommand.Parameters is null");
                        return null;
                    }
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
                            // Dynamically log all columns and their values
                            var values = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.GetValue(i);
                                values.Add($"{columnName}: {value}");
                            }
                            log.Debug($"DataLayer: reader: {string.Join(", ", values)}");

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
                log.Error($" DataLayer: Not connected to database or connection is not open. IsConnected: {IsConnected}, _connection: {_connection}, State: {_connection?.State}");
            }
        }
        catch (Exception ex)
        {
            log.Error($" DataLayer: LoadSubscriber failed with exception: {ex.Message}", ex);
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

