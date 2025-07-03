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
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Payrolls
{
    /// <summary>
    /// Interaction logic for CreatePayroll.xaml
    /// </summary>
    public partial class CreatePayroll : Window
    {
        private Action _onPayrollCreated;
        public CreatePayroll(Action onPayrollCreated)
        {
            InitializeComponent();
            _onPayrollCreated = onPayrollCreated;
            //_ = LoadUser();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }

        //private List<UserResultDto> _allUsers = new List<UserResultDto>();
        private UserResultDto _selectedUser;

        private async void txtSearchUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = txtSearchUser.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                lstUsers.ItemsSource = null;
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
                lstUsers.ItemsSource = result.Data.Items;
            }
            else
            {
                MessageBox.Show("Không thể tìm user.");
            }
        }

        private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = lstUsers.SelectedItem as UserResultDto;
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên từ danh sách");
                return;
            }

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            var url = $"{baseUrl}/api/Payroll/calculate?userId={_selectedUser.UserId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Tạo bảng lương thành công.");
                _onPayrollCreated?.Invoke();
                this.Close();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Tạo bảng lương thất bại: {error}");
            }
        }

    }
}
