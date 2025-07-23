using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Departments;
using FEQuanLyNhanSu.Screens.Departments;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadDepartment();
            _ = LoadCompanies();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "SystemAdmin":
                    DtaGridActionDepartment.Visibility = Visibility.Collapsed;
                    AddDprtmntBtn.Visibility = Visibility.Collapsed;
                    break;

                case "Administrator":
                    cbCompany.Visibility = Visibility.Collapsed;
                    //DtaGridColumnCompany.Visibility = Visibility.Collapsed;
                    break;
            }
        }


        // btnAdd_Click
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateDepartment(OnDepartmentCreated);
            window.Show();
        }
        private void OnDepartmentCreated(Departments.DepartmentResultDto newComp)
        {
            if (newComp != null)
            {
                var list = DprtmtDtaGrid.ItemsSource as List<Departments.DepartmentResultDto> ?? new List<Departments.DepartmentResultDto>();
                list.Insert(0, newComp); 
                DprtmtDtaGrid.ItemsSource = null;
                DprtmtDtaGrid.ItemsSource = list;

                DprtmtDtaGrid.SelectedItem = newComp;
                DprtmtDtaGrid.ScrollIntoView(newComp);
            }
        }


        //Update
        private void OnDepartmentUpdated(Departments.DepartmentResultDto updatedComp)
        {
            if (updatedComp != null)
            {
                var list = DprtmtDtaGrid.ItemsSource as List<Departments.DepartmentResultDto> ?? new List<Departments.DepartmentResultDto>();

                // Xoá phòng ban cũ có cùng Id
                var existing = list.FirstOrDefault(d => d.DepartmentId == updatedComp.DepartmentId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                // Thêm mới lên đầu
                list.Insert(0, updatedComp);

                // Cập nhật lại ItemsSource
                DprtmtDtaGrid.ItemsSource = null;
                DprtmtDtaGrid.ItemsSource = list;

                // Highlight dòng vừa update
                DprtmtDtaGrid.SelectedItem = updatedComp;
                DprtmtDtaGrid.ScrollIntoView(updatedComp);
            }
        }
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
        private async Task LoadCompanies()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<CompanyResultDto>>>(json);
                cbCompany.ItemsSource = result.Data.Items;

                if (result.Data.Items != null && result.Data.Items.Any())
                {
                    cbCompany.SelectedItem = result.Data.Items.First();
                    //await LoadDepartmentByCompanyAsync();
                    //await LoadPositionByCompanyAsync();
                    await FilterAsync();
                }
            }
        }


        //Search
        private async Task FilterAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            string keyword = txtSearch.Text?.Trim();

            Guid? CompanyId = null;
            string departmentText = cbCompany.Text?.Trim();

            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
            {
                CompanyId = selectedComp.CompanyId;
            }
            else if (!string.IsNullOrEmpty(departmentText))
            {
                var found = (cbCompany.ItemsSource as IEnumerable<CompanyResultDto>)
                    ?.FirstOrDefault(d => d.Name.Equals(departmentText, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    CompanyId = found.CompanyId;
                }
            }

            var items = await SearchAndFilterDepartmentsAsync(
                baseUrl,
                token,
                keyword,
                CompanyId
            );

            DprtmtDtaGrid.ItemsSource = null;
            DprtmtDtaGrid.ItemsSource = items;
        }
        public static async Task<List<DepartmentResultDto>> SearchAndFilterDepartmentsAsync(string baseUrl, string token, string searchKeyword, Guid? CompanyId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (CompanyId.HasValue)
                    parameters.Add($"companyId={CompanyId}");
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/Department";
                if (parameters.Any())
                    url += "?" + string.Join("&", parameters);

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API Error: {response.StatusCode}");
                    return new List<DepartmentResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<DepartmentResultDto>>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data?.Items?.ToList() ?? new List<DepartmentResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<DepartmentResultDto>();
            }
        }
        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();
        private async void cbCompany_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbCompany_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompany.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();
                    cbCompany.SelectedItem = null;
                    cbCompany.IsDropDownOpen = true;

                    LoadDepartment();
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<CompanyResultDto>>>(json);
                        cbCompany.ItemsSource = result.Data.Items;

                        cbCompany.SelectedItem = null;
                        cbCompany.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbCompany.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private async Task DeleteDepartmentAsync(Guid DepartmentId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department/" + DepartmentId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa phòng ban thành công.");
                //_ = _paginationHelper.LoadPageAsync(1); // Reload the first page
                await FilterAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể xóa phòng ban: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
