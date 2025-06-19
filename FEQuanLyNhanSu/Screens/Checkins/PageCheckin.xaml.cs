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
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Services;
using Newtonsoft.Json;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption5.xaml
    /// </summary>
    public partial class PageCheckin : Page
    {
        public PageCheckin()
        {
            InitializeComponent();
            LoadCheckins();
        }

        private async void LoadCheckins()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string url = $"https://demonhanvienapi.duckdns.org/api/Checkin";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var apiReuslt = JsonConvert.DeserializeObject<ApiResponse<PagedResult<Checkins.CheckinDto>>>(responseStr);
                    CheckinDtaGrid.ItemsSource = apiReuslt.Data.Items;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách checkin. Vui lòng thử lại sau.");
                }
            }
        }
    }
}
