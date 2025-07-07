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
        public Checkout()
        {
            InitializeComponent();
            _ = LoadUsers();
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
            var selectedUser = cbEmployee.SelectedItem as UserResultDto;
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/Chekout";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(selectedUser.UserId.ToString()), "userId");

            var response = await client.PutAsync(baseUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<CheckinResultDto>>(json);
                lblCheckinMor.Content = result.Data.CheckinMorning;
                lblCheckoutMor.Content = result.Data.CheckoutMorning;
                lblCheckinAft.Content = result.Data.CheckinAfternoon;
                lblCheckoutAft.Content = result.Data.CheckoutAfternoon;
                lblCheckinMorStatus.Content = result.Data.CheckinMorningStatus;
                lblCheckoutMorStatus.Content = result.Data.CheckoutMorningStatus;
                lblCheckinAftStatus.Content = result.Data.CheckinAfternoonStatus;
                lblCheckoutAftStatus.Content = result.Data.CheckoutAfternoonStatus;
                lblSalaryPerDay.Content = result.Data.SalaryPerDay.ToString("N0");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Check-out thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
