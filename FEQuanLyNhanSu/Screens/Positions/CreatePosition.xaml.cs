using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.ResponseModels;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu.Screens.Positions
{
    /// <summary>
    /// Interaction logic for CreatePosition.xaml
    /// </summary>
    public partial class CreatePosition : Window
    {
        private Action<PositionResultDto> _onPositionCreated;
        public CreatePosition(Action<PositionResultDto> onCreated)
        {
            InitializeComponent();
            _onPositionCreated = onCreated;
            Authorize(Application.Current.Properties["UserRole"]?.ToString());
            HandleManagerUI();
        }
        private void HandleManagerUI()
        {
            var role = Application.Current.Properties["UserRole"]?.ToString();
            if (role == "Manager")
            {
                cbDepartment.Visibility = Visibility.Collapsed;
            }
        }
        private void Authorize(string role)
        {
            if (role == "Manager")
            {
                cbDepartment.Visibility = Visibility.Collapsed;
                txtDepartment.Visibility = Visibility.Hidden;
            }
            else if (role == "Administrator")
            {
                // Chỉ Admin mới được gọi API lấy danh sách phòng ban
                _ = LoadDepartmentsAsync();
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

                    cbDepartment.ItemsSource = apiResult.Data.Items;
                    cbDepartment.DisplayMemberPath = "Name";
                    cbDepartment.SelectedValuePath = "DepartmentId";
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Không thể tải danh sách phòng ban: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void cbDepartment_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                //var comboBox = sender as ComboBox;
                //if (comboBox?.Text == null)
                //{
                //    cbDepartment.ItemsSource = null;
                //    return;
                //}

                //var keyword = comboBox.Text.Trim();
                //if (keyword == "")
                //{
                //    cbDepartment.ItemsSource = null;
                //    return;
                //}
                var keyword = cbDepartment.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                    cbDepartment.ItemsSource = result.Data.Items;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    cbDepartment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnExit.IsEnabled = false;
            btnCreate.IsEnabled = false;
            try
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

                if (role == "Administrator")
                {
                    var selectedDepartment = cbDepartment.SelectedItem as DepartmentResultDto;
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

                //if (response.IsSuccessStatusCode)
                //{
                //    MessageBox.Show("Tạo chức vụ thành công.");
                //    _onPositionCreated?.Invoke();
                //    this.Close();
                //}
                //else
                //{
                //    MessageBox.Show("Tạo chức vụ thất bại.");
                //}
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<PositionResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onPositionCreated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Tạo chức vụ thành công.");
                    this.Close();
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Tạo chức vụ thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                return;

            this.Close();
        }
    }
}
