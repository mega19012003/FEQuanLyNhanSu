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
                    break; 
                case "Manager":
                    break;
                case "Employee":
                    AddPayrollBtn.Visibility = Visibility.Collapsed;
                    DtaGridAction.Visibility = Visibility.Collapsed;
                    AddAllPayrollBtn.Visibility = Visibility.Collapsed;
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

            var items = await SearchAndFilterCheckinsAsync(
                baseUrl,
                token,
                keyword,
                day,
                month,
                year
            );

            PayrollDtaGrid.ItemsSource = items;
        }

        public static async Task<List<PayrollResultDto>> SearchAndFilterCheckinsAsync(
            string baseUrl,
            string token,
            string searchKeyword,
            int? day,
            int? month,
            int? year,
            int pageIndex = 1,
            int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (day.HasValue) parameters.Add($"Day={day}");
                if (month.HasValue) parameters.Add($"Month={month}");
                if (year.HasValue) parameters.Add($"Year={year}");
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


        private async void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();
        //private async void txtTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var token = Application.Current.Properties["Token"].ToString();
        //    string keyword = txtSearch.Text?.Trim();

        //    if (string.IsNullOrWhiteSpace(keyword))
        //        LoadPayroll();
        //    else
        //    {
        //        var result = await SearchHelper.SearchAsync<Payrolls.PayrollResultDto>("api/Payroll", keyword, token);
        //        PayrollDtaGrid.ItemsSource = result;
        //    }
        //}

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

        private async void AddPayrollAll(object sender, RoutedEventArgs e)
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/Payroll/calculateAll";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(url, null);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<
                        ApiResponse<List<PayrollResultDto>>>(json, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (result?.Data != null && result.Data.Any())
                    {
                        MessageBox.Show("Chấm công thành công!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        var currentList = PayrollDtaGrid.ItemsSource as List<PayrollResultDto> ?? new List<PayrollResultDto>();
                        foreach (var item in result.Data)
                        {
                            var existed = currentList.FirstOrDefault(x => x.Id == item.Id && x.CreatedDate.Month == item.CreatedDate.Month && x.CreatedDate.Year == item.CreatedDate.Year && x.CreatedDate.Day == item.CreatedDate.Day);
                            if (existed != null)
                                currentList.Remove(existed);

                            currentList.Insert(0, item);
                        }

                        PayrollDtaGrid.ItemsSource = null; 
                        PayrollDtaGrid.ItemsSource = currentList;
                    }
                    else
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                        MessageBox.Show($"Không có dữ liệu mới: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"API lỗi: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
