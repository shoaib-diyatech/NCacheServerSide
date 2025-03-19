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

public class ReadThrough : IReadThruProvider
{
    private SqlConnection _connection;
    private string connectionstringDB;
    private ICache cache;
    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Close();
        }
    }
    public void Init(IDictionary parameters, string cacheId)
    {
        //cache = CacheManager.GetCache(cacheId);
        //string connString = "Server=KUMAIL-RAZA\\SQLEXPRESS;Database=testdata;User Id=sa;Password=4Islamabad;Encrypt=False;";
        //connectionstringDB = connString;
        //if (connString != "")
        //    _connection = new SqlConnection(connString);
        //try
        //{
        //    _connection.Open();
        //}
        //catch (Exception ex)
        //{
        //    throw new Exception("Cannot connect Error: " + ex);
        //    //handle exception
        //}
    }
    public ProviderDataTypeItem<IEnumerable> LoadDataTypeFromSource(string key, DistributedDataType dataType)
    {
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
                cache.Add(key, data);
                break;
        }
        return dataTypeItem;
    }
    public ProviderCacheItem LoadFromSource(string key)
    {
        // LoadFromDataSource loads data from data source
        object value = LoadFromDataSource(key);
        var cacheItem = new ProviderCacheItem(value);
        return cacheItem;
    }
    public IDictionary<string, ProviderCacheItem> LoadFromSource(ICollection<string> keys)
    {
        var dictionary = new Dictionary<string, ProviderCacheItem>();
        try
        {
            var objects = FetchRandomProducts();
            foreach (Product key in objects)
            {
                // LoadFromDataSource loads data from data source
                dictionary.Add(key.ProductName, new ProviderCacheItem(key));
            }
            return dictionary;
        }
        catch (Exception exp)
        {
            // Handle exception
        }
        return dictionary;
    }
    private object LoadFromDataSource(string key)
    {
        // Define the SQL query to fetch product data
        string query = "SELECT ProductName, UnitPrice, QuantityPerUnit, RowVersionColumn FROM Product WHERE ProductName = @key";
        Product product = new Product();
        product.ProductName = key;
        product.UnitPrice = "123";
        product.QuantityPerUnit = "789";
        // Create SQL connection and command
        //using (_connection)
        //{
        //    using (SqlCommand command = new SqlCommand(query, _connection))
        //    {
        //        command.Parameters.AddWithValue("@key", key);
        //        // Execute the query and fetch data
        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                // Create Product object and populate it with data
        //                product = new Product
        //                {
        //                    ProductName = reader.GetString(0),      // ProductName
        //                    UnitPrice = reader.GetString(1), // UnitPrice (converted to string)
        //                    QuantityPerUnit = reader.GetString(2)   // QuantityPerUnit
        //                };
        //                //// Add the product to the list
        //                //products.Add(product);
        //            }
        //        }
        //    }
        //}
        return product;
    }
    private IList<object> FetchRandomProducts()
    {
        // List to hold fetched products
        var products = new List<object>();
        // Create a Random object to generate random data
        Random random = new Random();
        // Define an array of sample product names for random selection
        string[] sampleProductNames = new string[]
        {
        "Product A", "Product B", "Product C", "Product D", "Product E",
        "Product F", "Product G", "Product H", "Product I", "Product J",
        "Product K", "Product L", "Product M", "Product N", "Product O",
        "Product P", "Product Q", "Product R", "Product S", "Product T"
        };
        // Generate 20 random products
        for (int i = 0; i < 20; i++)
        {
            var product = new Product
            {
                ProductName = sampleProductNames[random.Next(sampleProductNames.Length)], // Randomly select a product name
                UnitPrice = random.Next(1, 1000).ToString(), // Random UnitPrice between 1 and 1000
                QuantityPerUnit = random.Next(1, 100).ToString() // Random QuantityPerUnit between 1 and 100
            };
            // Add the random product to the list
            products.Add(product);
        }
        return products;
    }
}