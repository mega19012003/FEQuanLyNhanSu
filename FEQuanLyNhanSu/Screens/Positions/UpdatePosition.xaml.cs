using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu.Screens.Positions
{
    /// <summary>
    /// Interaction logic for UpdatePosition.xaml
    /// </summary>
    public partial class UpdatePosition : Window
    {
        private Guid _positionId;
        private readonly Action _onPositionUpdated;
        public UpdatePosition(Guid positionId, Action onUpdated)
        {
            InitializeComponent();
            _positionId = positionId;
            _onPositionUpdated = onUpdated;
            _ = LoadPositionAsync();
        }

        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task LoadPositionAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Position/{_positionId}";

            //using var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", token);
            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PositionDTO>>(json);

                txtName.Text = result.Data.Name;

            }
            else
            {
                MessageBox.Show("Không thể tải thông tin chức vụ.");
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên chức vụ.");
                return;
            }

            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var token = Application.Current.Properties["Token"]?.ToString();
            var query = $"newName={Uri.EscapeDataString(name)}";
            var url = $"{baseUrl}/api/Position?id={_positionId}&{query}";
            //MessageBox.Show($"URL {url}");

            //using var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization =
            //    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var client = CreateAuthorizedClient(token);

            var response = await client.PutAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cập nhật chức vụ thành công.");
                _onPositionUpdated?.Invoke(); 
                this.Close();
            }
            else
            {
                MessageBox.Show("Cập nhật chức vụ thất bại.");
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
