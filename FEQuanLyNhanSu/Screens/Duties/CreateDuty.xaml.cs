using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Duties
{
    /// <summary>
    /// Interaction logic for CreateDuty.xaml
    /// </summary>
    public partial class CreateDuty : Window
    {
        private Action<DutyResultDto> _onDutyCreated;
        public CreateDuty(Action<DutyResultDto> onDutyCreated)
        {
            InitializeComponent();
            LoadUsers();
            _onDutyCreated = onDutyCreated;
        }

        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            string keyword = cbEmployee.Text.Trim();
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


            HttpResponseMessage response;

            if (string.IsNullOrEmpty(keyword))
            {
                response = await client.GetAsync(baseUrl); // không có query Search
            }
            else
            {
               /* var*/ 
                response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
                    cbEmployee.ItemsSource = result?.Data?.Items;
                    cbEmployee.SelectedItem = null;
                    cbEmployee.IsDropDownOpen = true;
                }
                else
                {
                    cbEmployee.ItemsSource = null;
                }
            }
        }

        private async Task LoadUsers()
        {
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
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(txtDescription.Text) || dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                    return;
                }
                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                if (selectedUser == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên thực hiện");
                    return;
                }

                var startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                var endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);

                var duty = new
                {
                    name = txtUsername.Text.Trim(),
                    startDate = startDate,
                    endDate = endDate,
                    dutyDetails = new[]
                    {
                        new {
                            userId = selectedUser.UserId,
                            description = txtDescription.Text?.Trim(),
                        }
                    }
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();

                var json = JsonConvert.SerializeObject(duty);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync($"{baseUrl}/api/Duty", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DutyResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onDutyCreated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Tạo công việc thành công!");
                    this.Close();
                }
                else
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    dynamic? apiResponse = JsonConvert.DeserializeObject<dynamic>(jsonResult);
                    string errorMessage = apiResponse?.Data != null ? apiResponse.Data.ToString() : apiResponse?.Message != null ? apiResponse.Message.ToString() : "Có lỗi xảy ra";
                    MessageBox.Show($"Tạo công việc thất bại.  Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
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
                return;
            this.Close();
        }
    }
}
