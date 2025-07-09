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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Users
{
    public partial class UpdateUser : Window
    {
        private Guid _userId;
        private Action<UserResultDto> _onUpdated;
        private string _imagePath;

        public UpdateUser(Guid userId, Action<UserResultDto> onUpdated)
        {
            InitializeComponent();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            HandleUI(role);
            LoadRoles();
            _userId = userId;
            _onUpdated = onUpdated;

            _ = InitDataAsync(); 
        }

        private async Task InitDataAsync()
        {
            var role = Application.Current.Properties["UserRole"]?.ToString();
            if (role == "Manager")
            {
                await LoadAllPositions(); // Load toàn bộ positions cho Manager
            }
            else
            {
                await LoadDepartments();
            }
            await LoadUserAsync();
        }

        private void HandleUI(string role)
        {
            if (role == "Manager")
            {
                cmbRole.Visibility = Visibility.Collapsed;
                txtboxRole.Visibility = Visibility.Collapsed;
                cbDepartment.Visibility = Visibility.Collapsed;
                txtboxDepartment.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadRoles()
        {
            var roles = Enum.GetNames(typeof(RoleType)).ToList();
            cmbRole.ItemsSource = roles;
        }

        private async Task LoadDepartments()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;
            }
            else
            {
                cbDepartment.ItemsSource = null;
            }
        }

        private async Task LoadAllPositions()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(baseUrl); 

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                    cbPosition.ItemsSource = result.Data.Items;
                }
                else
                {
                    cbPosition.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load positions: {ex.Message}");
            }
        }

        private async Task LoadPositionsByDepartmentId(Guid departmentId)
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/Position?departmentId={departmentId}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                    cbPosition.ItemsSource = result.Data.Items;
                }
                else
                {
                    cbPosition.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load positions: {ex.Message}");
            }
        }

        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
            {
                await LoadPositionsByDepartmentId(selectedDept.DepartmentId);
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
                    var result = JsonConvert.DeserializeObject<ApiResponse<UserResultUpdateDto>>(json);
                    var user = result.Data;

                    txtFullname.Text = user.Fullname;
                    txtAddress.Text = user.Address;
                    txtPhoneNo.Text = user.PhoneNumber;
                    cmbRole.Text = user.RoleName;
                    txtSalary.Text = user.BasicSalary?.ToString();
                    txtImage.Text = user.ImageUrl;
                    chkIsActive.IsChecked = user.IsActive;

                    if (user.DepartmentId != null)
                    {
                        cbDepartment.SelectedValue = user.DepartmentId;
                        await LoadPositionsByDepartmentId(user.DepartmentId.Value);
                        cbPosition.SelectedValue = user.PositionId;
                    }
                    else
                    {
                        cbDepartment.Text = user.DepartmentName ?? "";
                        cbPosition.Text = user.PositionName ?? "";
                    }

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
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                    var errorMessage = apiResponse?.Data?.ToString() ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Không thể tải thông tin người dùng. Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Chọn ảnh", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
            if (dialog.ShowDialog() == true)
            {
                txtImage.Text = Path.GetFileName(dialog.FileName);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgAvatar.Source = bitmap;

                _imagePath = dialog.FileName; 
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            this.Close();
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnUpdate.IsEnabled = false;

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

                var selectedDepartment = cbDepartment.SelectedItem as DepartmentResultDto;
                var selectedPosition = cbPosition.SelectedItem as PositionResultDto;
                var selectedRole = cmbRole.SelectedItem as string;

                if (!Enum.TryParse(selectedRole, out RoleType roleType))
                {
                    MessageBox.Show("Vui lòng chọn vai trò hợp lệ.");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(txtSalary.Text))
                {
                    if (!int.TryParse(txtSalary.Text, out var salary) || salary < 0)
                    {
                        MessageBox.Show("Vui lòng nhập lương cơ bản hợp lệ.");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtPhoneNo.Text))
                {
                    if (txtPhoneNo.Text.Length < 10 || txtPhoneNo.Text.Length > 11 || !txtPhoneNo.Text.All(char.IsDigit))
                    {
                        MessageBox.Show("Số điện thoại phải có độ dài từ 10 đến 11 ký tự và chỉ chứa số.");
                        return;
                    }
                }


                var formData = new MultipartFormDataContent
                {
                    { new StringContent(_userId.ToString()), "UserId" },
                    { new StringContent(txtFullname.Text), "Fullname" },
                    { new StringContent(txtPhoneNo.Text), "PhoneNumber" },
                    { new StringContent(txtAddress.Text), "Address" },
                    { new StringContent(roleType.ToString()), "Role" },
                    { new StringContent(txtSalary.Text), "BasicSalary" },
                    { new StringContent(txtImage.Text), "ImageUrl" },
                    { new StringContent(chkIsActive.IsChecked == true ? "true" : "false"), "IsActive" } 
                };

                if (selectedDepartment?.DepartmentId != null)
                    formData.Add(new StringContent(selectedDepartment.DepartmentId.ToString()), "DepartmentId");

                if (selectedPosition?.Id != null)
                    formData.Add(new StringContent(selectedPosition.Id.ToString()), "PositionId");

                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    var filestream = File.OpenRead(_imagePath);
                    var fileContent = new StreamContent(filestream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formData.Add(fileContent, "ImageUrl", Path.GetFileName(_imagePath));
                }

                using var client = CreateAuthorizedClient(token);
                var response = await client.PutAsync(baseUrl, formData);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserResultDto>>(json);
                    if (apiResponse?.Data != null)
                    {
                        MessageBox.Show("Cập nhật thông tin người dùng thành công.");
                        _onUpdated?.Invoke(apiResponse.Data);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thành công nhưng không nhận được dữ liệu người dùng.");
                    }
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
            finally
            {
                btnUpdate.IsEnabled = true;
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
                MessageBox.Show($"Lỗi khi tìm kiếm người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbPosition_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox?.Text == null) return;

                var keyword = comboBox.Text.Trim();
                if (keyword == "") return;

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                // 🟢 Lấy departmentId nếu có
                var selectedDepartment = cbDepartment.SelectedItem as DepartmentResultDto;
                string url;

                if (selectedDepartment != null && selectedDepartment.DepartmentId != Guid.Empty)
                {
                    url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}&departmentId={selectedDepartment.DepartmentId}";
                }
                else
                {
                    url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
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

    }
}
