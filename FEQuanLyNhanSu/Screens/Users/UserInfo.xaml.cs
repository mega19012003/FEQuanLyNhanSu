using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Users
{
    /// <summary>
    /// Interaction logic for UserInfo.xaml
    /// </summary>
    public partial class UserInfo : Window
    {
        public UserInfo()
        {
            InitializeComponent();
            _ = LoadCurrentUserAsync();
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Auth/current";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(baseUrl);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserResultDto>>(json);
                    var user = apiResponse?.Data;

                    if (user != null)
                    {
                        txtFullname.Text = user.Fullname;
                        txtRole.Text = user.RoleName;
                        txtPhoneNo.Text = user.PhoneNumber;
                        txtAddress.Text = user.Address;
                        txtDepartment.Text = user.DepartmentName;
                        txtPosition.Text = user.PositionName;

                        if (!string.IsNullOrEmpty((string)user.ImageUrl))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri((string)user.ImageUrl, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            imgAvatar.Source = bitmap;
                        }
                        else
                        {
                            imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/user.png"));
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Không thể tải thông tin người dùng: {json}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Auth/logout";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.PostAsync(baseUrl, null);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Application.Current.Properties.Remove("Token");

                    foreach (Window win in Application.Current.Windows)
                    {
                        if (win != this)
                            win.Close();
                    }

                    var loginWindow = new Login();
                    loginWindow.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Logout thất bại: {json}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi logout: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }

        private void btnChangePass_Click(object sender, RoutedEventArgs e)
        {
            var changePassWindow = new ChangePass();
            changePassWindow.Show();
        }
    }
}
