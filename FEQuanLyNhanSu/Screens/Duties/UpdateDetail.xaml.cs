using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Duties
{
    /// <summary>
    /// Interaction logic for UpdateDuty.xaml
    /// </summary>
    public partial class UpdateDetail : Window
    {
        private Guid _detailId;
        private Action _onDetailUpdated;
        //private Action<DutyDetailResultDto> _onDetailUpdated;
        public UpdateDetail(Guid detailId, Action onDetailUpdated)
        {
            InitializeComponent();
            LoadUsers();
            _detailId = detailId;
            _onDetailUpdated = onDetailUpdated;
            _ = LoadDetailAsync();
        }

        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            string keyword = cbEmployee.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                cbEmployee.ItemsSource = null;
                return;
            }

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async Task LoadDetailAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Duty/detail/{_detailId}";
            //MessageBox.Show("Url: " + url);
            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<DutyDetailResultDto>>(json);

                cbEmployee.Text = result.Data.Name;
                txtDescription.Text = result.Data.Description;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Không thể tải chi tiết thông tin chức vụ. Lỗi: {errorContent}");
            }
        }
        private async Task LoadUsers()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

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
                MessageBox.Show("Không thể tải danh sách nhân viên.");
            }
        }

        // Update
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (cbEmployee.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên.");
                return;
            }
            var selectedUser = cbEmployee.SelectedItem as UserResultDto;
            if (selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.");
                return;
            }
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty/DutyDetail";
            var dutyDetail = new DutyDetailResultDto
            {
                DutyDetailId = _detailId,
                UserId = selectedUser.UserId,
                Description = txtDescription.Text.Trim()
            };
            using var client = CreateAuthorizedClient(token);
            var content = new StringContent(JsonConvert.SerializeObject(dutyDetail), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(baseUrl, content);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cập nhật thông tin chức vụ thành công.");
                _onDetailUpdated?.Invoke();
                this.Close();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Cập nhật thông tin chức vụ thất bại. Lỗi: {errorContent}");
            }
            //if (response.IsSuccessStatusCode)
            //{
            //    var json = await response.Content.ReadAsStringAsync();
            //    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DetailResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //    if (apiResponse?.Data != null)
            //    {
            //        _onDetailUpdated?.Invoke(apiResponse.Data);
            //    }

            //    MessageBox.Show("Cập nhật chi tiết thành công.");
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show("Cập nhật chi tiết thất bại.");
            //}
        }

        // Exit
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
