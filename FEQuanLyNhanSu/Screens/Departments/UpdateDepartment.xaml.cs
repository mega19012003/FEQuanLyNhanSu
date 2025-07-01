using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

namespace FEQuanLyNhanSu.Screens.Departments
{
    /// <summary>
    /// Interaction logic for UpdateDepartment.xaml
    /// </summary>
    public partial class UpdateDepartment : Window
    {
        private Guid _departmentId;
        public UpdateDepartment(Guid departmentId)
        {
            InitializeComponent();
            _departmentId = departmentId;
            _ = LoadDepartmentAsync();
        }

        private async Task LoadDepartmentAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Department/{_departmentId}";

            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<Departments.UpdateDepartment>>(json);

                txtName.Text = result.Data.Name;

            }
            else
            {
                MessageBox.Show("Không thể tải thông tin chức vụ.");
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
            var name = txtName.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Tên phòng ban không được để trống.");
                return;
            }

            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var token = Application.Current.Properties["Token"]?.ToString();
            var query = $"newName={Uri.EscapeDataString(name)}";
            var url = $"{baseUrl}/api/Department?id={_departmentId}&{query}";

            using var client = CreateAuthorizedClient(token);
            var response = await client.PutAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cập nhật phòng ban thành công.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Cập nhật phòng ban thất bại. Vui lòng thử lại sau.");
            }
        }
    }
}
