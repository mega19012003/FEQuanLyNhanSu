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
using System.Net.Http.Headers;
using FEQuanLyNhanSu.Helpers;

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
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        { // Hiện loading
            btnCreate.IsEnabled = false;
            loadingBar.Visibility = Visibility.Visible;
            txtLoading.Visibility = Visibility.Visible;

            try
            {
                var loginDto = new
                {
                    Username = txtUsername.Text,
                    Password = txtPassword.Password
                };

                using var client = new HttpClient();

                // B1: Gửi yêu cầu login
                var loginJson = JsonConvert.SerializeObject(loginDto);
                var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
                var BaseUrl1 = AppsettingConfigHelper.GetBaseUrl();
                var apiUrl1 = $"{BaseUrl1}/api/Auth/login";
                var loginResponse = await client.PostAsync(apiUrl1, loginContent);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đăng nhập thất bại!");
                    return;
                }

                var loginResult = JsonConvert.DeserializeObject<dynamic>(await loginResponse.Content.ReadAsStringAsync());
                string accessToken = loginResult.data.accessToken;

                // B2: Gọi /current để lấy info
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var BaseUrl2 = AppsettingConfigHelper.GetBaseUrl();
                var apiUrl2 = $"{BaseUrl2}/api/auth/current";
                var userResponse = await client.GetAsync(apiUrl2);

                if (!userResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Không thể lấy thông tin người dùng!");
                    return;
                }

                var userData = JsonConvert.DeserializeObject<dynamic>(await userResponse.Content.ReadAsStringAsync());

                string fullname = userData.data.fullname;
                string role = userData.data.role;
                string userId = userData.data.userId;

                // Lưu vào Application để xài toàn app
                Application.Current.Properties["Token"] = accessToken;
                Application.Current.Properties["Fullname"] = fullname;
                Application.Current.Properties["UserRole"] = role;
                Application.Current.Properties["UserId"] = userId;

                // Mở MainWindow
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
            finally
            {
                // Ẩn loading
                btnCreate.IsEnabled = true;
                loadingBar.Visibility = Visibility.Collapsed;
                txtLoading.Visibility = Visibility.Collapsed;
            }
        }


        private bool _isPasswordVisible = false;
        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
            }
        }
    }
}
