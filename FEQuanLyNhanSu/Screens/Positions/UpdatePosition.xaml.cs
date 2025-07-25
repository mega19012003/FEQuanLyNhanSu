using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu.Screens.Positions
{
    /// <summary>
    /// Interaction logic for UpdatePosition.xaml
    /// </summary>
    public partial class UpdatePosition : Window
    {
        private Guid _positionId;
        private readonly Action<PositionResultDto> _onPositionUpdated;
        public UpdatePosition(Guid positionId, Action<PositionResultDto> onUpdated)
        {
            InitializeComponent();
            _positionId = positionId;
            _onPositionUpdated = onUpdated;
            _ = LoadPositionAsync();
        }
        private async Task LoadPositionAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Position/{_positionId}";

            //using var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", token);
            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PositionResultDto>>(json);

                txtName.Text = result.Data.Name;

            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải thông tin chức vụ: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnExit.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            try {
                string name = txtName.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Vui lòng nhập tên chức vụ.");
                    return;
                }

                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var token = Application.Current.Properties["Token"]?.ToString();
                var query = $"newName={Uri.EscapeDataString(name)}";
                var url = $"{baseUrl}/api/Position?id={_positionId}&{query}";

                using var client = CreateAuthorizedClient(token);

                var response = await client.PutAsync(url, null);

                //if (response.IsSuccessStatusCode)
                //{
                //    MessageBox.Show("Cập nhật chức vụ thành công.");
                //    _onPositionUpdated?.Invoke();
                //    this.Close();
                //}
                //else
                //{
                //    MessageBox.Show("Cập nhật chức vụ thất bại.");
                //}
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<PositionResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onPositionUpdated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Cập nhật chức vụ thành công.");
                    this.Close();
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show("Cập nhật chức vụ thất bại :{ errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btnExit.IsEnabled = true;
                btnUpdate.IsEnabled = true;
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
