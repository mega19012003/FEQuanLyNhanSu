using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Services.UserService;

namespace FEQuanLyNhanSu
{
    /// <summary>  
    /// Interaction logic for Login.xaml  
    /// </summary>  
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var loginData = new
            {
                Username = txtUsername.Text,
                Password = txtPassword.Password
            };

            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsync("https://demonhanvienapi.duckdns.org/api/Auth/login", content);

                var respStr = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var respObj = JObject.Parse(respStr);
                    var token = respObj["data"]?["accessToken"]?.ToString();
                    var refreshToken = respObj["data"]?["refreshToken"]?.ToString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        Application.Current.Properties["AccessToken"] = token;
                        Application.Current.Properties["RefreshToken"] = refreshToken;

                        new MainWindow().Show();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Đăng nhập thất bại: " + respStr, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
