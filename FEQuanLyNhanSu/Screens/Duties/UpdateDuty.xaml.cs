using EmployeeAPI.Enums;
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
            _ = LoadDutyAsync();
            //LoadDutyStatus();
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        //private void LoadDutyStatus()
        //{
        //    cb.ItemsSource = Enum.GetValues(typeof(DutyStatus)).Cast<DutyStatus>().Skip(1).ToList();
        //}
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
                // Convert DateOnly to DateTime? for compatibility
                dpStartDate.SelectedDate = result.Data.StartDate.ToDateTime(TimeOnly.MinValue);
                dpEndDate.SelectedDate = result.Data.EndDate.ToDateTime(TimeOnly.MinValue);
                //if (Enum.TryParse<DutyStatus>(result.Data.Status, out var dutyStatus))
                //{
                //    cb.SelectedItem = dutyStatus;
                //}
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
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                string name = txtUsername.Text?.Trim();
                string note = txtNote.Text?.Trim();
                DateTime? startDate = dpStartDate.SelectedDate;
                DateTime? endDate = dpEndDate.SelectedDate;

                if (string.IsNullOrWhiteSpace(name) || startDate == null || endDate == null || string.IsNullOrEmpty(txtNote.Text))
                {
                    MessageBox.Show("Vui lòng đầy đủ thông tin");
                    return;
                }

                //var result = new DutyResultDto
                //{
                //    Id = _dutyId,
                //    Name = name,
                //    StartDate = DateOnly.FromDateTime(startDate.Value), 
                //    EndDate = DateOnly.FromDateTime(endDate.Value),
                //    Status = ((DutyStatus)cb.SelectedItem).ToString()
                //};
                var updateDto = new
                {
                    Id = _dutyId,
                    Name = name,
                    StartDate = DateOnly.FromDateTime(startDate.Value),
                    EndDate = DateOnly.FromDateTime(endDate.Value),
                    Note = note,
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/Duty";

                var json = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = CreateAuthorizedClient(token);

                var response = await client.PutAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DutyResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onDutyUpdated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Cập nhật công việc thành công.");
                    this.Close();
                }
                else
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(responseBody);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Cập nhật công việc thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
