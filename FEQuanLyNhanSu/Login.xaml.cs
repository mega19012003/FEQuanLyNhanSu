using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Users;
using FEQuanLyNhanSu.Services.UserService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
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
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;
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
            Application.Current.Shutdown();
        }
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;
            loadingBar.Visibility = Visibility.Visible;
            txtLoading.Visibility = Visibility.Visible;

            try
            {
                string username = txtUsername.Text?.Trim();
                string password;

                if (txtPassword.Visibility != Visibility.Visible)
                {
                    password = txtPasswordVisible.Text.Trim();
                }
                else
                {
                    password = txtPassword.Password.Trim();
                }

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var loginDto = new
                {
                    Username = username,
                    Password = password
                };

                var json = JsonConvert.SerializeObject(loginDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var apiUrl = $"{baseUrl}/api/Auth/login";

                using var client = new HttpClient();
                var response = await client.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var apiError = JsonConvert.DeserializeObject<ApiResponse<string>>(responseJson);
                    MessageBox.Show($"Đăng nhập thất bại: {apiError?.Data ?? apiError?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var responseStr = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResult>>(responseStr);
                var loginData = apiResponse?.Data;

                if (loginData == null)
                {
                    MessageBox.Show("Đăng nhập thất bại: không nhận được dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Application.Current.Properties["Token"] = loginData.AccessToken;
                Application.Current.Properties["RefreshToken"] = loginData.RefreshToken;

                if (loginData.isResetPassword)
                {
                    MessageBox.Show("Mật khẩu đã được reset. Vui lòng đổi mật khẩu trước khi tiếp tục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    var changePassWindow = new ChangePass(); 
                    bool? result = changePassWindow.ShowDialog();

                    if (result != true)
                    {
                        MessageBox.Show("Bạn chưa đổi mật khẩu. Vui lòng đổi mật khẩu để tiếp tục.", "Thông báo");
                        return;
                    }
                }

                var userInfoUrl = $"{baseUrl}/api/Auth/current";
                var request = new HttpRequestMessage(HttpMethod.Get, userInfoUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginData.AccessToken);

                var userResponse = await client.SendAsync(request);
                if (!userResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Không thể lấy thông tin người dùng!", "Lỗi");
                    return;
                }

                var userJson = await userResponse.Content.ReadAsStringAsync();
                dynamic userData = JsonConvert.DeserializeObject(userJson);

                Application.Current.Properties["Fullname"] = userData.data.fullname;
                Application.Current.Properties["UserRole"] = userData.data.role;
                Application.Current.Properties["UserId"] = userData.data.userId;

                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi");
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
                loadingBar.Visibility = Visibility.Collapsed;
                txtLoading.Visibility = Visibility.Collapsed;
            }
        }


        public static async Task<HttpResponseMessage> SendAuthorizedRequestAsync(HttpRequestMessage request)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                bool refreshOk = await RefreshTokenAsync();
                if (refreshOk)
                {
                    var newToken = Application.Current.Properties["Token"]?.ToString();
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                    response = await client.SendAsync(request);
                }
                else
                {
                    MessageBox.Show("Phiên đăng nhập hết hạn, vui lòng đăng nhập lại!");
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
                Application.Current.Properties["Token"] = result.Data.AccessToken;
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
