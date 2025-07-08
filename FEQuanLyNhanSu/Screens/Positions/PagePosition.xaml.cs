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
        private PaginationHelper<Positions.PositionResultDto> _paginationHelper;
        public PagePosition()
        {
            InitializeComponent();
            LoadPosition();
        }

        private void OnPositionCreated(Positions.PositionResultDto newDept)
        {
            if (newDept != null)
            {
                var list = PositionDtaGrid.ItemsSource as List<Positions.PositionResultDto> ?? new List<Positions.PositionResultDto>();
                list.Insert(0, newDept);
                PositionDtaGrid.ItemsSource = null;
                PositionDtaGrid.ItemsSource = list;

                PositionDtaGrid.SelectedItem = newDept;
                PositionDtaGrid.ScrollIntoView(newDept);
            }
        }

        private void OnPositionUpdated(Positions.PositionResultDto updatedDept)
        {
            if (updatedDept != null)
            {
                var list = PositionDtaGrid.ItemsSource as List<Positions.PositionResultDto> ?? new List<Positions.PositionResultDto>();

                var existing = list.FirstOrDefault(d => d.Id == updatedDept.Id);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedDept);

                PositionDtaGrid.ItemsSource = null;
                PositionDtaGrid.ItemsSource = list;

                PositionDtaGrid.SelectedItem = updatedDept;
                PositionDtaGrid.ScrollIntoView(updatedDept);
            }
        }

        private void AddPosition(object sender, RoutedEventArgs e)
        {
            var window = new CreatePosition(OnPositionCreated);
            window.Show();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var editWindow = new UpdatePosition(positionId, OnPositionUpdated);
                editWindow.ShowDialog();
            }
        }


        public void LoadPosition()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";
                int pageSize = 20;

                _paginationHelper = new PaginationHelper<Positions.PositionResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => PositionDtaGrid.ItemsSource = items,
                    txtPage
                );

                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                LoadPosition();
            else
            {
                var result = await SearchHelper.SearchAsync<PositionResultDto>("api/Position", keyword, token);
                PositionDtaGrid.ItemsSource = result;
            }
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
