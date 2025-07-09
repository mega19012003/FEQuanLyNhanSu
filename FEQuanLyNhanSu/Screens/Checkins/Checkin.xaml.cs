using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
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
using Xceed.Wpf.Toolkit.Panels;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Checkins
{
    /// <summary>
    /// Interaction logic for Checkin.xaml
    /// </summary>
    public partial class Checkin : Window
    {
        private Action<CheckinResultDto> _onCheckinCreated;
        public Checkin(Action<CheckinResultDto> onCheckinCreated)
        {
            InitializeComponent();
            _onCheckinCreated = onCheckinCreated;
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
        }


        private async Task HandleUI(string userRole)
        {
            switch (userRole)
            {
                case "Admin":
                    cbEmployee.Visibility = Visibility.Visible;
                    lblName.Visibility = Visibility.Visible;
                    await LoadUsers();
                    break;

                case "Manager":
                    cbEmployee.Visibility = Visibility.Visible;
                    lblName.Visibility = Visibility.Visible;
                    await LoadUsers();
                    break;

                case "Employee":
                    cbEmployee.Visibility = Visibility.Collapsed;
                    lblName.Visibility = Visibility.Collapsed;
                    break;
            }
        }

       
        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/Checkin";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            HttpContent content = null;

            var selectedUser = cbEmployee.SelectedItem as UserResultDto;
            if (selectedUser != null)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(selectedUser.UserId.ToString()), "userId");
                content = form;
            }
       

            var response = await client.PostAsync(baseUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CheckinResultDto>>(json);
                if (apiResponse?.Data != null)
                {
                    lblCheckinMor.Content = apiResponse.Data.CheckinMorning;
                    lblCheckoutMor.Content = apiResponse.Data.CheckoutMorning;
                    lblCheckinAft.Content = apiResponse.Data.CheckinAfternoon;
                    lblCheckoutAft.Content = apiResponse.Data.CheckoutAfternoon;
                    lblCheckinMorStatus.Content = apiResponse.Data.CheckinMorningStatus;
                    lblCheckoutMorStatus.Content = apiResponse.Data.CheckoutMorningStatus;
                    lblCheckinAftStatus.Content = apiResponse.Data.CheckinAfternoonStatus;
                    lblCheckoutAftStatus.Content = apiResponse.Data.CheckoutAfternoonStatus;
                    lblSalaryPerDay.Content = apiResponse.Data.SalaryPerDay.ToString("N0");

                    _onCheckinCreated?.Invoke(apiResponse.Data);
                    MessageBox.Show("Checkin thành công.");
                }
                else
                {
                    var apiResponseError = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponseError?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Checkin thành công nhưng không nhận được dữ liệu: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    if (!string.IsNullOrEmpty(errorResponse?.Data))
                    {
                        MessageBox.Show($"Checkin thất bại: {errorResponse.Data}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (!string.IsNullOrEmpty(errorResponse?.Message))
                    {
                        MessageBox.Show($"Checkin thất bại: {errorResponse.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                        MessageBox.Show($"Checkin thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch
                {
                    MessageBox.Show($"Checkin thất bại: {json}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                string keyword = cbEmployee.Text.Trim();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadUsers();
                    cbEmployee.SelectedItem = null;
                    cbEmployee.IsDropDownOpen = true;
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
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
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                cbEmployee.SelectedItem = null;
                cbEmployee.IsDropDownOpen = true;
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show("Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }
    }
}
