using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Screens.Users;
using FEQuanLyNhanSu.Services.UserService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class PageUser : Page
    {
        private ObservableCollection<UserResultDto> users = new();
        private PaginationHelper<UserResultDto> _paginationHelper;

        public PageUser()
        {
            InitializeComponent();
            _ = LoadUser();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            //_ = LoadPositionsByDepartmentAsync();
            //_ = FilterAsync();
            //_ = LoadCompanies();
            //_ = LoadDepartmentByCompanyAsync();
            //_ = LoadPositionsByDepartmentAsync();
            LoadDateComboboxes();
            LoadIsActiveComboBox();
            Loaded += async (s, e) => await FilterAsync();
        }

        private async Task HandleUI(string role)
        {
            switch (role)
            {
                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    cbCompany.Visibility = Visibility.Collapsed;
                    await LoadPositions();
                    break;

                case "Administrator":
                    await LoadDepartments();
                    await LoadPositionsByDepartmentAsync();
                    cbCompany.Visibility = Visibility.Collapsed;
                  
                    break;
                case "SystemAdmin":
                    await LoadCompanies();
                    await LoadDepartmentByCompanyAsync();
                    await LoadPositionsByDepartmentAsync();
                    break;
            }
        }
        
        private void LoadDateComboboxes()
        {

            var months = new List<string> { "Tháng" };
            months.AddRange(Enumerable.Range(1, 12).Select(i => i.ToString()));
            cbMonth.ItemsSource = months;

            int currentYear = DateTime.Now.Year;
            var years = new List<string> { "Năm" };
            years.AddRange(Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse());

            cbMonth.SelectedIndex = DateTime.Now.Month;
            
        }
        private void LoadIsActiveComboBox()
        {
            cbIsActive.Items.Clear();

            cbIsActive.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = "" });
            cbIsActive.Items.Add(new ComboBoxItem { Content = "Đang hoạt động", Tag = "true" });
            cbIsActive.Items.Add(new ComboBoxItem { Content = "Không hoạt động", Tag = "false" });

            cbIsActive.SelectedIndex = 1; 
        }

        private async Task LoadUser()
        {

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";
            int pageSize = 10;

            _paginationHelper = new PaginationHelper<UserResultDto>(
                baseUrl,
                pageSize,
                token,
                items => UserDtaGrid.ItemsSource = items,
                txtPage,
                page => BuildUserUrlWithFilter(page, pageSize)
            );

            _ = _paginationHelper.LoadPageAsync(1);
        }
        private string BuildUserUrlWithFilter(int pageIndex, int pageSize)
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";
            var parameters = new List<string>();

            string keyword = txtSearch.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
                parameters.Add($"Search={Uri.EscapeDataString(keyword)}");

            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                parameters.Add($"companyId={selectedComp.CompanyId}");

            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                parameters.Add($"departmentId={selectedDept.DepartmentId}");

            if (cbPosition.SelectedItem is PositionResultDto selectedPos)
                parameters.Add($"positionId={selectedPos.Id}");

            if (cbMonth.SelectedIndex > 0 && int.TryParse(cbMonth.SelectedItem.ToString(), out int selectedMonth))
                parameters.Add($"Month={selectedMonth}");

            if (cbIsActive.SelectedItem is ComboBoxItem selectedStatus)
            {
                var tagValue = selectedStatus.Tag?.ToString();
                if (!string.IsNullOrEmpty(tagValue))
                    parameters.Add($"IsActive={tagValue.ToLower()}");
            }

            parameters.Add($"pageIndex={pageIndex}");
            parameters.Add($"pageSize={pageSize}");

            return parameters.Any() ? $"{baseUrl}?{string.Join("&", parameters)}" : baseUrl;
        }
        private async Task FilterAsync()
        {
            await LoadUser();
        }

        public static async Task<List<UserResultDto>> LoadAllUsersAsync(string baseUrl, string token, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var url = $"{baseUrl}/api/User?pageIndex={pageIndex}&pageSize={pageSize}";

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Lỗi khi load người dùng: {response.StatusCode}");
                    return new List<UserResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<UserResultDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Items?.ToList() ?? new List<UserResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<UserResultDto>();
            }
        }
        private async Task LoadCompanies()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<CompanyResultDto>>>(json);
                cbCompany.ItemsSource = result.Data.Items;

                if (result.Data.Items != null && result.Data.Items.Any())
                {
                    cbCompany.SelectedItem = result.Data.Items.First();
                    await LoadDepartmentByCompanyAsync();
                    await LoadPositionByCompanyAsync();
                    await FilterAsync();
                }
            }
        }
        private async Task LoadDepartments()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;

                if (result.Data.Items != null && result.Data.Items.Any())
                {
                    cbDepartment.SelectedItem = result.Data.Items.First();
                    await FilterAsync();
                }
            }
        }
        private async Task LoadDepartmentsByCompanyId(Guid? companyId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

            if (companyId.HasValue)
                baseUrl += $"?companyId={companyId.Value}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;

                if (result.Data.Items != null && result.Data.Items.Any())
                {
                    cbDepartment.SelectedItem = result.Data.Items.First();
                    await FilterAsync();
                }
            }
        }
        private async Task LoadPositions()
        {
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                    cbPosition.ItemsSource = result.Data.Items;
                    if (result.Data.Items != null && result.Data.Items.Any())
                    {
                        cbPosition.SelectedItem = result.Data.Items.First();
                        await FilterAsync();
                    }
                }
            }
        }
      
        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbIsActive_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            await FilterAsync();
        }
        private async void cbCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadDepartmentByCompanyAsync();
            await LoadPositionByCompanyAsync();
            await FilterAsync();
        }
        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDepartment.SelectedItem == null)
                await LoadPositionByCompanyAsync();
            else await LoadPositionsByDepartmentAsync();
            await FilterAsync();
        }
        private async void cbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await FilterAsync();
        }
        
        private async Task LoadPositionsByDepartmentAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

            Guid? departmentId = null;
            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                departmentId = selectedDept.DepartmentId;

            string url = baseUrl;
            if (departmentId.HasValue)
            {
                url += $"?departmentId={departmentId.Value}";
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                cbPosition.ItemsSource = result?.Data?.Items;
                if (result?.Data?.Items?.Any() == true)
                    cbPosition.SelectedItem = result.Data.Items.First();
            }
            else
            {
                cbPosition.ItemsSource = null;
            }
        }
        private async Task LoadDepartmentByCompanyAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

            Guid? companyId = null;
            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                companyId = selectedComp.CompanyId;

            string url = baseUrl;
            if (companyId.HasValue)
            {
                url += $"?companyId={companyId.Value}";
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result?.Data?.Items;
                if (result?.Data?.Items?.Any() == true)
                    cbDepartment.SelectedItem = result.Data.Items.First();
            }
            else
            {
                cbDepartment.ItemsSource = null;
            }
        }
        private async Task LoadPositionByCompanyAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

            Guid? companyId = null;
            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                companyId = selectedComp.CompanyId;

            string url = baseUrl;
            if (companyId.HasValue)
            {
                url += $"?companyId={companyId.Value}";
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                cbPosition.ItemsSource = result?.Data?.Items;
                if (result?.Data?.Items?.Any() == true)
                    cbPosition.SelectedItem = result.Data.Items.First();
            }
            else
            {
                cbPosition.ItemsSource = null;
            }
        }      
        
        private async void cbCompany_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompany.Text.Trim();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();               
                    cbCompany.SelectedItem = null;
                    cbCompany.IsDropDownOpen = true;
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<CompanyResultDto>>>(json);
                        cbCompany.ItemsSource = result?.Data?.Items;
                        cbCompany.SelectedItem = null;
                        cbCompany.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbCompany.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbDepartment_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbDepartment.Text.Trim();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    Guid? companyId = null;

                    if (cbCompany.SelectedItem is CompanyResultDto selectedCompany)
                        companyId = selectedCompany.CompanyId;

                    await LoadDepartmentsByCompanyId(companyId);
                    cbDepartment.SelectedItem = null;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";

                    if (cbCompany.SelectedItem is CompanyResultDto selectedCompany)
                        url += $"&companyId={selectedCompany.CompanyId}";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                        cbDepartment.ItemsSource = result?.Data?.Items;
                        cbDepartment.SelectedItem = null;
                        cbDepartment.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbDepartment.ItemsSource = null;
                    }
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
                var keyword = cbPosition.Text.Trim();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                Guid? departmentId = null;
                if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                    departmentId = selectedDept.DepartmentId;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    if (!departmentId.HasValue)
                    {
                        cbPosition.ItemsSource = null;
                        cbPosition.SelectedItem = null;
                        cbPosition.IsDropDownOpen = false;
                        return; 
                    }

                    string url = $"{baseUrl}?departmentId={departmentId.Value}";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                        cbPosition.ItemsSource = result?.Data?.Items;
                        cbPosition.SelectedItem = null;
                        cbPosition.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbPosition.ItemsSource = null;
                    }
                }
                else
                {
                    string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";
                    if (departmentId.HasValue)
                        url += $"&departmentId={departmentId.Value}";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                        cbPosition.ItemsSource = result?.Data?.Items;
                        cbPosition.SelectedItem = null;
                        cbPosition.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbPosition.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnUserCreated(Users.UserResultDto newDept)
        {
            if (newDept != null)
            {
                var list = UserDtaGrid.ItemsSource as List<Users.UserResultDto> ?? new List<Users.UserResultDto>();
                list.Insert(0, newDept);
                UserDtaGrid.ItemsSource = null;
                UserDtaGrid.ItemsSource = list;

                UserDtaGrid.SelectedItem = newDept;
                UserDtaGrid.ScrollIntoView(newDept);
            }
        }
        private void OnUserUpdated(Users.UserResultDto updatedUser)
        {
            if (updatedUser != null)
            {
                var list = UserDtaGrid.ItemsSource as List<Users.UserResultDto> ?? new List<Users.UserResultDto>();

                var existing = list.FirstOrDefault(d => d.UserId == updatedUser.UserId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedUser);

                UserDtaGrid.ItemsSource = null;
                UserDtaGrid.ItemsSource = list;

                UserDtaGrid.SelectedItem = updatedUser;
                UserDtaGrid.ScrollIntoView(updatedUser);
            }
        }
        /// Create
        private void CreateUser(object sender, RoutedEventArgs e)
        {
            var window = new CreateUser(OnUserCreated);
            window.Show();
        }
        /// Update
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid userId)
            {
                var editWindow = new UpdateUser(userId, OnUserUpdated);
                editWindow.ShowDialog();
            }
        }

        // Reset password
        private void btnResetPass_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid userId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn đặt lại mật khẩu cho người dùng này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = ResetPasswordAsync(userId);
                }
            }
        }
        
        private async Task ResetPasswordAsync(Guid userId)
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Auth/reset-password";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(userId.ToString()), "id");

                    var response = await client.PutAsync(baseUrl, form);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Đặt lại mật khẩu thành công. Pass là username");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(error);
                        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                        MessageBox.Show($"Lỗi khi đặt lại mật khẩu: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đặt lại mật khẩu: {ex.Message}");
            }
        }

        /// Delete 
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa chức vụ này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteUserAsync(positionId);
                }
            }
        }
        private async Task DeleteUserAsync(Guid userId)
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/" + userId;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.DeleteAsync("");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Xóa người dùng thành công.");
                        //LoadUser();
                        await FilterAsync();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(error);
                        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                        MessageBox.Show($"Lỗi khi xóa người dùng: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa người dùng: {ex.Message}");
            }
        }

        /// Pagination
        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }
        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();
        }
    }
}
