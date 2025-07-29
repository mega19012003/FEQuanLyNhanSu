using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
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
using static FEQuanLyNhanSu.Services.Checkins;

namespace FEQuanLyNhanSu.Screens.Checkins
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        private Guid _checkinId;
        private List<LogStatusConfig> _logStatuses;
        private Action<CheckinResultDto> _onUpdated;
        public Update(Action<CheckinResultDto> onUpdated, Guid checkinId)
        {
            InitializeComponent();
            _onUpdated = onUpdated;
            _checkinId = checkinId;
            _ = LoadDataAsync();
        }
        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async Task LoadDataAsync()
        {
            await LoadCheckinAsync();
        }
        private async Task LoadCheckinAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Checkin/{_checkinId}";

            using var client = CreateAuthorizedClient(token);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<CheckinResultDto>>(json);
                
                if (result?.Data != null)
                {
                    txtFullname.Text = result.Data.Name;
                    dtpCheckinTime.Text = result.Data.CheckinTime.ToString("dd/MM/yyyy HH:mm:ss");
                    dtpCheckoutTime.Text = result.Data.CheckoutTime.ToString("dd/MM/yyyy HH:mm:ss");
                    //cbChkinMor.SelectedValue = _logStatuses.FirstOrDefault(x =>
                    //    string.Equals(x.Name?.Trim(), result.Data.Status?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                }
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải thông tin checkin: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.IsEnabled = false;
            btnExit.IsEnabled = false;

            try
            {
                var checkinId = _checkinId; 
                var checkinTime = dtpCheckinTime.Value;
                var checkoutTime = dtpCheckoutTime.Value;
                var updateNote = txtNote.Text;

                var dataToSend = new
                {
                    checkinId = checkinId,
                    checkinTime = checkinTime,
                    checkoutTime = checkoutTime,
                    updateNote = updateNote
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(dataToSend);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var apiUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin";

                var response = await client.PutAsync(apiUrl, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CheckinResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onUpdated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Lỗi khi cập nhật!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
