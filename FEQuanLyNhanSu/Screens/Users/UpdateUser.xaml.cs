using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Users;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using System.Xml.Linq;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Users
{
    /// <summary>
    /// Interaction logic for UpdateCreate.xaml
    /// </summary>
    public partial class UpdateUser : Window
    {
        private Guid _userId;
        private Action _reloadAction;
        private string _imagePath;
        public UpdateUser(Guid userId, Action reloadAction)
        {
            var role = Application.Current.Properties["UserRole"]?.ToString();
            InitializeComponent();
            HandleUI(role);
            LoadDepartments();
            LoadPositions();
            LoadRoles();
            _userId = userId;
            _reloadAction = reloadAction;
            _ = LoadUserAsync();
        }

        private void HandleUI(string role)
        {
            switch(role)
            {
                case "Administrator":
                    break;
                case "Manager":
                    cmbRole.Visibility = Visibility.Collapsed;
                    txtboxRole.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    txtboxDepartment.Visibility = Visibility.Collapsed;
                    break;

            }
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private void LoadRoles()
        {
            var roles = Enum.GetNames(typeof(RoleType)).ToList();
            cmbRole.ItemsSource = roles;
        }
        private void LoadDepartments()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;
            }
        }
        private void LoadPositions()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionDTO>>>(json);
                cbPosition.ItemsSource = result.Data.Items;
            }
        }
        private async void cbDepartment_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox?.Text == null) return; // nếu Text là null thì thoát luôn, tránh crash

                var keyword = comboBox.Text.Trim();
                if (keyword == "") return; // trống thì không gọi API

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                    cbDepartment.ItemsSource = result.Data.Items;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    cbDepartment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbPosition_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox?.Text == null) return; // nếu Text là null thì thoát luôn, tránh crash

                var keyword = comboBox.Text.Trim();
                if (keyword == "") return; // trống thì không gọi API

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionDTO>>>(json);
                    cbPosition.ItemsSource = result.Data.Items;
                    cbPosition.IsDropDownOpen = true;
                }
                else
                {
                    cbPosition.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadUserAsync()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/User/{_userId}";

                using var client = CreateAuthorizedClient(token);
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<UserResultDto>>(json);
                    var user = result.Data;

                    txtFullname.Text = user.Fullname;
                    txtAddress.Text = user.Address;
                    txtPhoneNo.Text = user.PhoneNumber;
                    cmbRole.Text = user.RoleName;
                    cbDepartment.Text = user.DepartmentName;
                    cbPosition.Text = user.PositionName;
                    txtSalary.Text = user.BasicSalary.ToString();
                    txtImage.Text = user.ImageUrl;

                    if (!string.IsNullOrEmpty(user.ImageUrl))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(user.ImageUrl, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        imgAvatar.Source = bitmap;
                    }
                    else
                    {
                        imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/user.png"));
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Không thể tải thông tin người dùng. Lỗi: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }


        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Chọn ảnh";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                txtImage.Text = System.IO.Path.GetFileName(dialog.FileName);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imgAvatar.Source = bitmap;
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnUpdate.IsEnabled = false;

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

                var selectedeDepartment = cbDepartment.SelectedItem as DepartmentResultDto;
                var selectedPosition = cbPosition.SelectedItem as PositionDTO;
                var selectedRole = cmbRole.SelectedItem as string;

                if (!Enum.TryParse(selectedRole, out RoleType roleType))
                {
                    MessageBox.Show("Vui lòng chọn vai trò hợp lệ.");
                    return;
                }


                var formData = new MultipartFormDataContent
                {
                    {new StringContent(_userId.ToString()), "UserId" },
                    {new StringContent(txtFullname.Text), "Fullname" },
                    {new StringContent(txtPhoneNo.Text), "PhoneNumber" },
                    {new StringContent(txtAddress.Text), "Address" },
                    {new StringContent(roleType.ToString()), "Role" },
                    {new StringContent(txtSalary.Text), "BasicSalary" },
                    {new StringContent(txtImage.Text), "ImageUrl" },
                };

                if (!string.IsNullOrEmpty(_imagePath) && System.IO.File.Exists(_imagePath))
                {
                    var filestream = File.OpenRead(_imagePath);
                    var fileContent = new StreamContent(filestream);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    formData.Add(fileContent, "ImageUrl", System.IO.Path.GetFileName(_imagePath));
                }


                if (selectedeDepartment?.DepartmentId != null)
                    formData.Add(new StringContent(selectedeDepartment.DepartmentId.ToString()), "DepartmentId");

                if (selectedPosition?.Id != null)
                    formData.Add(new StringContent(selectedPosition.Id.ToString()), "PositionId");
                using var client = CreateAuthorizedClient(token);
                var response = await client.PutAsync($"{baseUrl}", formData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thông tin người dùng thành công.");
                    _reloadAction?.Invoke();
                    this.Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Không thể cập nhật thông tin người dùng. Lỗi: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
