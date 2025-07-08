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

                dpStartDate.Value = result.Data.startDate;
                dpEndDate.Value = result.Data.endDate;
            }
            else
            {
                MessageBox.Show("Không thể tải thông tin chức vụ.");
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            DateTime? startDate = dpStartDate.Value;
            DateTime? endDate = dpEndDate.Value;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            var result = new Holiday
            {
                HolidayId = _holidayConfigId,
                name = name,
                startDate = (DateTime)startDate,
                endDate = (DateTime)endDate
            };

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            //var query = $"IPAddress={Uri.EscapeDataString(LogStatusName)}";
            var url = $"{baseUrl}/api/Holiday"; //?id={_logStatusId}&{query}";

            var json = JsonConvert.SerializeObject(result);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = CreateAuthorizedClient(token);

            var response = await client.PutAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            //if (response.IsSuccessStatusCode)
            //{
            //    MessageBox.Show("Cập nhật holdiay thành công.");
            //    _onHolidayUpdated?.Invoke();
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show($"Lỗi khi cập nhật: {responseBody}");
            //}
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
                MessageBox.Show("Cập nhật ngày lễ thất bại.");
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
