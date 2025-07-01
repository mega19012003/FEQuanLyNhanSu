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
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using Newtonsoft.Json;
using static FEQuanLyNhanSu.ResponseModels.Departments;

namespace FEQuanLyNhanSu.Screens.Positions
{
    /// <summary>
    /// Interaction logic for CreatePosition.xaml
    /// </summary>
    public partial class CreatePosition : Window
    {
        public CreatePosition()
        {
            InitializeComponent();

            var role = Application.Current.Properties["UserRole"]?.ToString();

            if (role == "Manager")
            {
                cmbDepartment.Visibility = Visibility.Collapsed;
                txtDepartment.Visibility = Visibility.Hidden;
            }
            else if (role == "Administrator")
            {
                // Chỉ Admin mới được gọi API lấy danh sách phòng ban
                _ = LoadDepartmentsAsync();
            }

            HandleManagerUI();
        }

        private void HandleManagerUI()
        {
            var role = Application.Current.Properties["UserRole"]?.ToString();
            if (role == "Manager")
            {
                cmbDepartment.Visibility = Visibility.Collapsed;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            this.Close();
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên chức vụ.");
                return;
            }

            var role = Application.Current.Properties["UserRole"]?.ToString();
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            var query = $"Name={Uri.EscapeDataString(name)}";

            // Chỉ Admin cần truyền thêm DepartmentId
            if (role == "Administrator")
            {
                var selectedDepartment = cmbDepartment.SelectedItem as DepartmentResultDto;
                if (selectedDepartment == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng ban.");
                    return;
                }

                query += $"&DepartmentId={selectedDepartment.DepartmentId}";
            }

            var url = $"{baseUrl}/api/Position?{query}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Tạo chức vụ thành công.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Tạo chức vụ thất bại.");
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var url = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);

                    cmbDepartment.ItemsSource = apiResult.Data.Items;
                    cmbDepartment.DisplayMemberPath = "Name";
                    cmbDepartment.SelectedValuePath = "DepartmentId";
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách phòng ban.");
                }
            }
        }
    }
}
