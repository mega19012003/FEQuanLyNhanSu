using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.ApiResonses;
using FEQuanLyNhanSu.Models.Users;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Payrolls;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Payrolls
{
    /// <summary>
    /// Interaction logic for CreatePayroll.xaml
    /// </summary>
    public partial class CreatePayroll : Window
    {
        private Action<PayrollResultDto> _onPayrollCreated;
        public CreatePayroll(Action<PayrollResultDto> onPayrollCreated)
        {
            InitializeComponent();
            _onPayrollCreated = onPayrollCreated;
            _ = LoadUsers(); // có thể xóa nếu ko cần
        }

        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            string keyword = cbEmployee.Text.Trim();

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            //var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(keyword))
            {
                response = await client.GetAsync(baseUrl); // không có query Search
                cbEmployee.IsDropDownOpen = true;
            }
            else
            {
                response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
            }

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
            string keyword = cbEmployee.Text.Trim();
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
                cbEmployee.IsDropDownOpen = true;
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra khi load user";
                MessageBox.Show($"Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;

            try
            {
                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                if (selectedUser == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên từ danh sách");
                    return;
                }

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();

                var url = $"{baseUrl}/api/Payroll/calculate?userId={selectedUser.UserId}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PayrollResultDto>>(json);

                    if (result != null && result.Data != null)
                    {
                        //lblId.Content = result.Data.Id.ToString();
                        lblFullname.Content = result.Data.Name;
                        //lblSalary.Content = result.Data.Salary.ToString("N0");
                        lblDayWorked.Content = result.Data.DaysWorked.ToString();
                        lblNote.Content = result.Data.Note ?? "Không có ghi chú";
                        lblCreatedDate.Content = result.Data.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                    }
                    MessageBox.Show("Chấm công thành công.");
                    _onPayrollCreated?.Invoke(result.Data);
                    //this.Close();
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra tạo payroll";
                    MessageBox.Show($"Chấm công thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
