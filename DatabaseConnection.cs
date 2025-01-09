using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;

namespace QuanLyKho
{
    public class DatabaseConnection
    {
        private static readonly string _connectionString;
        private static readonly object _lock = new object();
        private static SqlConnection? _sharedConnection;

        static DatabaseConnection()
        {
            string[] connectionStrings = new string[]
            {
                ConfigurationManager.ConnectionStrings["QuanLyKhoDatabase"]?.ConnectionString + ";CharSet=utf8",
                ConfigurationManager.ConnectionStrings["QuanLyKhoDatabase"]?.ConnectionString,
                @"Data Source=DESKTOP-UQPCGAK\SQLEXPRESS;Initial Catalog=QuanLyKho;Persist Security Info=True;User ID=sa;Password=123;Max Pool Size=200;Min Pool Size=5;Pooling=true"
            };

            foreach (var connStr in connectionStrings.Where(cs => !string.IsNullOrEmpty(cs)))
            {
                try
                {
                    using (var testConnection = new SqlConnection(connStr))
                    {
                        testConnection.Open();
                        _connectionString = connStr;
                        return;
                    }
                }
                catch
                {
                    continue;
                }
            }

            throw new Exception("Không thể thiết lập kết nối đến database. Vui lòng kiểm tra cấu hình.");
        }

        public static SqlConnection GetConnection()
        {
            if (_sharedConnection == null)
            {
                lock (_lock)
                {
                    if (_sharedConnection == null)
                    {
                        _sharedConnection = new SqlConnection(_connectionString);
                    }
                }
            }

            if (_sharedConnection.State != ConnectionState.Open)
            {
                lock (_lock)
                {
                    if (_sharedConnection.State != ConnectionState.Open)
                    {
                        try
                        {
                            _sharedConnection.Open();
                        }
                        catch (Exception)
                        {
                            _sharedConnection.Dispose();
                            _sharedConnection = new SqlConnection(_connectionString);
                            _sharedConnection.Open();
                        }
                    }
                }
            }

            return _sharedConnection;
        }

        public static SqlConnection CreateNewConnection()
        {
            var connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public static void CloseConnection(SqlConnection? connection)
        {
            if (connection != null && connection != _sharedConnection)
            {
                try
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi đóng kết nối: {ex.Message}");
                }
            }
        }

        public static void ExecuteInTransaction(Action<SqlConnection, SqlTransaction> action)
        {
            using (var connection = CreateNewConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    action(connection, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static T ExecuteInTransaction<T>(Func<SqlConnection, SqlTransaction, T> func)
        {
            using (var connection = CreateNewConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var result = func(connection, transaction);
                    transaction.Commit();
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
