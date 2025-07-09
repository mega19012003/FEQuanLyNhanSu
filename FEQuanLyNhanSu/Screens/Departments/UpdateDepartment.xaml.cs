using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;

namespace FEQuanLyNhanSu.Screens.Departments
{
    /// <summary>
    /// Interaction logic for UpdateDepartment.xaml
    /// </summary>
    public partial class UpdateDepartment : Window
    {
        private Guid _departmentId;
        private Action<DepartmentResultDto> _onDepartmentUpdated;
        public UpdateDepartment(Guid departmentId, Action<DepartmentResultDto> onUpdated)
        {
            InitializeComponent();
            _departmentId = departmentId;
            _onDepartmentUpdated = onUpdated;
            _ = LoadDepartmentAsync();
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
            //if (response.IsSuccessStatusCode)
            //{
            //    MessageBox.Show("Cập nhật phòng ban thành công.");
            //    _onDepartmentUpdated?.Invoke();
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show("Cập nhật phòng ban thất bại. Vui lòng thử lại sau.");
            //}
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DepartmentResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data != null)
                {
                    _onDepartmentUpdated?.Invoke(apiResponse.Data);
                }

                MessageBox.Show("Cập nhật phòng ban thành công.");
                this.Close();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Cập nhật phòng ban thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải thông tin chức vụ: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
