using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Checkins;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.Checkins;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption5.xaml
    /// </summary>
    public partial class PageCheckin : Page
    {
        private PaginationHelper<Checkins.CheckinResultDto> _paginationHelper;
        public PageCheckin()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadCheckin();
            LoadDateComboboxes();
        }

        private void OnCheckinCreated(Checkins.CheckinResultDto newDept)
        {
            if (newDept != null)
            {
                var list = CheckinDtaGrid.ItemsSource as List<Checkins.CheckinResultDto> ?? new List<Checkins.CheckinResultDto>();
                list.Insert(0, newDept);
                CheckinDtaGrid.ItemsSource = null;
                CheckinDtaGrid.ItemsSource = list;

                CheckinDtaGrid.SelectedItem = newDept;
                CheckinDtaGrid.ScrollIntoView(newDept);
            }
        }
        private void OnCheckinUpdated(Checkins.CheckinResultDto updatedDept)
        {
            if (updatedDept != null)
            {
                var list = CheckinDtaGrid.ItemsSource as List<Checkins.CheckinResultDto> ?? new List<Checkins.CheckinResultDto>();

                var existing = list.FirstOrDefault(d => d.CheckinId == updatedDept.CheckinId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedDept);

                CheckinDtaGrid.ItemsSource = null;
                CheckinDtaGrid.ItemsSource = list;

                CheckinDtaGrid.SelectedItem = updatedDept;
                CheckinDtaGrid.ScrollIntoView(updatedDept);
            }
        }
        private void AddCheckinBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new Checkin(OnCheckinCreated);
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddCheckouBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new Checkout(OnCheckinUpdated);
            window.Show();
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
                    DtaGridAction.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Chấm công";
                    cbCompany.Visibility = Visibility.Collapsed;
                    break;
                case "SystemAdmin":
                    DtaGridAction.Visibility = Visibility.Collapsed;
                    AddCheckinBtn.Visibility = Visibility.Collapsed;
                    AddCheckouBtn.Visibility = Visibility.Collapsed;
                    LoadCompanies();
                    break;
            }
        }
        private void LoadDateComboboxes()
        {
            var days = new List<string> { "Ngày" };
            days.AddRange(Enumerable.Range(0, 31).Select(i => i.ToString()));
            cbDay.ItemsSource = days;

            var months = new List<string> { "Tháng" };
            months.AddRange(Enumerable.Range(0, 12).Select(i => i.ToString()));
            cbMonth.ItemsSource = months;

            int currentYear = DateTime.Now.Year;
            var years = new List<string> { "Năm" };
            years.AddRange(Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse());
            cbYear.ItemsSource = years;

            cbDay.SelectedIndex = 0;
            cbMonth.SelectedIndex = 0;
            cbYear.SelectedIndex = 0;
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
        private void LoadCheckin()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<Checkins.CheckinResultDto>(
                baseUrl,
                pageSize,
                token,
                items => CheckinDtaGrid.ItemsSource = items,
                txtPage
            );

            _ = _paginationHelper.LoadPageAsync(1);
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
            CheckinDtaGrid.ItemsSource = items;
        }
        public static async Task<List<CheckinResultDto>> SearchAndFilterCheckinsAsync(string baseUrl, string token, string searchKeyword, int? day, int? month, int? year, Guid? companyId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (day.HasValue) parameters.Add($"Day={day}");
                if (month.HasValue) parameters.Add($"Month={month}");
                if (year.HasValue) parameters.Add($"Year={year}");
                if (companyId.HasValue) parameters.Add($"companyId={companyId}");
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/Checkin";
                if (parameters.Any()) url += "?" + string.Join("&", parameters);

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API Error: {response.StatusCode}");
                    return new List<CheckinResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<CheckinResultDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Items?.ToList() ?? new List<CheckinResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<CheckinResultDto>();
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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid checkinId)
            {
                var editWindow = new Update(OnCheckinUpdated, checkinId);
                editWindow.ShowDialog();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid checkinId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa checkin này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteCheckinAsync(checkinId);
                }
            }
        }
        private async Task DeleteCheckinAsync(Guid checkinId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + $"/api/Checkin/{checkinId}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa checkin thành công.");
                //LoadCheckin();
                await FilterAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Xóa checkin thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
