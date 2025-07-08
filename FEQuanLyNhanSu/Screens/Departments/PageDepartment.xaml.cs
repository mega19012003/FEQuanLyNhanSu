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
            LoadDepartment();
        }

        // Sửa btnAdd_Click
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateDepartment(OnDepartmentCreated);
            window.Show();
        }

        private void OnDepartmentCreated(Departments.DepartmentResultDto newDept)
        {
            if (newDept != null)
            {
                var list = DprtmtDtaGrid.ItemsSource as List<Departments.DepartmentResultDto> ?? new List<Departments.DepartmentResultDto>();
                list.Insert(0, newDept); 
                DprtmtDtaGrid.ItemsSource = null;
                DprtmtDtaGrid.ItemsSource = list;

                DprtmtDtaGrid.SelectedItem = newDept;
                DprtmtDtaGrid.ScrollIntoView(newDept);
            }
        }

        private void OnDepartmentUpdated(Departments.DepartmentResultDto updatedDept)
        {
            if (updatedDept != null)
            {
                var list = DprtmtDtaGrid.ItemsSource as List<Departments.DepartmentResultDto> ?? new List<Departments.DepartmentResultDto>();

                // Xoá phòng ban cũ có cùng Id
                var existing = list.FirstOrDefault(d => d.DepartmentId == updatedDept.DepartmentId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                // Thêm mới lên đầu
                list.Insert(0, updatedDept);

                // Cập nhật lại ItemsSource
                DprtmtDtaGrid.ItemsSource = null;
                DprtmtDtaGrid.ItemsSource = list;

                // Highlight dòng vừa update
                DprtmtDtaGrid.SelectedItem = updatedDept;
                DprtmtDtaGrid.ScrollIntoView(updatedDept);
            }
        }
        //Update
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid DepartmentId)
            {
                var editWindow = new UpdateDepartment(DepartmentId, OnDepartmentUpdated);
                editWindow.ShowDialog();
            }
        }

        //Load Department List
        private void LoadDepartment()
        {
            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Search
        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            string keyword = txtSearch.Text?.Trim();

            if (keyword == null)
                LoadDepartment();
            else
            {
                var result = await SearchHelper.SearchAsync<Departments.DepartmentResultDto>("api/Department", keyword, token);
                DprtmtDtaGrid.ItemsSource = result;
            }
        }



        //Delete
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

        //Pagination
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
