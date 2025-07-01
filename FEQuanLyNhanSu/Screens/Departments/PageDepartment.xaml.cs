using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Departments;
using FEQuanLyNhanSu.Screens.Positions;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class PageDepartment : Page
    {
        private PaginationHelper<Departments.DepartmentResultDto> _paginationHelper;
        public PageDepartment()
        {
            InitializeComponent();
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<Departments.DepartmentResultDto>(
                baseUrl,
                pageSize,
                token,
                items => DprtmtDtaGrid.ItemsSource = items,
                txtPage
            );
            _ = _paginationHelper.LoadPageAsync(1);
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            string keyword = txtSearch.Text?.Trim();

            if (keyword == null)
                return;

            var result = await SearchHelper.SearchAsync<Departments.DepartmentResultDto>("api/Department", keyword, token);

            DprtmtDtaGrid.ItemsSource = result;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateDepartment();
            window.Show();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid DepartmentId)
            {
                var editWindow = new UpdateDepartment(DepartmentId);
                editWindow.ShowDialog();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid DepartmentId)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa phòng ban này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _ = DeleteDepartmentAsync(DepartmentId);
                }
            }
        }

        private async Task DeleteDepartmentAsync(Guid departmentId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department/" + departmentId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa phòng ban thành công.");
                _ = _paginationHelper.LoadPageAsync(1); // Reload the first page
            }
            else
            {
                MessageBox.Show("Không thể xóa phòng ban. Vui lòng thử lại sau.");
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
