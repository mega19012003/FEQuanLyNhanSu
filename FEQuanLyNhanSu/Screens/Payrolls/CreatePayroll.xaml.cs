using EmployeeAPI.Models;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.ApiResonses;
using FEQuanLyNhanSu.Models.Users;
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
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Payrolls;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Payrolls
{
    /// <summary>
    /// Interaction logic for CreatePayroll.xaml
    /// </summary>
    public partial class CreatePayroll : Window
    {
        private Action<PayrollResultDto> _onPayrollCreated;
        public CreatePayroll(Action<PayrollResultDto> onPayrollCreated)
        {
            InitializeComponent();
            //HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            _onPayrollCreated = onPayrollCreated;
            /*_ = LoadUsers();*/ 
            LoadDateComboboxes();
            Loaded += CreatePayroll_Loaded;
        }
        private async void CreatePayroll_Loaded(object sender, RoutedEventArgs e)
        {
            await HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            await FilterAsync();
        }
        private async Task HandleUI(string role)
        {
            switch (role)
            {
                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    _ =LoadPositions();
                    break;
                case "Administrator":
                    _ = LoadDepartments();
                    _ = LoadPositionsByDepartmentAsync();
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
            cbYear.ItemsSource = years;

            cbMonth.SelectedIndex = DateTime.Now.Month;
            cbYear.SelectedIndex = years.IndexOf(currentYear.ToString());
        }
        private async Task LoadUsers()
        {
            string keyword = cbEmployee.Text.Trim();
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);

                cbEmployee.ItemsSource = result.Data.Items;
                cbEmployee.IsDropDownOpen = true;
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra khi load user";
                MessageBox.Show($"Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task FilterAsync()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/User/employee-manager?";

                if (cbDepartment.SelectedItem is DepartmentResultDto selectedDepartment && selectedDepartment?.DepartmentId != Guid.Empty)
                    url += $"departmentId={selectedDepartment.DepartmentId}&";

                if (cbPosition.SelectedItem is PositionResultDto selectedPosition && selectedPosition?.Id != Guid.Empty)
                    url += $"positionId={selectedPosition.Id}&";

                var keyword = cbEmployee.Text.Trim();
                if (!string.IsNullOrWhiteSpace(keyword))
                    url += $"Search={Uri.EscapeDataString(keyword)}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
                    cbEmployee.ItemsSource = result.Data.Items;
                    cbEmployee.SelectedValuePath = "UserId";
                    cbEmployee.IsDropDownOpen = true;
                }
                else
                {
                    cbEmployee.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Filter: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            string keyword = cbEmployee.Text.Trim();

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            //var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
            //HttpResponseMessage response;

            if (string.IsNullOrEmpty(keyword))
            {
                await FilterAsync();
                cbEmployee.IsDropDownOpen = false;
            }
            else
            {
                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
                    cbEmployee.ItemsSource = result.Data.Items;
                    cbEmployee.IsDropDownOpen = true;
                }
                else
                {
                    cbEmployee.ItemsSource = null;
                }
            }
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
                    await LoadDepartments();
                    cbDepartment.SelectedItem = null;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";

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


        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;

            try
            {
                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                var selectedMonth = cbMonth.SelectedItem;
                var selectedYear = cbYear.SelectedItem;

                if (selectedUser == null || selectedMonth == null || selectedYear == null)
                {
                    MessageBox.Show("Vui lòng chọn đầy đủ nhân viên, tháng và năm.");
                    return;
                }

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();

                // Gán Month và Year vào query string
                var url = $"{baseUrl}/api/Payroll/calculate?userId={selectedUser.UserId}&Month={selectedMonth}&Year={selectedYear}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Gửi POST nhưng không có body
                var response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PayrollResultDto>>(json);
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<PayrollResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result?.Data != null)
                    {
                        lblFullname.Content = result.Data.Name;
                        lblDayWorked.Content = result.Data.DaysWorked.ToString();
                        lblNote.Content = result.Data.Note ?? "Không có ghi chú";
                        lblCreatedDate.Content = result.Data.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    }

                    MessageBox.Show("Chấm công thành công.");
                    _onPayrollCreated?.Invoke(result.Data);
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra khi tạo payroll.";
                    MessageBox.Show($"Chấm công thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
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
