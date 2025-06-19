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
using FEQuanLyNhanSu.Screens.Payrolls;
using Newtonsoft.Json;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PagePayroll.xaml
    /// </summary>
    public partial class PagePayroll : Page
    {
        public PagePayroll()
        {
            InitializeComponent();
            LoadPayrolls();
        }

        private void AddPayroll(object sender, RoutedEventArgs e)
        {
            var window = new CreatePayroll();
            window.Show();
        }
        private async void LoadPayrolls()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                string url = $"https://demonhanvienapi.duckdns.org/api/Payroll";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var apiReuslt = JsonConvert.DeserializeObject<ApiResponse<PagedResult<Payrolls.PayrollDto>>>(responseStr);
                    PayrollDtaGrid.ItemsSource = apiReuslt.Data.Items;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách checkin. Vui lòng thử lại sau.");
                }
            }
        }
    }
}
