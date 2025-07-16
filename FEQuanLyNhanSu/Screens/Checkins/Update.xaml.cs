using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
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

            //LoadLogStatusesAsync();
            //LoadCheckinAsync();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadLogStatusesAsync(); 
            await LoadCheckinAsync();     
        }

        private async Task LoadLogStatusesAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/LogStatusConfig";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<LogStatusConfig>>>(json);
                _logStatuses = apiResponse?.Data;

                // Phải gán ItemsSource tại đây
                cbChkinMor.ItemsSource = _logStatuses;
                //cbChkoutMor.ItemsSource = _logStatuses;
                //cbChkinAft.ItemsSource = _logStatuses;
                //cbChkoutAft.ItemsSource = _logStatuses;

                cbChkinMor.DisplayMemberPath = "Name";
                //cbChkoutMor.DisplayMemberPath = "Name";
                //cbChkinAft.DisplayMemberPath = "Name";
                //cbChkoutAft.DisplayMemberPath = "Name";

                cbChkinMor.SelectedValuePath = "Id";
                //cbChkoutMor.SelectedValuePath = "Id";
                //cbChkinAft.SelectedValuePath = "Id";
                //cbChkoutAft.SelectedValuePath = "Id";
            }
            else
            {
                MessageBox.Show("Không thể tải LogStatus", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private async Task LoadLogStatusesAsync()
        //{
        //    var token = Application.Current.Properties["Token"]?.ToString();
        //    var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/LogStatusConfig";

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //    var response = await client.GetAsync(baseUrl);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var json = await response.Content.ReadAsStringAsync();
        //        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<LogStatusConfig>>>(json);
        //        _logStatuses = apiResponse?.Data;

        //        cbChkinMor.ItemsSource = _logStatuses;
        //        cbChkoutMor.ItemsSource = _logStatuses;
        //        cbChkinAft.ItemsSource = _logStatuses;
        //        cbChkoutAft.ItemsSource = _logStatuses;
        //    }
        //    else
        //    {
        //        MessageBox.Show("Không thể tải LogStatus", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

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

                    //MessageBox.Show($"API raw CheckinMorningStatus: '{result.Data.CheckinMorningStatus}'");
                    //MessageBox.Show($"API raw CheckoutMorningStatus: '{result.Data.CheckoutMorningStatus}'");
                    //MessageBox.Show($"API raw CheckinAfternoonStatus: '{result.Data.CheckinAfternoonStatus}'");
                    //MessageBox.Show($"API raw CheckoutAfternoonStatus: '{result.Data.CheckoutAfternoonStatus}'");

                    cbChkinMor.SelectedValue = _logStatuses.FirstOrDefault(x =>
                        string.Equals(x.Name?.Trim(), result.Data.Status?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                    //cbChkoutMor.SelectedValue = _logStatuses.FirstOrDefault(x =>
                    //    string.Equals(x.Name?.Trim(), result.Data.CheckoutMorningStatus?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                    //cbChkinAft.SelectedValue = _logStatuses.FirstOrDefault(x =>
                    //    string.Equals(x.Name?.Trim(), result.Data.CheckinAfternoonStatus?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                    //cbChkoutAft.SelectedValue = _logStatuses.FirstOrDefault(x =>
                    //    string.Equals(x.Name?.Trim(), result.Data.CheckoutAfternoonStatus?.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;

                    //MessageBox.Show($"Mapped Morning Checkin Id: {cbChkinMor.SelectedValue}");
                    //MessageBox.Show($"Mapped Morning Checkout Id: {cbChkoutMor.SelectedValue}");
                    //MessageBox.Show($"Mapped Afternoon Checkin Id: {cbChkinAft.SelectedValue}");
                    //MessageBox.Show($"Mapped Afternoon Checkout Id: {cbChkoutAft.SelectedValue}");
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


        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var token = Application.Current.Properties["Token"]?.ToString();
            var url = $"{baseUrl}/api/Checkin";

            using var client = CreateAuthorizedClient(token);

            var request = new CheckinResultDto
            {
                CheckinId = _checkinId,
                LogStatus = (int?)cbChkinMor.SelectedValue ?? 0,
                //CheckoutMorningStatus = (int?)cbChkoutMor.SelectedValue ?? 0,
                //CheckinAfternoonStatus = (int?)cbChkinAft.SelectedValue ?? 0,
                //CheckoutAfternoonStatus = (int?)cbChkoutAft.SelectedValue ?? 0
            };

            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CheckinResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data != null)
                {
                    _onUpdated?.Invoke(apiResponse.Data);
                }

                MessageBox.Show("Cập nhật checkin thành công.");
                this.Close();
            }
            else
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Cập nhật checkin thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
