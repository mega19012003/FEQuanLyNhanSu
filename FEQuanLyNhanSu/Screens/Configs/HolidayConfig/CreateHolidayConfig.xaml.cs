using FEQuanLyNhanSu.Base;
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
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                string name = txtName.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Vui lòng nhập tên ngày lễ");
                    return;
                }

                var startDate = dpStartDate.SelectedDate;
                var endDate = dpEndDate.SelectedDate;
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);
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
                    startDate = startDateOnly,
                    endDate = endDateOnly
                };

                var json = JsonConvert.SerializeObject(holiday);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync($"{baseUrl}/api/Holiday", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<HolidayResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onHolidayCreated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Tạo ngày lễ thành công.");
                    this.Close();
                }
                else
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();

                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<HolidayResponse>(
                        jsonResult,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    var errorData = apiResponse?.Data?.ToString() ?? apiResponse?.Message ?? "Có lỗi xảy ra";

                    MessageBox.Show($"Tạo ngày lễ thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
            }
        }
    }
}
