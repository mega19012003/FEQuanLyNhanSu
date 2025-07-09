using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Users
{
    /// <summary>
    /// Interaction logic for Createuser.xaml
    /// </summary>
    public partial class CreateUser : Window
    {
        private Action<UserResultDto> _onCreated;
        public CreateUser(Action<UserResultDto> onCreated)
        {
            InitializeComponent();
            _onCreated = onCreated;
            LoadRoles();
        }

        private void LoadRoles()
        {
            var roles = Enum.GetNames(typeof(RoleType)).ToList();
            cmbRole.ItemsSource = roles;
        }


        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var selectedRole = cmbRole.SelectedItem as string;
            if (!Enum.TryParse<RoleType>(selectedRole, out var roleEnum))
            {
                MessageBox.Show("Vai trò không hợp lệ");
                return;
            }
            if (string.IsNullOrEmpty(txtFullname.Text) || string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text) || cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                return;
            }

            var newUser = new
            {
                Fullname = txtFullname.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                Role = (int)roleEnum
            };

            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Auth/register";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
            
            var response = client.PostAsync(baseUrl, content).Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    MessageBox.Show("Tạo người dùng thành công!");
            //    this.Close();
            //}
            //else
            //{
            //    var errorContent = response.Content.ReadAsStringAsync().Result;
            //    MessageBox.Show($"Không thể tạo người dùng. Lỗi: {errorContent}");
            //}
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data != null)
                {
                    _onCreated?.Invoke(apiResponse.Data);
                }

                MessageBox.Show("Tạo người dùng thành công.");
                this.Close();
            }
            else
            {
                var apiResponses = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponses?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Tạo người dùng thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
