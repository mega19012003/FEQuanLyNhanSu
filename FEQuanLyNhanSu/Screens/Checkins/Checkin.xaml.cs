using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection.Metadata;
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
                case "Administrator":
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
                    lblNote.Visibility = Visibility.Collapsed;
                    txtNote.Visibility = Visibility.Collapsed;
                    break;
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

                HttpResponseMessage response;

                if (string.IsNullOrEmpty(keyword))
                {
                    response = await client.GetAsync(baseUrl); // không có query Search
                    cbEmployee.IsDropDownOpen = true;
                }
                else
                {
                    /*var */response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
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
        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;

            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/Checkin";

                var form = new MultipartFormDataContent();
                var deviceInfo = Environment.MachineName;
                form.Add(new StringContent(deviceInfo), "DeviceInfo");
                var note = txtNote.Text ?? "";
                form.Add(new StringContent(note), "Note");

                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                if (selectedUser != null)
                {
                    form.Add(new StringContent(selectedUser.UserId.ToString()), "userId");
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(baseUrl, form);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CheckinResultDto>>(json);
                    if (apiResponse?.Data != null)
                    {
                        lblCheckinMor.Content = apiResponse.Data.CheckinTime;
                        lblCheckoutMor.Content = apiResponse.Data.CheckoutTime;
                        lblCheckinAft.Content = apiResponse.Data.LogStatus;
                        lblNoteResult.Content = apiResponse.Data.Note;

                        _onCheckinCreated?.Invoke(apiResponse.Data);
                        MessageBox.Show("Checkin thành công.");
                    }
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorMessage = errorResponse?.Data ?? errorResponse?.Message ?? "Có lỗi xảy ra.";
                    MessageBox.Show($"Checkin thất bại: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
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
