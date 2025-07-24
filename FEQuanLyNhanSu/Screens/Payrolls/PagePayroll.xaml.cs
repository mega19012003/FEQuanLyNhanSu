using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Payrolls;
using FEQuanLyNhanSu.Services;
using Newtonsoft.Json;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Payrolls;
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PagePayroll.xaml
    /// </summary>
    public partial class PagePayroll : Page
    {

        private PaginationHelper<Payrolls.UserWithPayrollDto> _paginationHelper;
        public PagePayroll()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadDateComboboxes();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "SystemAdmin":
                    //DtaGridAction.Visibility = Visibility.Collapsed;
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    _ = LoadUserWithPayroll();
                    _ = LoadCompanies();
                    _ = LoadDepartmentByCompanyAsync();
                    _ = LoadPositionsByDepartmentAsync();
                    break;
                case "Administrator":
                    cbCompany.Visibility = Visibility.Collapsed;
                    _ = LoadUserWithPayroll();
                    _ = LoadDepartments();
                    _ = LoadPositionsByDepartmentAsync();
                    break; 
                case "Manager":
                    cbCompany.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    _ = LoadUserWithPayroll();
                    _ = LoadPositions();
                    break;
                case "Employee":
                    _ = LoadEmployeePayroll();
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    //DtaGridAction.Visibility = Visibility.Collapsed;
                    cbCompany.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Chấm công";
                    cbCompany.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    cbPosition.Visibility = Visibility.Collapsed;
                    //AddAllPayrollBtn.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private async Task LoadUserWithPayroll()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<UserWithPayrollDto>(
                baseUrl,
                pageSize,
                token,
                items => PayrollDtaGrid.ItemsSource = items,
                txtPage
            );

            await _paginationHelper.LoadPageAsync(1);
        }
        private async Task LoadEmployeePayroll()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<UserWithPayrollDto>(
                baseUrl,
                pageSize,
                token,
                items => PayrollDtaGrid.ItemsSource = items,
                txtPage
            );

            await _paginationHelper.LoadPageAsync(1);
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
                    //await LoadDepartmentByCompanyAsync();
                    //await LoadPositionByCompanyAsync();
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
        private async Task LoadPositions()
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

        private void LoadDateComboboxes()
        {
            //var days = new List<string> { "Ngày" };
            //days.AddRange(Enumerable.Range(1, 31).Select(i => i.ToString()));
            //cbDay.ItemsSource = days;

            var months = new List<string> { "Tháng" };
            months.AddRange(Enumerable.Range(1, 12).Select(i => i.ToString()));
            cbMonth.ItemsSource = months;

            int currentYear = DateTime.Now.Year;
            var years = new List<string> { "Năm" };
            years.AddRange(Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse());
            cbYear.ItemsSource = years;

            //cbDay.SelectedIndex = 0;
            //cbMonth.SelectedIndex = 0;
            //cbYear.SelectedIndex = 0;
            //cbDay.SelectedIndex = DateTime.Now.Day;
            cbMonth.SelectedIndex = DateTime.Now.Month;
            cbYear.SelectedIndex = years.IndexOf(currentYear.ToString());
        }
        
        private async Task FilterAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            string keyword = txtSearch.Text?.Trim();
            //int? day = cbDay.SelectedIndex > 0 ? int.Parse(cbDay.SelectedItem.ToString()) : (int?)null;
            int? month = cbMonth.SelectedIndex > 0 ? int.Parse(cbMonth.SelectedItem.ToString()) : (int?)null;
            int? year = cbYear.SelectedIndex > 0 ? int.Parse(cbYear.SelectedItem.ToString()) : (int?)null;

            Guid? companyId = null;
            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                companyId = selectedComp.CompanyId;

            Guid? departmentId = null;
            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                departmentId = selectedDept.DepartmentId;

            Guid? positionId = null;
            if (cbPosition.SelectedItem is PositionResultDto selectedPos)
                positionId = selectedPos.Id;

            var items = await SearchAndFilterCheckinsAsync(baseUrl, token, keyword, month, year, companyId, departmentId, positionId);
            PayrollDtaGrid.ItemsSource = items;
        }
        public static async Task<List<UserWithPayrollDto>> SearchAndFilterCheckinsAsync(string baseUrl, string token, string searchKeyword, int? month, int? year, Guid? companyId, Guid? departmentId, Guid? positionId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                //if (day.HasValue) parameters.Add($"Day={day}");
                if (month.HasValue) parameters.Add($"Month={month}");
                if (year.HasValue) parameters.Add($"Year={year}");
                if (companyId.HasValue) parameters.Add($"companyId={companyId}");
                if (departmentId.HasValue) parameters.Add($"departmentId={departmentId}");
                if (positionId.HasValue) parameters.Add($"positionId={positionId}");
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/Payroll/user-payrolls";
                if (parameters.Any()) url += "?" + string.Join("&", parameters);
           
                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Lỗi khi lọc payrol: {response.StatusCode}");
                    return new List<UserWithPayrollDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<UserWithPayrollDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Items?.ToList() ?? new List<UserWithPayrollDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<UserWithPayrollDto>();
            }
        }

        private async void cbCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadDepartmentByCompanyAsync();
            await LoadPositionByCompanyAsync();
            await FilterAsync();
        }
        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadPositionsByDepartmentAsync();
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
        private async void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();

        private void OnPayrollCreated(Payrolls.PayrollResultDto newPayroll)
        {
            if (newPayroll != null)
            {
                var list = PayrollDtaGrid.ItemsSource as List<Payrolls.UserWithPayrollDto>;
                if (list == null) return;

                var user = list.FirstOrDefault(u => u.UserId == newPayroll.UserId);
                if (user == null) return;

                // Xóa bản cũ nếu cùng tháng/năm (nếu là cập nhật)
                var existing = user.Payrolls.FirstOrDefault(p =>
                    p.CreatedDate.Month == newPayroll.CreatedDate.Month &&
                    p.CreatedDate.Year == newPayroll.CreatedDate.Year);

                if (existing != null)
                    user.Payrolls.Remove(existing);

                user.Payrolls.Insert(0, newPayroll); // Thêm payroll mới vào đầu danh sách (của user đó)

                PayrollDtaGrid.Items.Refresh(); // Cập nhật hiển thị
            }
        }
        private void AddPayroll(object sender, RoutedEventArgs e)
        {
            var window = new CreatePayroll(OnPayrollCreated);
            window.ShowDialog();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid PayrollId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa bảng lương này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteAsync(PayrollId);
                }
            }
        }
        private async Task DeleteAsync(Guid PayrollId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll/" + PayrollId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa cấu  bảng lương thành công.");
                _paginationHelper.RefreshAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra khi xóa";
                MessageBox.Show($"Không thể xóa bảng lương. Vui lòng thử lại sau: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
