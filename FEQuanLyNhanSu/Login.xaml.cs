using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Services.UserService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Auths;

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
                string password;
                if (txtPassword.Visibility == Visibility.Collapsed)
                {
                    password = txtPasswordVisible.Text;
                }
                else  {
                    password = txtPassword.Password;
                }
                var loginDto = new
                {
                    Username = txtUsername.Text,
                    Password = password
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
                string refreshToken = userData.data.refreshToken;

                Application.Current.Properties["Token"] = accessToken;
                Application.Current.Properties["RefreshToken"] = refreshToken;
                Application.Current.Properties["Fullname"] = fullname;
                Application.Current.Properties["UserRole"] = role;
                Application.Current.Properties["UserId"] = userId;

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

        public static async Task<HttpResponseMessage> SendAuthorizedRequestAsync(HttpRequestMessage request)
        {
            var token = Application.Current.Properties["AccessToken"]?.ToString();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Thử refresh token
                bool refreshOk = await RefreshTokenAsync();
                if (refreshOk)
                {
                    // Gửi lại request cũ với token mới
                    var newToken = Application.Current.Properties["AccessToken"]?.ToString();
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                    response = await client.SendAsync(request);
                }
                else
                {
                    MessageBox.Show("Phiên đăng nhập hết hạn, vui lòng đăng nhập lại!");
                    // TODO: Redirect về màn hình đăng nhập
                }
            }

            return response;
        }

        public static async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = Application.Current.Properties["RefreshToken"]?.ToString();
            if (string.IsNullOrWhiteSpace(refreshToken)) return false;

            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Auth/refresh-token";

            var payload = new { refreshToken = refreshToken };
            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<LoginResult>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Data?.AccessToken != null)
            {
                Application.Current.Properties["AccessToken"] = result.Data.AccessToken;
                Application.Current.Properties["RefreshToken"] = result.Data.RefreshToken;
                return true;
            }

            return false;
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
                txtPassword.Password = txtPasswordVisible.Text;
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
