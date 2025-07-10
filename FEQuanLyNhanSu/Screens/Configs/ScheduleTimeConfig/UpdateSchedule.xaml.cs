using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.ApiResonses;
using FEQuanLyNhanSu.Models.Configs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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

namespace FEQuanLyNhanSu.Screens.Configs.ScheduleTimeConfig
{
    /// <summary>
    /// Interaction logic for UpdateSchedule.xaml
    /// </summary>
    public partial class UpdateSchedule : Window
    {
        public Action _onUpdated;
        public UpdateSchedule(Action onUpdated)
        {
            InitializeComponent();
            _onUpdated = onUpdated;
            LoadScheduleTime();
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

                        txtStatTimeMorning.Text = data.StartTimeMorning.ToString();
                        txtEndTimeMorning.Text = data.EndTimeMorning.ToString();
                        //txtLateMinutes.Text = data.LateThresholdMinutes.ToString();
                        txtAllowTime.Text = data.LogAllowtime.ToString();
                        txtStartTimeAfternoon.Text = data.StartTimeAfternoon.ToString();
                        txtEndTimeAfternoon.Text = data.EndTimeAfternoon.ToString();
                    }
                    else
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                        MessageBox.Show($"Không thể tải cấu hình: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cấu hình thời gian: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/ScheduleTime";

                if (int.Parse(txtAllowTime.Text) >= 60 || int.Parse(txtAllowTime.Text) < 1)
                {
                    MessageBox.Show("Thời gian cho phép checkin phải nằm trong khoảng từ 1-60 phút", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //if (int.Parse(txtLateMinutes.Text) >= 60 || int.Parse(txtLateMinutes.Text) < 1)
                //{
                //    MessageBox.Show("Thời gian cho phép trễ phải nằm trong khoảng từ 1-60 phút", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                var updated = new ScheduleTime
                {
                    StartTimeMorning = TimeOnly.Parse(txtStatTimeMorning.Text),
                    EndTimeMorning = TimeOnly.Parse(txtEndTimeMorning.Text),
                    StartTimeAfternoon = TimeOnly.Parse(txtStartTimeAfternoon.Text),
                    EndTimeAfternoon = TimeOnly.Parse(txtEndTimeAfternoon.Text),
                    LogAllowtime = int.Parse(txtAllowTime.Text),
                    //LateThresholdMinutes = int.Parse(txtLateMinutes.Text)
                };

                var json = JsonConvert.SerializeObject(updated);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsync(baseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thành công!");
                    _onUpdated?.Invoke();
                    this.Close();
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(responseContent);
                    var errorData = apiResponse?.Data ?? apiResponse?.Message ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Lỗi khi cập nhật: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
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
