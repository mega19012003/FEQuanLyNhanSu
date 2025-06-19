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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models.Configs;
using FEQuanLyNhanSu.ResponseModels;
using Newtonsoft.Json;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageConfig.xaml
    /// </summary>
    public partial class PageConfig : Page
    {
        public PageConfig()
        {
            InitializeComponent();
            LoadCheckinStatus();
            LoadIpConfig();
            LoadHoliday();
        }

        private async void LoadIpConfig()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string url = $"https://demonhanvienapi.duckdns.org/api/AllowedIP";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var apiReuslt = JsonConvert.DeserializeObject<ApiResponse<PagedResult<AllowedIPs.IPDto>>>(responseStr);
                    IPDtaGrid.ItemsSource = apiReuslt.Data.Items;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách IP. Vui lòng thử lại sau.");
                }
            }
        }

        private async void LoadCheckinStatus()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string url = $"https://demonhanvienapi.duckdns.org/api/CheckinStatusConfig";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonConvert.DeserializeObject<ApiResponse<List<CheckinStatusConfig>>>(responseStr);
                    ChknDtaGrid.ItemsSource = apiResult.Data;
                }
                else
                {
                    MessageBox.Show("Không thể tải cấu hình. Vui lòng thử lại sau.");
                }
            }
        }

        private async void LoadHoliday()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string url = $"https://demonhanvienapi.duckdns.org/api/Holiday";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<Holidays.HolidayDto>>>(responseStr);
                    HolidayDtaGrid.ItemsSource = apiResult.Data.Items;
                }
                else
                {
                    MessageBox.Show("Không thể tải cấu hình. Vui lòng thử lại sau.");
                }
            }
        }
    }
}
