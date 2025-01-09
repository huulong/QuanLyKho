using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;

namespace QuanLyKho
{
    public class DatabaseConnection
    {
        // Đọc connection string từ file cấu hình
        private static string GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QuanLyKhoDatabase"]?.ConnectionString;

            // Nếu không có trong file cấu hình, sử dụng giá trị mặc định
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLyKho;Integrated Security=True";
            }

            return connectionString;
        }

        public static SqlConnection GetConnection()
        {
            string[] connectionStrings = new string[]
            {
                 ConfigurationManager.ConnectionStrings["QuanLyKhoDatabase"]?.ConnectionString + ";CharSet=utf8",
                // Thử các connection string khác nhau
                ConfigurationManager.ConnectionStrings["QuanLyKhoDatabase"]?.ConnectionString,
                @"Data Source=LAPTOP-CU2R9HHH\SQLEXPRESS01;Initial Catalog=QuanLyKho;Persist Security Info=True;User ID=sa;Password=123456;"
            };

            foreach (var connectionString in connectionStrings.Where(cs => !string.IsNullOrEmpty(cs)))
            {
                try
                {
                    SqlConnection connection = new SqlConnection(connectionString);
                    connection.Open();

                    // Kiểm tra kết nối và database
                    using (SqlCommand cmd = new SqlCommand("SELECT DB_NAME()", connection))
                    {
                        string currentDatabase = cmd.ExecuteScalar()?.ToString();
                        Console.WriteLine($"Kết nối thành công đến database: {currentDatabase}");
                    }

                    return connection;
                }
                catch (SqlException ex)
                {
                    // Log chi tiết lỗi
                    Console.WriteLine($"Lỗi kết nối: {ex.Message}");
                    Console.WriteLine($"Mã lỗi: {ex.Number}");
                    Console.WriteLine($"Thử connection string: {connectionString}");

                    // Các mã lỗi phổ biến
                    switch (ex.Number)
                    {
                        case 4060: // Không thể mở database
                            Console.WriteLine("Lỗi: Không thể mở database. Kiểm tra tên database.");
                            break;
                        case 18456: // Lỗi đăng nhập
                            Console.WriteLine("Lỗi: Đăng nhập không thành công. Kiểm tra tài khoản và mật khẩu.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi không xác định: {ex.Message}");
                }
            }

            throw new Exception("Không thể kết nối đến cơ sở dữ liệu sau nhiều lần thử.");
        }

        public static void CloseConnection(SqlConnection connection)
        {
            if (connection != null)
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
    }
}
