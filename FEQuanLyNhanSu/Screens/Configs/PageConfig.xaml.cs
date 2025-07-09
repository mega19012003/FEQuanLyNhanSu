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
using Newtonsoft.Json;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageConfig.xaml
    /// </summary>
    public partial class PageConfig : Page
    {
        private PaginationHelper<AllowedIPs.IPResultDto> _ipPaginationHelper;
        private PaginationHelper<Holidays.HolidayResultDto> _holidayPaginationHelper;


        public PageConfig()
        {
            InitializeComponent();
            //MessageBox.Show($"Chào mừng bạn đến với trang cấu hình, vai trò của bạn là: {Application.Current.Properties["UserRole"]?.ToString()}");
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
        }

        private void HandleUI(string role)
        {
            switch(role)
            {
                case "Administrator":
                    LoadLogStatus();
                    LoadAllowedIPStatus();
                    LoadHolidayConfig();
                    LoadScheduleTime();
                    break;
                case "Manager":
                    TabIP.Visibility = Visibility.Collapsed;
                    IPGrid.Visibility = Visibility.Collapsed;
                    AddHolidayBtn.Visibility = Visibility.Collapsed;
                    btnUpdateWorkTime.Visibility = Visibility.Collapsed;
                    DtaGridActionHoliday.Visibility = Visibility.Collapsed;
                    DtaGridActionStatus.Visibility = Visibility.Collapsed;
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

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(baseUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseStr = await response.Content.ReadAsStringAsync();
                        var apiResult = JsonConvert.DeserializeObject<ApiResponse<List<LogStatusConfig>>>(responseStr);
                        ChknDtaGrid.ItemsSource = apiResult.Data;
                    }
                    else
                    {
                        MessageBox.Show("Không thể tải cấu hình. Vui lòng thử lại sau.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình: {ex.Message}");
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
                    items => IPDtaGrid.ItemsSource = items,
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
        public async void LoadScheduleTime()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/ScheduleTime";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(baseUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseStr = await response.Content.ReadAsStringAsync();
                        var apiResult = JsonConvert.DeserializeObject<ApiResponse<ScheduleTime>>(responseStr);
                        var data = apiResult.Data;

                        txtStartTimeMorning.Text = data.StartTimeMorning.ToString();
                        txtEndTimeMorning.Text = data.EndTimeMorning.ToString();
                        txtLateTime.Text = data.LateThresholdMinutes.ToString();
                        txtAllowTime.Text = data.LogAllowtime.ToString();
                        txtStartTimeAfternoon.Text = data.StartTimeAfternoon.ToString();
                        txtEndTimeAfternoon.Text = data.EndTimeAfternoon.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Không thể tải cấu hình. Vui lòng thử lại sau.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình thời gian: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // IP CONFIG
        /// ///////////////////////////////////////////

        private void OnIPCreated(IPResultDto newDept)
        {
            if (newDept != null)
            {
                var list = IPDtaGrid.ItemsSource as List<IPResultDto> ?? new List<IPResultDto>();
                list.Insert(0, newDept);
                IPDtaGrid.ItemsSource = null;
                IPDtaGrid.ItemsSource = list;

                IPDtaGrid.SelectedItem = newDept;
                IPDtaGrid.ScrollIntoView(newDept);
            }
        }

        //private void OnDepartmentUpdated(IPResultDto updatedDept)
        //{
        //    if (updatedDept != null)
        //    {
        //        var list = IPDtaGrid.ItemsSource as List<IPResultDto> ?? new List<IPResultDto>();

        //        var existing = list.FirstOrDefault(d => d.AllowedIPId == updatedDept.AllowedIPId);
        //        if (existing != null)
        //        {
        //            list.Remove(existing);
        //        }

        //        list.Insert(0, updatedDept);

        //        IPDtaGrid.ItemsSource = null;
        //        IPDtaGrid.ItemsSource = list;

        //        IPDtaGrid.SelectedItem = updatedDept;
        //        IPDtaGrid.ScrollIntoView(updatedDept);
        //    }
        //}


        // Create
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

        // Search
        private async void txtTextChangedIP(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearchIP.Text?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
                LoadAllowedIPStatus();
            else
            {
                var result = await SearchHelper.SearchAsync<AllowedIPs.IPResultDto>("api/AllowedIP", keyword, token);
                IPDtaGrid.ItemsSource = result;
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
            if (button?.Tag is int Id)
            {
                var editWindow = new UpdateLogStatus(Id, LoadLogStatus);
                editWindow.ShowDialog();
            }
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

        // Search
        private async void txtTextChangedHoliday(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearchHoliday.Text?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
                LoadHolidayConfig();
            else
            {
                var result = await SearchHelper.SearchAsync<Holidays.HolidayResultDto>("api/Holiday", keyword, token);
                HolidayDtaGrid.ItemsSource = result;
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

        private void btnUpdateWorkTime_Click(object sender, RoutedEventArgs e)
        {
            var window = new UpdateSchedule(LoadScheduleTime);
            window.ShowDialog();
        }
    }
}
