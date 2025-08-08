using EmployeeAPI.Models;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.Models.Positions;
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
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Companies
{
    /// <summary>
    /// Interaction logic for PageCompany.xaml
    /// </summary>
    public partial class PageCompany : Page
    {
        private PaginationHelper<CompanyResultDto> _paginationHelper;
        public PageCompany()
        {
            InitializeComponent();
            LoadIsActive();
            //LoadCompany();
            Loaded += async (s, e) => await FilterAsync();
        }
        // btnAdd_Click
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateCompany(OnCompanyCreated);
            window.ShowDialog();
        }
        private void OnCompanyCreated(CompanyResultDto newComp)
        {
            if (newComp != null)
            {
                var list = CompDtaGrid.ItemsSource as List<CompanyResultDto> ?? new List<CompanyResultDto>();
                list.Insert(0, newComp);
                CompDtaGrid.ItemsSource = null;
                CompDtaGrid.ItemsSource = list;

                CompDtaGrid.SelectedItem = newComp;
                CompDtaGrid.ScrollIntoView(newComp);
            }
        }
        private void OnCompanyUpdated(CompanyResultDto UpdatedComp)
        {
            if (UpdatedComp != null)
            {
                var list = CompDtaGrid.ItemsSource as List<CompanyResultDto> ?? new List<CompanyResultDto>();

                var existing = list.FirstOrDefault(d => d.CompanyId == UpdatedComp.CompanyId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, UpdatedComp);

                CompDtaGrid.ItemsSource = null;
                CompDtaGrid.ItemsSource = list;

                CompDtaGrid.SelectedItem = UpdatedComp;
                CompDtaGrid.ScrollIntoView(UpdatedComp);
            }
        }
        //Update
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid CompanyId)
            {
                var editWindow = new UpdateCompany(CompanyId, OnCompanyUpdated);
                editWindow.ShowDialog();
            }
        }
        //Load Company List
        private async Task LoadCompany()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";
                int pageSize = 20;

                _paginationHelper = new PaginationHelper<CompanyResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => CompDtaGrid.ItemsSource = items,
                    txtPage,
                    page => BuildCompanyUrlWithFilter(page, pageSize)
                );
                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string BuildCompanyUrlWithFilter(int pageIndex, int pageSize)
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";
            var parameters = new List<string>();

            string keyword = txtSearch.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
                parameters.Add($"Search={Uri.EscapeDataString(keyword)}");

            if (cbIsActive.SelectedItem is ComboBoxItem selectedStatus)
            {
                var tagValue = selectedStatus.Tag?.ToString();
                if (!string.IsNullOrEmpty(tagValue))
                    parameters.Add($"IsActive={tagValue.ToLower()}");
            }

            parameters.Add($"pageIndex={pageIndex}");
            parameters.Add($"pageSize={pageSize}");

            return parameters.Any() ? $"{baseUrl}?{string.Join("&", parameters)}" : baseUrl;
        }
        private async Task FilterAsync()
        {
            await LoadCompany();
        }
        //Search
        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            string keyword = txtSearch.Text?.Trim();

            if (keyword == null)
                LoadCompany();
            else
            {
                var result = await SearchHelper.SearchAsync<CompanyResultDto>("api/Company", keyword, token);
                CompDtaGrid.ItemsSource = result;
            }
        }
        //Delete
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid CompanyId)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa công ty này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _ = DeleteCompanyAsync(CompanyId);
                }
            }
        }
        private async Task DeleteCompanyAsync(Guid CompanyId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company/" + CompanyId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa công ty thành công.");
                //_ = _paginationHelper.LoadPageAsync(1);
                await _paginationHelper.RefreshAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể xóa công ty: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // cbIsActive
        private void LoadIsActive()
        {
            cbIsActive.Items.Clear();

            cbIsActive.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = "" });
            cbIsActive.Items.Add(new ComboBoxItem { Content = "Đang hoạt động", Tag = "true" });
            cbIsActive.Items.Add(new ComboBoxItem { Content = "Không hoạt động", Tag = "false" });

            cbIsActive.SelectedIndex = 1; 
        }
        private async void cbIsActive_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();

        private async void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompanies = new List<CompanyResultDto>();
            foreach (var item in CompDtaGrid.Items)
            {
                var row = (DataGridRow)CompDtaGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(row);
                    if (checkbox != null && checkbox.IsChecked == true)
                    {
                        selectedCompanies.Add(item as CompanyResultDto);
                    }
                }
            }

            if (selectedCompanies.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một công ty để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa {selectedCompanies.Count} công ty?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            var token = Application.Current.Properties["Token"]?.ToString();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            bool hasError = false;

            foreach (var company in selectedCompanies)
            {
                var url = $"{AppsettingConfigHelper.GetBaseUrl()}/api/Company/{company.CompanyId}";
                var response = await client.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    hasError = true;
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Lỗi khi xóa công ty: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!hasError)
            {
                MessageBox.Show("Xóa thành công các công ty đã chọn.");
            }
            //await _paginationHelper.RefreshAsync();
            //_ = _paginationHelper.LoadPageAsync(1);
            await FilterAsync();
        }
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
