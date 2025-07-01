using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Positions;
using Newtonsoft.Json;
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
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption3.xaml
    /// </summary>
    public partial class PagePosition : Page
    {
        private PaginationHelper<Positions.PositionDTO> _paginationHelper;
        public PagePosition()
        {
            InitializeComponent();
            LoadPosition();
        }

        public void LoadPosition()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<Positions.PositionDTO>(
                baseUrl,
                pageSize,
                token,
                items => PositionDtaGrid.ItemsSource = items,
                txtPage
            );

            _ = _paginationHelper.LoadPageAsync(1);
        }

        private void AddPosition(object sender, RoutedEventArgs e)
        {
            var window = new CreatePosition();
            window.Show();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var editWindow = new UpdatePosition(positionId);
                editWindow.ShowDialog(); 
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa chức vụ này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeletePositionAsync(positionId);
                }
            }
        }

        private async Task DeletePositionAsync(Guid positionId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position/" + positionId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa chức vụ thành công.");
                LoadPosition(); 
            }
            else
            {
                MessageBox.Show("Không thể xóa chức vụ. Vui lòng thử lại sau.");
            }
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
                return;

            var result = await SearchHelper.SearchAsync<PositionDTO>("api/Position", keyword, token);

            PositionDtaGrid.ItemsSource = result;
        }


        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();
        }
    }
}
