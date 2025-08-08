using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Payrolls;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Payrolls;
using FEQuanLyNhanSu.Services;
using Newtonsoft.Json;
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
using static FEQuanLyNhanSu.Services.UserService.Users;

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
            //LoadDateComboboxes();
            ////_ = LoadUserWithPayroll();
            //HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            //Loaded += async (s, e) => await FilterAsync();
            Loaded += PageUser_Loaded;
        }

        private async void PageUser_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDateComboboxes();


            HandleUI(Application.Current.Properties["UserRole"]?.ToString());

            await Task.Delay(200);

            await FilterAsync(); 
        }

        private async void HandleUI(string role)
        {
            switch (role)
            {
                case "SystemAdmin":
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    await LoadCompanies();
                    await LoadDepartmentByCompanyAsync();
                    await LoadPositionsByDepartmentAsync();
                    break;
                case "Administrator":
                    cbCompany.Visibility = Visibility.Collapsed;
                    await LoadDepartments();
                    await LoadPositionsByDepartmentAsync();
                    break; 
                case "Manager":
                    cbCompany.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    await LoadPositions();
                    break;
                case "Employee":
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    cbCompany.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Chấm công";
                    cbCompany.Visibility = Visibility.Collapsed;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    cbPosition.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async Task LoadUserWithPayroll()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll/user-payrolls";
            int pageSize = 10;

            _paginationHelper = new PaginationHelper<UserWithPayrollDto>(
                baseUrl,
                pageSize,
                token,
                items => PayrollDtaGrid.ItemsSource = items,
                lblPage,
                page => BuildPayrollUrlWithFilter(page, pageSize) 
            );

            await _paginationHelper.LoadPageAsync(1);
        }
        private string BuildPayrollUrlWithFilter(int pageIndex, int pageSize)
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll/user-payrolls";
            var parameters = new List<string>();

            string keyword = txtSearch.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
                parameters.Add($"Search={Uri.EscapeDataString(keyword)}");

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
            await LoadUserWithPayroll();
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
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
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
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
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
        //private async void cbPosition_KeyUp(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        var keyword = cbPosition.Text.Trim();
        //        var token = Application.Current.Properties["Token"]?.ToString();
        //        var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

        //        Guid? departmentId = null;
        //        if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
        //            departmentId = selectedDept.DepartmentId;

        //        using var client = new HttpClient();
        //        client.DefaultRequestHeaders.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //        if (string.IsNullOrEmpty(keyword))
        //        {
        //            if (!departmentId.HasValue)
        //            {
        //                cbPosition.ItemsSource = null;
        //                cbPosition.SelectedItem = null;
        //                cbPosition.IsDropDownOpen = false;
        //                return;
        //            }

        //            string url = $"{baseUrl}?departmentId={departmentId.Value}";

        //            var response = await client.GetAsync(url);
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var json = await response.Content.ReadAsStringAsync();
        //                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
        //                cbPosition.ItemsSource = result?.Data?.Items;
        //                cbPosition.SelectedItem = null;
        //                cbPosition.IsDropDownOpen = true;
        //            }
        //            else
        //            {
        //                cbPosition.ItemsSource = null;
        //            }
        //        }
        //        else
        //        {
        //            string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";
        //            if (departmentId.HasValue)
        //                url += $"&departmentId={departmentId.Value}";

        //            var response = await client.GetAsync(url);
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var json = await response.Content.ReadAsStringAsync();
        //                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
        //                cbPosition.ItemsSource = result?.Data?.Items;
        //                cbPosition.SelectedItem = null;
        //                cbPosition.IsDropDownOpen = true;
        //            }
        //            else
        //            {
        //                cbPosition.ItemsSource = null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
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

        private async void OnPayrollCreated(Payrolls.PayrollResultDto newPayroll)
        {
            if (newPayroll == null) return;

            var list = PayrollDtaGrid.ItemsSource as List<Payrolls.UserWithPayrollDto>;
            if (list == null) return;

            var user = list.FirstOrDefault(u => u.UserId == newPayroll.UserId);

            var token = Application.Current.Properties["Token"]?.ToString();
            if (string.IsNullOrEmpty(token)) return;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var response = await httpClient.GetAsync($"{baseUrl}/api/User/{newPayroll.UserId}");
            if (!response.IsSuccessStatusCode) return;

            var resultJson = await response.Content.ReadAsStringAsync();
            var userResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponse>(resultJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (userResponse?.Data == null) return;

            var userData = userResponse.Data;

            if (user == null)
            {
                user = new Payrolls.UserWithPayrollDto
                {
                    UserId = userData.UserId,
                    Fullname = userData.Fullname,
                    PhoneNumber = userData.PhoneNumber,
                    Address = userData.Address,
                    ImageUrl = userData.ImageUrl,
                    Payrolls = new ObservableCollection<Payrolls.PayrollResultDto>()
                };
                list.Add(user);
            }

            var existing = user.Payrolls.FirstOrDefault(p => p.CreatedDate.Month == newPayroll.CreatedDate.Month &&  p.CreatedDate.Year == newPayroll.CreatedDate.Year);

            if (existing != null)
                user.Payrolls.Remove(existing);

            user.Payrolls.Insert(0, newPayroll);
            list.Remove(user);
            list.Insert(0, user);

            PayrollDtaGrid.ItemsSource = null;
            PayrollDtaGrid.ItemsSource = list;
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
                MessageBox.Show("Xóa bảng lương thành công.");

                var list = PayrollDtaGrid.ItemsSource as List<Payrolls.UserWithPayrollDto>;
                if (list != null)
                {
                    foreach (var user in list)
                    {
                        var payroll = user.Payrolls.FirstOrDefault(p => p.Id == PayrollId);
                        if (payroll != null)
                        {
                            user.Payrolls.Remove(payroll);
                            break;
                        }
                    }

                    PayrollDtaGrid.ItemsSource = null;
                    PayrollDtaGrid.ItemsSource = list;
                }

                _paginationHelper.RefreshAsync();
            }
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _paginationHelper.NextPageAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"NextPageAsync Error: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _paginationHelper.PrevPageAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PrevPageAsync Error: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DtaGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dep = (DependencyObject)e.OriginalSource;

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

                if (PayrollDtaGrid.SelectedItems.Contains(item))
                    PayrollDtaGrid.SelectedItems.Remove(item); 
                else
                    PayrollDtaGrid.SelectedItems.Add(item); 

                e.Handled = true; 
            }
        }
    }
}
