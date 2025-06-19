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
using FEQuanLyNhanSu.Screens.Departments;
using Newtonsoft.Json;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class PageDepartment : Page
    {
        public PageDepartment()
        {
            InitializeComponent();
            LoadDepartment();
        }

        private void AddDepartment(object sender, RoutedEventArgs e)
        {
            var window = new CreateDepartment();
            window.Show();
        }

        private async void LoadDepartment()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string url = $"https://demonhanvienapi.duckdns.org/api/Department";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var repsonseStr = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<Departments.DepartmentDto>>>(repsonseStr);
                    DprtmtDtaGrid.ItemsSource = apiResult.Data.Items;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách phòng ban. Vui lòng thử lại sau.");
                }
            }
        }
    }
}
