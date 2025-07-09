using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Checkins
{
    /// <summary>
    /// Interaction logic for Checkout.xaml
    /// </summary>
    public partial class Checkout : Window
    {
        private Action<CheckinResultDto> _onCheckoutCreated;
        public Checkout(Action<CheckinResultDto> onCheckoutCreated)
        {
            InitializeComponent();
            _onCheckoutCreated = onCheckoutCreated;
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
        }

        private async void HandleUI(string userRole)
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

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/Chekout"; 

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

            var response = await client.PutAsync(baseUrl, content);
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

                    _onCheckoutCreated?.Invoke(apiResponse.Data);
                    MessageBox.Show("Checkin thành công.");
                }
                else
                {
                    MessageBox.Show("Checkin thành công nhưng không nhận được dữ liệu.");
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
                        MessageBox.Show($"Checkin thất bại: {json}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch
                {
                    MessageBox.Show($"Checkin thất bại: {json}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
