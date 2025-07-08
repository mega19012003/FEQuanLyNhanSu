using FEQuanLyNhanSu.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Xml.Linq;
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;
using static FEQuanLyNhanSu.ResponseModels.Departments;

namespace FEQuanLyNhanSu.Screens.Configs
{
    /// <summary>
    /// Interaction logic for CreateIPConfig.xaml
    /// </summary>
    public partial class CreateIPConfig : Window
    {
        private Action<IPResultDto> _onIPConfigCreated;
        public CreateIPConfig(Action<IPResultDto> onCreated)
        {
            InitializeComponent();
            _onIPConfigCreated = onCreated;
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ipAddress = txtIP.Text.Trim();
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    MessageBox.Show("Vui lòng nhập địa chỉ IP.");
                    return;
                }

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var query = $"IPAddress={Uri.EscapeDataString(ipAddress)}";
                var url = $"{baseUrl}/api/AllowedIP?{query}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(url, null);

                //if (response.IsSuccessStatusCode)
                //{
                //    MessageBox.Show("Tạo cấu hình IP thành công.");
                //    _onIPConfigCreated?.Invoke();
                //    this.Close();
                //}
                //else
                //{
                //    MessageBox.Show("Tạo cấu hình IP thất bại.");
                //}
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<IPResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onIPConfigCreated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Tạo cấu hình IP thành công.");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Tạo cấu hình IP thất bại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}");
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
