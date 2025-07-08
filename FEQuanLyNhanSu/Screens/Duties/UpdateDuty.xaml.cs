using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
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
using System.Xml.Linq;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu.Screens.Duties
{
    /// <summary>
    /// Interaction logic for UpdateDuty.xaml
    /// </summary>
    public partial class UpdateDuty : Window
    {
        private Guid _dutyId;
        private Action<DutyResultDto> _onDutyUpdated;
        public UpdateDuty(Guid dutyId, Action<DutyResultDto> onDutyUpdated)
        {
            InitializeComponent();
            _dutyId = dutyId;
            _onDutyUpdated = onDutyUpdated;
            LoadDutyAsync();
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async Task LoadDutyAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Duty/{_dutyId}";

            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<DutyResultDto>>(json);

                txtUsername.Text = result.Data.Name;
                dpStartDate.Value = result.Data.StartDate;
                dpEndDate.Value = result.Data.EndDate;
            }
            else
            {
                MessageBox.Show("Không thể tải thông tin chức vụ.");
            }

        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtUsername.Text?.Trim();
            string startDate = dpStartDate.Value?.ToString();
            string endDate = dpEndDate.Value?.ToString();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                MessageBox.Show("Vui lòng đầy đ3u thông tin");
                return;
            }

            var result = new DutyResultDto
            {
                Id = _dutyId,
                Name = name,
                StartDate = (DateTime)dpStartDate.Value,
                EndDate = (DateTime)dpEndDate.Value
            };

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Duty";

            var json = JsonConvert.SerializeObject(result);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = CreateAuthorizedClient(token);

            var response = await client.PutAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            //if (response.IsSuccessStatusCode)
            //{
            //    MessageBox.Show("Cập nhật công việc thành công.");
            //    _onDutyUpdated?.Invoke();
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show($"Lỗi khi cập nhật: {responseBody}");
            //}
            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DutyResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data != null)
                {
                    _onDutyUpdated?.Invoke(apiResponse.Data);
                }

                MessageBox.Show("Cập nhật công việc thành công.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Cập nhật công việc thất bại.");
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
