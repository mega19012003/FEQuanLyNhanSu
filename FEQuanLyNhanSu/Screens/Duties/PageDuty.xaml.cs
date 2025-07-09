using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Duties;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
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
        public PageDuty()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadDuty();
            LoadDateComboboxes();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "Administrator":
                    AdDutyBtn.Visibility = Visibility.Visible;
                    DtaGridActionDuty.Visibility = Visibility.Visible;
                    break;
                case "Manager":
                    AdDutyBtn.Visibility = Visibility.Visible;
                    DtaGridActionDuty.Visibility = Visibility.Visible;
                    break;
                case "Employee":
                    AdDutyBtn.Visibility = Visibility.Collapsed;
                    DtaGridActionDuty.Visibility = Visibility.Collapsed;
                    //btnDeleteDetail.Visibility = Visibility.Collapsed;
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
                MessageBox.Show($"Lỗi khi tải dữ liệu chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        DutyDtaGrid.ItemsSource = items;
        }

        public static async Task<List<DutyResultDto>> SearchAndFilterCheckinsAsync(
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

                var url = baseUrl + "/api/Duty";
                if (parameters.Any()) url += "?" + string.Join("&", parameters);

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API Error: {response.StatusCode}");
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


        private async void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();
        //private async void txtTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var token = Application.Current.Properties["Token"].ToString();
        //    string keyword = txtSearch.Text?.Trim();

        //    if (string.IsNullOrWhiteSpace(keyword))
        //        LoadDuty();
        //    else
        //    {
        //        var result = await SearchHelper.SearchAsync<DutyResultDto>("api/Duty", keyword, token);
        //        DutyDtaGrid.ItemsSource = result;
        //    }
        //}

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
                LoadDuty();
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

        //private void OnDetailCreated(Duties.DutyDetailResultDto newDept)
        //{
        //    if (newDept != null)
        //    {
        //        var list = DutyDtaGrid.ItemsSource as List<Duties.DutyDetailResultDto> ?? new List<Duties.DutyDetailResultDto>();
        //        list.Insert(0, newDept);
        //        DutyDtaGrid.ItemsSource = null;
        //        DutyDtaGrid.ItemsSource = list;

        //        DutyDtaGrid.SelectedItem = newDept;
        //        DutyDtaGrid.ScrollIntoView(newDept);
        //    }
        //}

        //private void OnDetailUpdated(Duties.DutyDetailResultDto updatedDept)
        //{
        //    if (updatedDept != null)
        //    {
        //        var list = DutyDtaGrid.ItemsSource as List<Duties.DutyDetailResultDto> ?? new List<Duties.DutyDetailResultDto>();

        //        var existing = list.FirstOrDefault(d => d.DutyDetailId == updatedDept.DutyDetailId);
        //        if (existing != null)
        //        {
        //            list.Remove(existing);
        //        }

        //        list.Insert(0, updatedDept);

        //        DutyDtaGrid.ItemsSource = null;
        //        DutyDtaGrid.ItemsSource = list;

        //        DutyDtaGrid.SelectedItem = updatedDept;
        //        DutyDtaGrid.ScrollIntoView(updatedDept);
        //    }
        //}

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
                LoadDuty();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Không thể xóa chi tiết công việc.\nStatusCode: {response.StatusCode}\nUrl: {baseUrl}\nChi tiết: {errorContent}");
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
                LoadDuty();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Không thể đánh dấu công việc là đã hoàn thành.\nStatusCode: {response.StatusCode}\nUrl: {baseUrl}\nChi tiết: {errorContent}");
            }
        }
    }
}
