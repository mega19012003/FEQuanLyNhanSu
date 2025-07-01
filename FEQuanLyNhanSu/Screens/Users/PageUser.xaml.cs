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
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json.Linq;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class PageUser : Page
    {
        private ObservableCollection<UserResultDto> users = new();
        private PaginationHelper<UserResultDto> _paginationHelper;

        public PageUser()
        {
            InitializeComponent();
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<UserResultDto>(
                baseUrl,
                pageSize,
                token,
                items => UserDtaGrid.ItemsSource = items,
                txtPage
            );

            _ = _paginationHelper.LoadPageAsync(1);
        }
        
        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();
        }

        private void CreateUser(object sender, RoutedEventArgs e)
        {
            var window = new CreateUser();
            window.Show();
        }
    }
}
