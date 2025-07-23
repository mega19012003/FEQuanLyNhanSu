using EmployeeAPI.Models;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Configs;
using FEQuanLyNhanSu.Screens.Configs.HolidayConfig;
using FEQuanLyNhanSu.Screens.Configs.LogStatusConfig;
using FEQuanLyNhanSu.Screens.Configs.ScheduleTimeConfig;
using FEQuanLyNhanSu.Screens.Positions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.LogStatusConfigs;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageConfig.xaml
    /// </summary>
    public partial class PageConfig : Page
    {
        private PaginationHelper<AllowedIPs.IPResultDto> _ipPaginationHelper;
        private PaginationHelper<Holidays.HolidayResultDto> _holidayPaginationHelper;
        private PaginationHelper<LogStatusConfigs.LogStatusDto> _logStatusPaginationHelper;
        private PaginationHelper<Schedules.ScheduleDto> _schedulePaginationHelper;
        private ScheduleTime _currentScheduleTime;
        public PageConfig()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
        }
        private void HandleUI(string role)
        {
            switch(role)
            {
                case "SystemAdmin":
                    LoadLogStatus();
                    LoadAllowedIPStatus();
                    LoadHolidayConfig();
                    DtaGridActionHoliday.Visibility = Visibility.Collapsed;
                    DtaGridActionStatus.Visibility = Visibility.Collapsed;
                    DtaGridActionIP.Visibility = Visibility.Collapsed;
                    TabSchedule.Visibility = Visibility.Collapsed;
                    AddIPBtn.Visibility = Visibility.Collapsed;
                    AddHolidayBtn.Visibility = Visibility.Collapsed;
                    LoadListScheduleTime();
                    LoadCompanies();
                    break;
                case "Administrator":
                    LoadLogStatus();
                    LoadAllowedIPStatus();
                    LoadHolidayConfig();
                    LoadScheduleTime();
                    btnPrevPageLogStatus.Visibility = Visibility.Collapsed;
                    btnNextPageLogStatus.Visibility = Visibility.Collapsed;
                    txtPageLogStatus.Visibility = Visibility.Collapsed;
                    cbCompanyAllowedIP.Visibility = Visibility.Collapsed;
                    cbCompanyHoliday.Visibility = Visibility.Collapsed;
                    cbCompanyStatus.Visibility = Visibility.Collapsed;
                    TabListSchedule.Visibility = Visibility.Collapsed;
                    break;
                case "Manager":
                    TabIP.Visibility = Visibility.Collapsed;
                    IPGrid.Visibility = Visibility.Collapsed;
                    AddHolidayBtn.Visibility = Visibility.Collapsed;
                    btnUpdateWorkTime.Visibility = Visibility.Collapsed;
                    DtaGridActionHoliday.Visibility = Visibility.Collapsed;
                    DtaGridActionStatus.Visibility = Visibility.Collapsed;
                    btnPrevPageLogStatus.Visibility = Visibility.Collapsed;
                    btnNextPageLogStatus.Visibility = Visibility.Collapsed;
                    txtPageLogStatus.Visibility = Visibility.Collapsed;
                    cbCompanyAllowedIP.Visibility = Visibility.Collapsed;
                    cbCompanyHoliday.Visibility = Visibility.Collapsed;
                    cbCompanyStatus.Visibility = Visibility.Collapsed;
                    TabListSchedule.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Xem cấu hình";
                    LoadLogStatus();
                    LoadHolidayConfig();
                    LoadScheduleTime();
                    break;
                case "Employee":
                    TabIP.Visibility = Visibility.Collapsed;
                    IPGrid.Visibility = Visibility.Collapsed;
                    AddHolidayBtn.Visibility = Visibility.Collapsed;
                    btnUpdateWorkTime.Visibility = Visibility.Collapsed;
                    DtaGridActionHoliday.Visibility = Visibility.Collapsed;
                    DtaGridActionStatus.Visibility = Visibility.Collapsed;
                    btnPrevPageLogStatus.Visibility = Visibility.Collapsed;
                    btnNextPageLogStatus.Visibility = Visibility.Collapsed;
                    txtPageLogStatus.Visibility = Visibility.Collapsed;
                    cbCompanyAllowedIP.Visibility = Visibility.Collapsed;
                    cbCompanyHoliday.Visibility = Visibility.Collapsed;
                    cbCompanyStatus.Visibility = Visibility.Collapsed;
                    TabListSchedule.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Xem cấu hình";
                    LoadLogStatus();
                    LoadHolidayConfig();
                    LoadScheduleTime();
                    break;
            }
        }

        // LOAD DATABASE
        /// ///////////////////////////////////////////
        private async void LoadLogStatus()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/LogStatusConfig";
                int pageSize = 20;

                _logStatusPaginationHelper = new PaginationHelper<LogStatusConfigs.LogStatusDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => LogDtaGrid.ItemsSource = items,
                    txtPageLogStatus
                );

                await _logStatusPaginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadAllowedIPStatus()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/AllowedIP";
                int pageSize = 20;

                _ipPaginationHelper = new PaginationHelper<AllowedIPs.IPResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => AllowedIPDtaGrid.ItemsSource = items,
                    txtPageIP
                );
                _ = _ipPaginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình IP: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadHolidayConfig()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Holiday";
                int pageSize = 20;
                _holidayPaginationHelper = new PaginationHelper<Holidays.HolidayResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => HolidayDtaGrid.ItemsSource = items,
                    txtPageHoliday
                );
                _ = _holidayPaginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình ngày nghỉ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async void LoadListScheduleTime()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/ScheduleTime";
                int pageSize = 20;
                _schedulePaginationHelper = new PaginationHelper<Schedules.ScheduleDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => ScheduleDtaGrid.ItemsSource = items,
                    txtPageSchedule
                );
                _ = _schedulePaginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình giờ làm việc: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async void LoadScheduleTime()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/ScheduleTime";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(baseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Không thể tải cấu hình. Vui lòng thử lại sau.");
                    return;
                }

                var responseStr = await response.Content.ReadAsStringAsync();

                var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<ScheduleTime>>>(responseStr);
                var schedule = apiResult?.Data?.Items?.FirstOrDefault();

                if (schedule != null)
                {
                    _currentScheduleTime = schedule;
                    txtStartTimeMorning.Text = schedule.StartTimeMorning.ToString();
                    txtEndTimeMorning.Text = schedule.EndTimeMorning.ToString();
                    txtAllowTime.Text = schedule.LogAllowtime.ToString();
                    txtStartTimeAfternoon.Text = schedule.StartTimeAfternoon.ToString();
                    txtEndTimeAfternoon.Text = schedule.EndTimeAfternoon.ToString();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy cấu hình thời gian.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình thời gian: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                cbCompanyStatus.ItemsSource = result.Data.Items;
                cbCompanySchedule.ItemsSource = result.Data.Items;
                cbCompanyAllowedIP.ItemsSource = result.Data.Items;
                cbCompanyHoliday.ItemsSource = result.Data.Items;

                if (result.Data.Items != null && result.Data.Items.Any())
                {
                    cbCompanyStatus.SelectedItem = result.Data.Items.First();
                    cbCompanySchedule.SelectedItem = result.Data.Items.First();
                    cbCompanyAllowedIP.SelectedItem = result.Data.Items.First();
                    cbCompanyHoliday.SelectedItem = result.Data.Items.First();
                    //await LoadDepartmentByCompanyAsync();
                    //await LoadPositionByCompanyAsync();
                    await FilterHolidayAsync();
                    await FilterIPAsync();
                    await FilterScheduleAsync();
                    await FilterLogStatusAsync();
                }
            }
        }


        // IP CONFIG
        /// ///////////////////////////////////////////
        private void OnIPCreated(IPResultDto newDept)
        {
            if (newDept != null)
            {
                var list = AllowedIPDtaGrid.ItemsSource as List<IPResultDto> ?? new List<IPResultDto>();
                list.Insert(0, newDept);
                AllowedIPDtaGrid.ItemsSource = null;
                AllowedIPDtaGrid.ItemsSource = list;

                AllowedIPDtaGrid.SelectedItem = newDept;
                AllowedIPDtaGrid.ScrollIntoView(newDept);
            }
        }
        private void AddIPBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateIPConfig(OnIPCreated);
            window.ShowDialog();
        }
        // Delete
        private async void btnDeleteIP_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid AllowedIPId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa cấu hình IP này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteIPConfigAsync(AllowedIPId);
                }
            }
        }
        private async Task DeleteIPConfigAsync(Guid IPAddressId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/AllowedIP/" + IPAddressId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa cấu hình IP thành công.");
                _ipPaginationHelper.LoadPageAsync(1);
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể xóa cấu hình IP: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // PAGINATION
        private async void btnNextPageIP_Click(object sender, RoutedEventArgs e)
        {
            await _ipPaginationHelper.NextPageAsync();
        }
        private async void btnPrevPageIP_Click(object sender, RoutedEventArgs e)
        {
            await _ipPaginationHelper.PrevPageAsync();
        }


        // LOGSTATUS CONFIG
        /// ///////////////////////////////////////////
        private void btnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid Id)
            {
                var editWindow = new UpdateLogStatus(Id, LoadLogStatus);
                editWindow.ShowDialog();
            }
        }
        // PAGINATION
        private async void btnNextPageLogStatus_Click(object sender, RoutedEventArgs e)
        {
            await _logStatusPaginationHelper.NextPageAsync();
        }
        private async void btnPrevPageLogStatus_Click(object sender, RoutedEventArgs e)
        {
            await _logStatusPaginationHelper.PrevPageAsync();
        }


        // HOLIDAY CONFIG
        /// ///////////////////////////////////////////
        private void OnHolidayCreated(Holidays.HolidayResultDto newDept)
        {
            if (newDept != null)
            {
                var list = HolidayDtaGrid.ItemsSource as List<Holidays.HolidayResultDto> ?? new List<Holidays.HolidayResultDto>();
                list.Insert(0, newDept);
                HolidayDtaGrid.ItemsSource = null;
                HolidayDtaGrid.ItemsSource = list;

                HolidayDtaGrid.SelectedItem = newDept;
                HolidayDtaGrid.ScrollIntoView(newDept);
            }
        }
        private void OnHolidayUpdated(Holidays.HolidayResultDto updatedDept)
        {
            if (updatedDept != null)
            {
                var list = HolidayDtaGrid.ItemsSource as List<Holidays.HolidayResultDto> ?? new List<Holidays.HolidayResultDto>();

                var existing = list.FirstOrDefault(d => d.HolidayId == updatedDept.HolidayId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedDept);

                HolidayDtaGrid.ItemsSource = null;
                HolidayDtaGrid.ItemsSource = list;

                HolidayDtaGrid.SelectedItem = updatedDept;
                HolidayDtaGrid.ScrollIntoView(updatedDept);
            }
        }
        // Create
        private void AddHolidayBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateHolidayConfig(OnHolidayCreated);
            window.ShowDialog();
        }
        // Update
        private void btnUpdateHoliday_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid HolidayId)
            {
                var editWindow = new UpdateHolidayConfig(HolidayId, OnHolidayUpdated);
                editWindow.ShowDialog();
            }
        }
        // Delete
        private void btnDeleteHoliday_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid HolidayId)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa ngày nghỉ này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _ = DeleteHolidayAsync(HolidayId);
                }
            }
        }
        private async Task DeleteHolidayAsync(Guid HolidayId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Holiday/" + HolidayId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa ngày nghỉ thành công.");
                _ = _holidayPaginationHelper.LoadPageAsync(1); 
            }
            else
            {
                MessageBox.Show("Không thể xóa ngày lễ. Vui lòng thử lại sau.");
            }
        }
        // Pagination
        private async void btnNextPageHoliday_Click(object sender, RoutedEventArgs e)
        {
            await _holidayPaginationHelper.NextPageAsync();
        }
        private async void btnPrevPageHoliday_Click(object sender, RoutedEventArgs e)
        {
            await _holidayPaginationHelper.PrevPageAsync();
        }


        //////////////////////////////////////////////// Schedule
        private void btnUpdateWorkTime_Click(object sender, RoutedEventArgs e)
        {
            var window = new UpdateSchedule(LoadScheduleTime, _currentScheduleTime);
            window.ShowDialog();
        }
        // Pagination
        private async void btnNextPageSchedule_Click(object sender, RoutedEventArgs e)
        {
            await _holidayPaginationHelper.NextPageAsync();
        }
        private async void btnPrevPageSchedule_Click(object sender, RoutedEventArgs e)
        {
            await _holidayPaginationHelper.PrevPageAsync();
        }


        //////////////////////////////////////////////// search
        private async Task FilterAsync<TDto>(string apiPath, TextBox? searchTextBox, ComboBox companyComboBox, DataGrid targetDataGrid)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            string? keyword = searchTextBox?.Text?.Trim();

            Guid? companyId = null;
            string companyText = companyComboBox.Text?.Trim();

            if (companyComboBox.SelectedItem is CompanyResultDto selectedComp)
            {
                companyId = selectedComp.CompanyId;
            }
            else if (!string.IsNullOrEmpty(companyText))
            {
                var found = (companyComboBox.ItemsSource as IEnumerable<CompanyResultDto>)
                    ?.FirstOrDefault(d => d.Name.Equals(companyText, StringComparison.OrdinalIgnoreCase));
                companyId = found?.CompanyId;
            }

            var items = await FilterService.SearchAndFilterAsync<TDto>(baseUrl, apiPath, token, keyword, companyId);

            //MessageBox.Show("Items count: " + items.Count);
            targetDataGrid.ItemsSource = items;
        }
        public static class FilterService
        {
            public static async Task<List<TDto>> SearchAndFilterAsync<TDto>(string baseUrl, string apiPath, string token, string? searchKeyword, Guid? companyId, int pageIndex = 1, int pageSize = 20)
            {
                try
                {
                    var parameters = new List<string>();
                    if (!string.IsNullOrWhiteSpace(searchKeyword))
                        parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                    if (companyId.HasValue)
                        parameters.Add($"companyId={companyId}");
                    parameters.Add($"pageIndex={pageIndex}");
                    parameters.Add($"pageSize={pageSize}");

                    var url = baseUrl + apiPath;
                    if (parameters.Any())
                        url += "?" + string.Join("&", parameters);
                    //MessageBox.Show($"Search keyword: {url}");
                    using var client = new HttpClient();
                    if (!string.IsNullOrWhiteSpace(token))
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetAsync(url);
                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Lỗi khi lọc: {response.StatusCode}");
                        return new List<TDto>();
                    }

                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<TDto>>>(
                        json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result?.Data?.Items?.ToList() ?? new List<TDto>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    return new List<TDto>();
                }
            }
        }

        private async Task FilterLogStatusAsync()
        {
            await FilterAsync<LogStatusConfigs.LogStatusDto>("/api/LogStatusConfig", txtSearchLog, cbCompanyStatus, LogDtaGrid);
        }
        private async Task FilterHolidayAsync()
        {
            await FilterAsync<Holidays.HolidayResultDto>("/api/Holiday", txtSearchHoliday, cbCompanyHoliday, HolidayDtaGrid);
        }
        private async Task FilterScheduleAsync()
        {
            await FilterAsync<Schedules.ScheduleDto>("/api/ScheduleTime", null, cbCompanySchedule, ScheduleDtaGrid);
        }
        private async Task FilterIPAsync()
        {
            await FilterAsync<IPResultDto>("/api/AllowedIP", txtSearchAllowedIP, cbCompanyAllowedIP, AllowedIPDtaGrid);
        }

        // LogStatus
        private async void txtTextChangedLog(object sender, TextChangedEventArgs e) => await FilterLogStatusAsync();
        private async void cbCompanyStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterLogStatusAsync();
        // Holiday
        private async void txtTextChangedHoliday(object sender, TextChangedEventArgs e) => await FilterHolidayAsync();
        private async void cbCompanyHoliday_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterHolidayAsync();
        // Schedule
        private async void cbCompanySchedule_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterScheduleAsync();
        // IP
        private async void txtTextChangedIP(object sender, TextChangedEventArgs e) => await FilterIPAsync();
        private async void cbCompanyIP_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterIPAsync();

        // Key up
        private async void cbCompanyStatus_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompanyStatus.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/LogStatusConfig";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();
                    cbCompanyStatus.SelectedItem = null;
                    cbCompanyStatus.IsDropDownOpen = true;

                    LoadLogStatus();
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<CompanyResultDto>>>(json);
                        cbCompanyStatus.ItemsSource = result.Data.Items;

                        cbCompanyStatus.SelectedItem = null;
                        cbCompanyStatus.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbCompanyStatus.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm Log Status: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbCompanyHoliday_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompanyHoliday.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Holiday";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();
                    cbCompanyHoliday.SelectedItem = null;
                    cbCompanyHoliday.IsDropDownOpen = true;

                    LoadLogStatus();
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<CompanyResultDto>>>(json);
                        cbCompanyHoliday.ItemsSource = result.Data.Items;

                        cbCompanyHoliday.SelectedItem = null;
                        cbCompanyHoliday.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbCompanyHoliday.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm Log Status: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbCompanySchedule_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompanySchedule.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/ScheduleTime";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();
                    cbCompanySchedule.SelectedItem = null;
                    cbCompanySchedule.IsDropDownOpen = true;

                    LoadLogStatus();
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<Schedules.ScheduleDto>>>(json);
                        cbCompanySchedule.ItemsSource = result.Data.Items;

                        cbCompanySchedule.SelectedItem = null;
                        cbCompanySchedule.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbCompanySchedule.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm ScheduleTime: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbCompanyIP_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompanyAllowedIP.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/AllowedIP";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();
                    cbCompanyAllowedIP.SelectedItem = null;
                    cbCompanyAllowedIP.IsDropDownOpen = true;

                    LoadLogStatus();
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<IPResultDto>>>(json);
                        cbCompanyAllowedIP.ItemsSource = result.Data.Items;

                        cbCompanyAllowedIP.SelectedItem = null;
                        cbCompanyAllowedIP.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbCompanyAllowedIP.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm IP: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
