using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Holidays;

namespace FEQuanLyNhanSu.Screens.Configs.HolidayConfig
{
    /// <summary>
    /// Interaction logic for CreateHoliday.xaml
    /// </summary>
    public partial class CreateHolidayConfig : Window
    {
        private Action<HolidayResultDto> _onHolidayCreated;
        public CreateHolidayConfig(Action<HolidayResultDto> onCreated)
        {
            InitializeComponent();
            _onHolidayCreated = onCreated;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            this.Close();
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên ngày lễ");
                return;
            }

            var startDate = dpStartDate.Value;
            var endDate = dpEndDate.Value;
            if (startDate == null || endDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu và kết thúc");
                return;
            }

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            var holiday = new
            {
                name = name,
                startDate = startDate,
                endDate = endDate
            };

            var json = JsonConvert.SerializeObject(holiday);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"{baseUrl}/api/Holiday", content);

            //if (response.IsSuccessStatusCode)
            //{
            //    MessageBox.Show("Tạo thêm ngày lễ thành công.");
            //    _onHolidayCreated?.Invoke();
            //    this.Close();
            //}
            //else
            //{
            //    var error = await response.Content.ReadAsStringAsync();
            //    MessageBox.Show($"Tạo ngày lễ thất bại: {error}");
            //}
            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<HolidayResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data != null)
                {
                    _onHolidayCreated?.Invoke(apiResponse.Data);
                }

                MessageBox.Show("Tạo phòng ban thành công.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Tạo phòng ban thất bại.");
            }
        }
    }
}
