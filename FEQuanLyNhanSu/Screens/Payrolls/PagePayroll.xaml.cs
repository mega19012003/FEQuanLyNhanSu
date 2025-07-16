using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Payrolls;
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
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Payrolls;
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PagePayroll.xaml
    /// </summary>
    public partial class PagePayroll : Page
    {

        private PaginationHelper<Payrolls.PayrollResultDto> _paginationHelper;
        public PagePayroll()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadPayroll();
            LoadDateComboboxes();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "Administrator":
                    cbCompany.Visibility = Visibility.Collapsed;
                    break; 
                case "Manager":
                    cbCompany.Visibility = Visibility.Collapsed;
                    break;
                case "Employee":
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    DtaGridAction.Visibility = Visibility.Collapsed;
                    cbCompany.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Chấm công";
                    //AddAllPayrollBtn.Visibility = Visibility.Collapsed;
                    break;
                case "SystemAdmin":
                    DtaGridAction.Visibility = Visibility.Collapsed;
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    LoadCompanies();
                    break;
            }
        }
        private void LoadPayroll()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll";
                int pageSize = 20;
                _paginationHelper = new PaginationHelper<Payrolls.PayrollResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => PayrollDtaGrid.ItemsSource = items,
                    txtPage
                );
                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu bảng lương: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
        }
        private void LoadDateComboboxes()
        {
            var days = new List<string> { "Ngày" };
            days.AddRange(Enumerable.Range(1, 31).Select(i => i.ToString()));
            cbDay.ItemsSource = days;

            var months = new List<string> { "Tháng" };
            months.AddRange(Enumerable.Range(1, 12).Select(i => i.ToString()));
            cbMonth.ItemsSource = months;

            int currentYear = DateTime.Now.Year;
            var years = new List<string> { "Năm" };
            years.AddRange(Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse());
            cbYear.ItemsSource = years;

            cbDay.SelectedIndex = 0;
            cbMonth.SelectedIndex = 0;
            cbYear.SelectedIndex = 0;
        }
        
        private async Task FilterAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            string keyword = txtSearch.Text?.Trim();
            int? day = cbDay.SelectedIndex > 0 ? int.Parse(cbDay.SelectedItem.ToString()) : (int?)null;
            int? month = cbMonth.SelectedIndex > 0 ? int.Parse(cbMonth.SelectedItem.ToString()) : (int?)null;
            int? year = cbYear.SelectedIndex > 0 ? int.Parse(cbYear.SelectedItem.ToString()) : (int?)null;

            Guid? companyId = null;
            string companyText = cbCompany.Text?.Trim();
            if (cbCompany.SelectedItem is CompanyResultDto selectedCompany)
            {
                companyId = selectedCompany.CompanyId;
            }
            else if (!string.IsNullOrEmpty(companyText))
            {
                var found = (cbCompany.ItemsSource as IEnumerable<CompanyResultDto>)
                    ?.FirstOrDefault(c => c.Name.Equals(companyText, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                    companyId = found.CompanyId;
            }

            var items = await SearchAndFilterCheckinsAsync(baseUrl, token, keyword, day, month, year, companyId);
            PayrollDtaGrid.ItemsSource = items;
        }
        public static async Task<List<PayrollResultDto>> SearchAndFilterCheckinsAsync(string baseUrl, string token, string searchKeyword, int? day, int? month, int? year, Guid? companyId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (day.HasValue) parameters.Add($"Day={day}");
                if (month.HasValue) parameters.Add($"Month={month}");
                if (year.HasValue) parameters.Add($"Year={year}");
                if (companyId.HasValue) parameters.Add($"companId={year}");
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/Payroll";
                if (parameters.Any()) url += "?" + string.Join("&", parameters);

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API Error: {response.StatusCode}");
                    return new List<PayrollResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<PayrollResultDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Items?.ToList() ?? new List<PayrollResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<PayrollResultDto>();
            }
        }

        private async void cbCompany_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbCompany.Text.Trim();

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCompanies();
                    cbCompany.SelectedItem = null;
                    cbCompany.IsDropDownOpen = true;
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
                MessageBox.Show($"Lỗi khi tìm kiếm công ty: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbCompany_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();

        private void AddPayroll(object sender, RoutedEventArgs e)
        {
            var window = new CreatePayroll(LoadPayroll);
            window.Show();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid PayrollId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa bảng lương này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteAsync(PayrollId);
                }
            }
        }
        private async Task DeleteAsync(Guid PayrollId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll/" + PayrollId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa cấu  bảng lương thành công.");
                _paginationHelper.RefreshAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể xóa bảng lương. Vui lòng thử lại sau: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();
        }
        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }
    }
}
