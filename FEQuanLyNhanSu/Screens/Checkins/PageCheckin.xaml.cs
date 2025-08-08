using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Checkins;
using FEQuanLyNhanSu.Screens.Payrolls;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Services;
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
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Payrolls;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption5.xaml
    /// </summary>
    public partial class PageCheckin : Page
    {
        private PaginationHelper<Checkins.UserWithCheckinsDto> _paginationHelper;
        public ObservableCollection<UserWithCheckinsDto> UserList { get; set; } = new();
        public PageCheckin()
        {
            InitializeComponent();
            CheckinDtaGrid.ItemsSource = UserList;
            //LoadDateComboboxes();
            //HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            //Loaded += async (s, e) => await FilterAsync();
            Loaded += PageUser_Loaded;
        }

        private async void PageUser_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDateComboboxes();

            await HandleUI(Application.Current.Properties["UserRole"]?.ToString());

            await Task.Delay(200);

            await FilterAsync(); 
        }

        private async Task HandleUI(string role)
        {
            switch (role)
            {
                case "SystemAdmin":
                    AddCheckinBtn.Visibility = Visibility.Collapsed;
                    AddCheckouBtn.Visibility = Visibility.Collapsed;

                    await LoadCompanies();
                    await LoadDepartmentByCompanyAsync();
                    await LoadPositionsByDepartmentAsync();
                    break;
                case "Administrator":
                    cbCompany.Visibility = Visibility.Collapsed;;
                    await LoadDepartments();
                    await LoadPositionsByDepartmentAsync();
                    break;
                case "Manager":
                    cbCompany.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    await LoadPositions();
                    break;
                case "Employee":
                    lblTitle.Text = "Checkin";
                    cbCompany.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    cbPosition.Visibility = Visibility.Collapsed;
                    break;
  
            }
        }
        private void LoadDateComboboxes()
        {
            var days = new List<string> { "Ngày" };
            days.AddRange(Enumerable.Range(1, 31).Select(i => i.ToString()));
            cbDay.ItemsSource = days;

            var months = new List<string> { "Tháng" };
            months.AddRange(Enumerable.Range(1, 12).Select(i => i.ToString()));
            cbMonth.ItemsSource = months;

            int currentYear = DateTime.Now.Year;
            var years = new List<string> { "Năm" };
            var yearList = Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse().ToList();
            years.AddRange(yearList);
            cbYear.ItemsSource = years;

            cbDay.SelectedIndex = DateTime.Now.Day;
            cbMonth.SelectedIndex = DateTime.Now.Month;
            cbYear.SelectedIndex = years.IndexOf(currentYear.ToString());
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

            //using var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization =
            //    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            //var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;

                //if (result.Data.Items != null && result.Data.Items.Any())
                //{
                //    cbDepartment.SelectedItem = result.Data.Items.First();
                //    await LoadPositionsByDepartmentAsync();
                //    await FilterAsync();
                //}
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

        private async Task LoadUserWithCheckin()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/users-checkins";
            int pageSize = 10;

            _paginationHelper = new PaginationHelper<UserWithCheckinsDto>(
                baseUrl,
                pageSize,
                token,
                items => CheckinDtaGrid.ItemsSource = items,
                txtPage,
                page => BuildCheckinUrlWithFilter(page, pageSize)
            );

            await _paginationHelper.LoadPageAsync(1);
        }
        private string BuildCheckinUrlWithFilter(int pageIndex, int pageSize)
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/users-checkins";
            var parameters = new List<string>();

            string keyword = txtSearch.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
                parameters.Add($"Search={Uri.EscapeDataString(keyword)}");

            if (cbDay.SelectedIndex > 0)
                parameters.Add($"Day={cbDay.SelectedItem}");

            if (cbMonth.SelectedIndex > 0)
                parameters.Add($"Month={cbMonth.SelectedItem}");

            if (cbYear.SelectedIndex > 0)
                parameters.Add($"Year={cbYear.SelectedItem}");

            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                parameters.Add($"companyId={selectedComp.CompanyId}");

            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                parameters.Add($"departmentId={selectedDept.DepartmentId}");

            if (cbPosition.SelectedItem is PositionResultDto selectedPos)
                parameters.Add($"positionId={selectedPos.Id}");

            parameters.Add($"pageIndex={pageIndex}");
            parameters.Add($"pageSize={pageSize}");

            return parameters.Any() ? $"{baseUrl}?{string.Join("&", parameters)}" : baseUrl;
        }
        private async Task FilterAsync()
        {
            await LoadUserWithCheckin();
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
                    await LoadCompanies();               // load lại toàn bộ
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
                Guid? companyId = null;

                if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                    departmentId = selectedDept.DepartmentId;

                if (cbCompany.SelectedItem is CompanyResultDto selectedCompany)
                    companyId = selectedCompany.CompanyId;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string url;

                if (string.IsNullOrEmpty(keyword))
                {
                    if (departmentId.HasValue)
                    {
                        url = $"{baseUrl}?departmentId={departmentId.Value}";
                    }
                    else if (companyId.HasValue)
                    {
                        url = $"{baseUrl}?companyId={companyId.Value}";
                    }
                    else
                    {
                        url = baseUrl;
                    }
                }
                else
                {
                    url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";

                    if (departmentId.HasValue)
                        url += $"&departmentId={departmentId.Value}";
                    else if (companyId.HasValue)
                        url += $"&companyId={companyId.Value}";
                }

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
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();

        private async void OnCheckinCreated(Checkins.CheckinResultDto newCheckin)
        {
            if (newCheckin == null) return;

            var currentUserRole = Application.Current.Properties["Role"]?.ToString();
            var currentUserIdStr = Application.Current.Properties["UserId"]?.ToString();
            if ((currentUserRole == "Admin" || currentUserRole == "Manager")
                && Guid.TryParse(currentUserIdStr, out Guid currentUserId)
                && newCheckin.UserId != currentUserId)
            {
                newCheckin.DeviceInfo = null;
            }

            var list = CheckinDtaGrid.ItemsSource as List<Checkins.UserWithCheckinsDto>;
            if (list == null) return;

            var user = list.FirstOrDefault(u => u.UserId == newCheckin.UserId);

            var token = Application.Current.Properties["Token"]?.ToString();
            if (string.IsNullOrEmpty(token)) return;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var response = await httpClient.GetAsync($"{baseUrl}/api/User/{newCheckin.UserId}");
            if (!response.IsSuccessStatusCode) return;

            var resultJson = await response.Content.ReadAsStringAsync();
            var userResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponse>(resultJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (userResponse?.Data == null) return;

            var userData = userResponse.Data;

            if (user == null)
            {
                user = new Checkins.UserWithCheckinsDto
                {
                    UserId = userData.UserId,
                    FullName = userData.Fullname,
                    PhoneNumber = userData.PhoneNumber,
                    Address = userData.Address,
                    ImageUrl = userData.ImageUrl,
                    Checkins = new ObservableCollection<Checkins.CheckinResultDto>()
                };
                list.Add(user);
            }

            var existing = user.Checkins.FirstOrDefault(p => p.CheckinTime.Day == newCheckin.CheckinTime.Day && p.CheckinTime.Month == newCheckin.CheckinTime.Month && p.CheckinTime.Year == newCheckin.CheckinTime.Year);

            if (existing != null)
                user.Checkins.Remove(existing);

            user.Checkins.Insert(0, newCheckin);
            list.Remove(user);
            list.Insert(0, user);

            CheckinDtaGrid.ItemsSource = null;
            CheckinDtaGrid.ItemsSource = list;
        }
        private async void OnCheckinUpdated(Checkins.CheckinResultDto newCheckin)
        {
            if (newCheckin == null) return;

            var list = CheckinDtaGrid.ItemsSource as List<Checkins.UserWithCheckinsDto>;
            if (list == null) return;

            var user = list.FirstOrDefault(u => u.UserId == newCheckin.UserId);

            var token = Application.Current.Properties["Token"]?.ToString();
            if (string.IsNullOrEmpty(token)) return;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var response = await httpClient.GetAsync($"{baseUrl}/api/User/{newCheckin.UserId}");
            if (!response.IsSuccessStatusCode) return;

            var resultJson = await response.Content.ReadAsStringAsync();
            var userResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponse>(resultJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (userResponse?.Data == null) return;

            var userData = userResponse.Data;

            if (user == null)
            {
                user = new Checkins.UserWithCheckinsDto
                {
                    UserId = userData.UserId,
                    FullName = userData.Fullname,
                    PhoneNumber = userData.PhoneNumber,
                    Address = userData.Address,
                    ImageUrl = userData.ImageUrl,
                    Checkins = new ObservableCollection<Checkins.CheckinResultDto>()
                };
                list.Add(user);
            }

            var existing = user.Checkins.FirstOrDefault(p => p.CheckinTime.Day == newCheckin.CheckinTime.Day && p.CheckinTime.Month == newCheckin.CheckinTime.Month && p.CheckinTime.Year == newCheckin.CheckinTime.Year);

            if (existing != null)
                user.Checkins.Remove(existing);

            user.Checkins.Insert(0, newCheckin);
            list.Remove(user);
            list.Insert(0, user);

            CheckinDtaGrid.ItemsSource = null;
            CheckinDtaGrid.ItemsSource = list;
        }
        private void AddCheckinBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new Checkin(OnCheckinCreated);
            window.ShowDialog();
        }
        private void AddCheckouBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new Checkout(OnCheckinUpdated);
            window.ShowDialog();
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid checkinId)
            {
                var editWindow = new Update(OnCheckinUpdated, checkinId);
                editWindow.ShowDialog();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid checkinId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa checkin này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteCheckinAsync(checkinId);
                }
            }
        }
        private async Task DeleteCheckinAsync(Guid checkinId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + $"/api/Checkin/{checkinId}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa checkin thành công.");
                //LoadCheckin();
                await FilterAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Xóa checkin thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();
        }
        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }
        private void DtaGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dep = (DependencyObject)e.OriginalSource;

            // Nếu click vào Button (hoặc phần tử con của Button), thì bỏ qua để nút hoạt động bình thường
            while (dep != null)
            {
                if (dep is Button)
                    return;
                dep = VisualTreeHelper.GetParent(dep);
            }

            dep = (DependencyObject)e.OriginalSource;

            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is DataGridRow row)
            {
                var item = row.Item;

                if (CheckinDtaGrid.SelectedItems.Contains(item))
                {
                    CheckinDtaGrid.SelectedItems.Remove(item); // Ẩn details
                }
                else
                {
                    CheckinDtaGrid.SelectedItems.Add(item); // Hiện details
                }

                e.Handled = true; // Ngăn chọn lại dòng
            }
        }
    }
}
