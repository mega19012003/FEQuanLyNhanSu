﻿using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Configs;
using FEQuanLyNhanSu.Screens.Configs.HolidayConfig;
using FEQuanLyNhanSu.Screens.Configs.LogStatusConfig;
using FEQuanLyNhanSu.Screens.Configs.ScheduleTimeConfig;
using FEQuanLyNhanSu.Screens.Departments;
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

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageConfig.xaml
    /// </summary>
    public partial class PageConfig : Page
    {
        private PaginationHelper<AllowedIPs.IPDto> _ipPaginationHelper;
        private PaginationHelper<Holidays.HolidayResultDto> _holidayPaginationHelper;

        public PageConfig()
        {
            InitializeComponent();
            LoadLogStatus();
            LoadAllowedIPStatus();
            LoadHolidayConfig();
            LoadScheduleTime();
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

                _ipPaginationHelper = new PaginationHelper<AllowedIPs.IPDto>(
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
        // Create
        private void AddIPBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateIPConfig(LoadAllowedIPStatus);
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
                MessageBox.Show("Không thể xóa cấu hình IP. Vui lòng thử lại sau.");
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
                var result = await SearchHelper.SearchAsync<AllowedIPs.IPDto>("api/AllowedIP", keyword, token);
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
        // Create
        private void AddHolidayBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateHolidayConfig(LoadHolidayConfig);
            window.ShowDialog();
        }

        // Update
        private void btnUpdateHoliday_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid HolidayId)
            {
                var editWindow = new UpdateHolidayConfig(HolidayId, LoadHolidayConfig);
                editWindow.ShowDialog();
            }
        }

        // Delete
        private void btnDeleteHoliday_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid DepartmentId)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa ngày nghỉ này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _ = DeleteDepartmentAsync(DepartmentId);
                }
            }
        }
        private async Task DeleteDepartmentAsync(Guid departmentId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Holiday/" + departmentId;
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
                MessageBox.Show("Không thể xóa phòng ban. Vui lòng thử lại sau.");
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
