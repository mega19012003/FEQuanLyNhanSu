using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
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
using static FEQuanLyNhanSu.ResponseModels.Companies;

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
            LoadCompany();
        }
        // btnAdd_Click
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateCompany(OnCompanyCreated);
            window.Show();
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
        private void LoadCompany()
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
                MessageBox.Show("Xóa phòng ban thành công.");
                _ = _paginationHelper.LoadPageAsync(1); // Reload the first page
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
