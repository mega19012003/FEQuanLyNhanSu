using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Duties;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption4.xaml
    /// </summary>
    public partial class PageDuty : Page
    {
        private PaginationHelper<DutyResultDto> _paginationHelper;
        private DispatcherTimer _debounceTimer;

        public PageDuty()
        {
            InitializeComponent();
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(600)
            };
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop(); // Dừng timer để tránh gọi lặp
                await FilterAsync();
            };
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadDuty();
            LoadDateComboboxes();
            Loaded += async (s, e) => await FilterAsync();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "Administrator":
                    AdDutyBtn.Visibility = Visibility.Visible;
                    DtaGridActionDuty.Visibility = Visibility.Visible;
                    cbCompany.Visibility = Visibility.Collapsed;
                    break;
                case "Manager":
                    AdDutyBtn.Visibility = Visibility.Visible;
                    DtaGridActionDuty.Visibility = Visibility.Visible;
                    cbCompany.Visibility = Visibility.Collapsed;
                    break;
                case "Employee":
                    AdDutyBtn.Visibility = Visibility.Collapsed;
                    DtaGridActionDuty.Visibility = Visibility.Collapsed;
                    lblTitle.Text = "Công việc";
                    cbCompany.Visibility = Visibility.Collapsed;
                    //btnDeleteDetail.Visibility = Visibility.Collapsed;
                    break;
                case "SystemAdmin":
                    DtaGridActionDuty.Visibility = Visibility.Collapsed;
                    AdDutyBtn.Visibility = Visibility.Collapsed;
                    _ = LoadCompanies();
                    break;
            }
        }

        /// Load duty
        private void LoadDuty()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();

                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty";

                int pageSize = 20;
                _paginationHelper = new PaginationHelper<DutyResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => DutyDtaGrid.ItemsSource = items,
                    txtPage
                );
                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Search
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
            var yearList = Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse().ToList();
            years.AddRange(yearList);
            cbYear.ItemsSource = years;

            cbDay.SelectedIndex = DateTime.Now.Day;         
            cbMonth.SelectedIndex = DateTime.Now.Month;    
            cbYear.SelectedIndex = years.IndexOf(currentYear.ToString()); 
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

            var items = await SearchAndFilterDutiesAsync(baseUrl, token, keyword, day, month, year, companyId);
            DutyDtaGrid.ItemsSource = items;
        }
        public static async Task<List<DutyResultDto>> SearchAndFilterDutiesAsync(string baseUrl, string token, string searchKeyword, int? day, int? month, int? year, Guid? companyId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (companyId.HasValue) parameters.Add($"companyId={companyId}");
                if (day.HasValue) parameters.Add($"Day={day}");
                if (month.HasValue) parameters.Add($"Month={month}");
                if (year.HasValue) parameters.Add($"Year={year}");
                
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/Duty";
                if (parameters.Any()) url += "?" + string.Join("&", parameters);

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Lỗi khi lọc công việc: {response.StatusCode}\n{errorContent}");
                    return new List<DutyResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<DutyResultDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Items?.ToList() ?? new List<DutyResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<DutyResultDto>();
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
        private void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e) => _debounceTimerRestart();
        private void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => _debounceTimerRestart();
        private void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => _debounceTimerRestart();
        private void txtTextChanged(object sender, TextChangedEventArgs e) => _debounceTimerRestart();
        private void cbCompany_SelectionChanged(object sender, SelectionChangedEventArgs e) => _debounceTimerRestart();

        private void _debounceTimerRestart()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }


        /// Pagination
        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }
        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();

        }


        /// ///////////////////////////////////////
        /// /////////////////////////////////////// Duty 

        private void OnDutyCreated(Duties.DutyResultDto newDept)
        {
            if (newDept != null)
            {
                var list = DutyDtaGrid.ItemsSource as List<Duties.DutyResultDto> ?? new List<Duties.DutyResultDto>();
                list.Insert(0, newDept);
                DutyDtaGrid.ItemsSource = null;
                DutyDtaGrid.ItemsSource = list;

                DutyDtaGrid.SelectedItem = newDept;
                DutyDtaGrid.ScrollIntoView(newDept);
            }
        }

        private void OnDutyUpdated(Duties.DutyResultDto updatedDept)
        {
            if (updatedDept != null)
            {
                var list = DutyDtaGrid.ItemsSource as List<Duties.DutyResultDto> ?? new List<Duties.DutyResultDto>();

                var existing = list.FirstOrDefault(d => d.Id == updatedDept.Id);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedDept);

                DutyDtaGrid.ItemsSource = null;
                DutyDtaGrid.ItemsSource = list;

                DutyDtaGrid.SelectedItem = updatedDept;
                DutyDtaGrid.ScrollIntoView(updatedDept);
            }
        }

        /// Create
        private void AdDutyBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateDuty(OnDutyCreated);
            window.Show();
        }

        /// Update
        private void btnDutyUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (DutyDtaGrid.SelectedItem is DutyResultDto selectedDuty)
            {
                var window = new UpdateDuty(selectedDuty.Id, OnDutyUpdated);
                window.Show();
            }
        }

        /// Delete
        private void btnDutyDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagValue = button?.Tag?.ToString();

            if (!string.IsNullOrWhiteSpace(tagValue) && Guid.TryParse(tagValue, out Guid dutyId))
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa công việc này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteDutyAsync(dutyId);
                }
            }
            else
            {
                MessageBox.Show("Không lấy được ID công việc cần xóa. Tag: " + tagValue);
            }
        }
        private async Task DeleteDutyAsync(Guid dutyId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty/dutyId";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(dutyId.ToString()), "dutyId");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseUrl),
                Content = formData
            };

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa công việc thành công.");
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể xóa công việc: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// ///////////////////////////////////////
        /// /////////////////////////////////////// Duty Detail 

        /// Create Detail
        private void btnAddDetail_Click(object sender, RoutedEventArgs e)
        {
            if (DutyDtaGrid.SelectedItem is DutyResultDto selectedDuty)
            {
               // var window = new CreateDetail(OnDetailCreated, selectedDuty.Id);
                var window = new CreateDetail(LoadDuty, selectedDuty.Id);
                window.Show();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một chức vụ để thêm chi tiết.");
            }
        }

        /// Update Detail
        private void btnUpdateDetail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagValue = button?.Tag?.ToString();
            //MessageBox.Show("Tag value: " + tagValue);

            if (!string.IsNullOrWhiteSpace(tagValue) && Guid.TryParse(tagValue, out Guid detailId))
            {
                //var window = new UpdateDetail(detailId, OnDetailUpdated);
                var window = new UpdateDetail(detailId, LoadDuty);
                window.Show();
            }
            else
            {
                MessageBox.Show("Không lấy được ID chi tiết công việc cần sửa.");
            }
        }

        /// Delete Detail
        private void btnDeleteDetail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagValue = button?.Tag?.ToString();

            if (!string.IsNullOrWhiteSpace(tagValue) && Guid.TryParse(tagValue, out Guid dutyId))
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa chi tiết công việc này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteDutyDetailAsync(dutyId);
                }
            }
            else
            {
                MessageBox.Show("Không lấy được ID chi tiết công việc cần xóa. Tag: " + tagValue);
            }
        }
        private async Task DeleteDutyDetailAsync(Guid detailId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty/detailId";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(detailId.ToString()), "detailId");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseUrl),
                Content = formData
            };
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa chi tiết công việc thành công.");
                //LoadDuty();
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(errorJson);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                //var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Không thể xóa chi tiết công việc: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// Done Task
        private void btnDoneTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagValue = button?.Tag?.ToString();

            if (!string.IsNullOrWhiteSpace(tagValue) && Guid.TryParse(tagValue, out Guid dutyId))
            {
                var result = MessageBox.Show("Bạn có chắc chắn là đã hoàn thành công việc này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = MarkAsCompletedAsync(dutyId);
                }
            }
            else
            {
                MessageBox.Show("Không lấy được ID chi tiết công việc. Tag: " + tagValue);
            }
        }
        private async Task MarkAsCompletedAsync(Guid detailId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty/MarkCompleted/" + detailId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(detailId.ToString()), "detailId");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseUrl),
                Content = formData
            };
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Đánh dấu công việc là đã hoàn thành thành công.");
                //LoadDuty();
                await FilterAsync();
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(errorJson);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                //MessageBox.Show($"Có lỗi xảy ra: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show($"Không thể đánh dấu công việc là đã hoàn thành: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                //MessageBox.Show($"Không thể đánh dấu công việc là đã hoàn thành.\nStatusCode: {response.StatusCode}\nUrl: {baseUrl}\nChi tiết: {errorContent}");
            }
        }
    }
}
