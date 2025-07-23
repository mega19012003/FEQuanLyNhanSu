using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
using FEQuanLyNhanSu.ResponseModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using static FEQuanLyNhanSu.ResponseModels.Holidays;

namespace FEQuanLyNhanSu.Screens.Configs.HolidayConfig
{
    /// <summary>
    /// Interaction logic for UpdateHolidayConfig.xaml
    /// </summary>
    public partial class UpdateHolidayConfig : Window
    {
        private Guid _holidayConfigId;
        private Action<HolidayResultDto> _onHolidayUpdated;
        public UpdateHolidayConfig(Guid holidayConfigId, Action<HolidayResultDto> onUpdated)
        {
            InitializeComponent();
            _holidayConfigId = holidayConfigId;
            _onHolidayUpdated = onUpdated;
            LoadHolidayConfig();
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async void LoadHolidayConfig()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Holiday/{_holidayConfigId}";

            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<Holiday>>(json);

                txtName.Text = result.Data.name;

                // Convert DateOnly to DateTime? for compatibility
                dpStartDate.SelectedDate = result.Data.startDate.ToDateTime(TimeOnly.MinValue);
                dpEndDate.SelectedDate = result.Data.endDate.ToDateTime(TimeOnly.MinValue);
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải thông tin chức vụ: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                string name = txtName.Text.Trim();
                DateTime? startDate = dpStartDate.SelectedDate;
                DateTime? endDate = dpEndDate.SelectedDate;
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                    return;
                }

                var result = new Holiday
                {
                    HolidayId = _holidayConfigId,
                    name = name,
                    startDate = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : default,
                    endDate = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : default
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/Holiday";

                var json = JsonConvert.SerializeObject(result);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = CreateAuthorizedClient(token);

                var response = await client.PutAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<HolidayResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onHolidayUpdated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Cập nhật ngày lễ thành công.");
                    this.Close();
                }
                else
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Cập nhật ngày lễ thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btnUpdate.IsEnabled = true;
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
