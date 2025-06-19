using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
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
using FEQuanLyNhanSu.Screens.Users;
using FEQuanLyNhanSu.Services.UserService;
using Newtonsoft.Json;
using static FEQuanLyNhanSu.Services.UserService.Users;
using FEQuanLyNhanSu.Base;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class PageUser : Page
    {
        private ObservableCollection<UserDto> users = new();
        public PageUser()
        {
            InitializeComponent();
            LoadUsersAsync();
        }
        private void CreateUser(object sender, RoutedEventArgs e)
        {
            var window = new CreateUser();  
            window.Show(); 
        }

        private async Task LoadUsersAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var role = Application.Current.Properties["UserRole"]?.ToString();
            var userId = Application.Current.Properties["UserId"]?.ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Tạo URL API
                string url = $"https://demonhanvienapi.duckdns.org/api/User";
                /*if (!string.IsNullOrEmpty(searchName))
                    url += $"&Name={searchName}";*/

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseStr = await response.Content.ReadAsStringAsync();

                    var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserDto>>>(responseStr);

                    UserDtaGrid.ItemsSource = apiResult.Data.Items;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách người dùng.");
                }
            }
        }

    }
}
