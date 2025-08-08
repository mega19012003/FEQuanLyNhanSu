using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Duties;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
                _debounceTimer.Stop(); 
                await FilterAsync();
            };
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadIsCompletedComboBox();
            LoadDateComboboxes();
            Loaded += async (s, e) => await FilterAsync();
        }

        private async Task HandleUI(string role)
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
                    await LoadCompanies();
                    break;
            }
        }
        /// Load duty
        private void LoadIsCompletedComboBox()
        {
            var statusMap = new Dictionary<string, string>
            {
                { "Tất cả", "" },
                { "Chờ xử lý", "Pending" },
                { "Đang xử lý", "InProgress" },
                { "Hoàn tất", "Completed" }
            };
            cbIsCompleted.ItemsSource = statusMap;
            cbIsCompleted.DisplayMemberPath = "Key";  
            cbIsCompleted.SelectedValuePath = "Value"; 
            cbIsCompleted.SelectedIndex = 0;
        }
        private async Task LoadDuty()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty";
                int pageSize = 10;
                _paginationHelper = new PaginationHelper<DutyResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => DutyDtaGrid.ItemsSource = items,
                    txtPage,
                    page => BuildDutyUrlWithFilter(page, pageSize)
				);
                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
		private string BuildDutyUrlWithFilter(int pageIndex, int pageSize)
		{
			var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty";
			var parameters = new List<string>();

			string keyword = txtSearch.Text?.Trim();
			if (!string.IsNullOrWhiteSpace(keyword))
				parameters.Add($"Search={Uri.EscapeDataString(keyword)}");

			if (cbDay.SelectedIndex > 0 && int.TryParse(cbDay.SelectedItem.ToString(), out int selectedDay))
				parameters.Add($"Day={selectedDay}");

			if (cbMonth.SelectedIndex > 0 && int.TryParse(cbMonth.SelectedItem.ToString(), out int selectedMonth))
				parameters.Add($"Month={selectedMonth}");

			if (cbYear.SelectedIndex > 0 && int.TryParse(cbYear.SelectedItem.ToString(), out int selectedYear))
				parameters.Add($"Year={selectedYear}");

			if (cbIsCompleted.SelectedItem is KeyValuePair<string, string> selectedStatus)
			{
				string statusValue = selectedStatus.Value;
				if (!string.IsNullOrWhiteSpace(statusValue))
					parameters.Add($"Status={Uri.EscapeDataString(statusValue)}");
			}

			if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
				parameters.Add($"companyId={selectedComp.CompanyId}");

			parameters.Add($"pageIndex={pageIndex}");
			parameters.Add($"pageSize={pageSize}");

			return parameters.Any() ? $"{baseUrl}?{string.Join("&", parameters)}" : baseUrl;
		}
		private async Task FilterAsync()
		{
			await LoadDuty();
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
        private void cbIsCompleted_SelectionChanged(object sender, SelectionChangedEventArgs e) => _debounceTimerRestart();
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
            window.ShowDialog();
        }
        /// Update
        private void btnDutyUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (DutyDtaGrid.SelectedItem is DutyResultDto selectedDuty)
            {
                var window = new UpdateDuty(selectedDuty.Id, OnDutyUpdated);
                window.ShowDialog();
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
                await FilterAsync();
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
		private void RefreshDutyList()
		{
			_ = LoadDuty(); 
		}
		/// Create Detail
		private void btnAddDetail_Click(object sender, RoutedEventArgs e)
        {
            if (DutyDtaGrid.SelectedItem is DutyResultDto selectedDuty)
            {
               // var window = new CreateDetail(OnDetailCreated, selectedDuty.Id);
                var window = new CreateDetail(RefreshDutyList, selectedDuty.Id);
                window.ShowDialog();
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
                var window = new UpdateDetail(detailId, RefreshDutyList);
                window.ShowDialog();
            }
            else
            {
                MessageBox.Show("Không lấy được ID chi tiết công việc cần sửa.");
            }
        }
        /// Delete Detail
        private async void btnDeleteDetail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tagValue = button?.Tag?.ToString();

            if (!string.IsNullOrWhiteSpace(tagValue) && Guid.TryParse(tagValue, out Guid dutyId))
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa chi tiết công việc này không?",
                                             "Xác nhận xóa",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    string oldText = button.Content.ToString();
                    button.Content = "Đang xóa...";
                    button.IsEnabled = false;

                    // Chờ hàm xóa chạy xong
                    await DeleteDutyDetailAsync(dutyId);

                    button.Content = oldText;
                    button.IsEnabled = true;
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
                await FilterAsync();
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
        //private void btnDoneTask_Click(object sender, RoutedEventArgs e)
        //{
        //    var button = sender as Button;
        //    var tagValue = button?.Tag?.ToString();

        //    if (!string.IsNullOrWhiteSpace(tagValue) && Guid.TryParse(tagValue, out Guid dutyId))
        //    {
        //        var result = MessageBox.Show("Bạn có chắc chắn là đã hoàn thành công việc này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        //        if (result == MessageBoxResult.Yes)
        //        {
        //            _ = MarkAsCompletedAsync(dutyId);
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Không lấy được ID chi tiết công việc. Tag: " + tagValue);
        //    }
        //}
        //private async Task MarkAsCompletedAsync(Guid detailId)
        //{
        //    var token = Application.Current.Properties["Token"]?.ToString();
        //    var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty/MarkCompleted/" + detailId;
        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //    var formData = new MultipartFormDataContent();
        //    formData.Add(new StringContent(detailId.ToString()), "detailId");
        //    var request = new HttpRequestMessage
        //    {
        //        Method = HttpMethod.Put,
        //        RequestUri = new Uri(baseUrl),
        //        Content = formData
        //    };
        //    var response = await client.SendAsync(request);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        MessageBox.Show("Đánh dấu công việc là đã hoàn thành thành công.");
        //        //LoadDuty();
        //        await FilterAsync();
        //    }
        //    else
        //    {
        //        var errorJson = await response.Content.ReadAsStringAsync();
        //        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(errorJson);
        //        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
        //        //MessageBox.Show($"Có lỗi xảy ra: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //        MessageBox.Show($"Không thể đánh dấu công việc là đã hoàn thành: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //        //MessageBox.Show($"Không thể đánh dấu công việc là đã hoàn thành.\nStatusCode: {response.StatusCode}\nUrl: {baseUrl}\nChi tiết: {errorContent}");
        //    }
        //}
        //private void DtaGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var dep = (DependencyObject)e.OriginalSource;

        //    // Nếu click vào Button (hoặc phần tử con của Button), thì bỏ qua để nút hoạt động bình thường
        //    while (dep != null)
        //    {
        //        if (dep is Button)
        //            return;
        //        dep = VisualTreeHelper.GetParent(dep);
        //    }

        //    dep = (DependencyObject)e.OriginalSource;

        //    while (dep != null && !(dep is DataGridRow))
        //    {
        //        dep = VisualTreeHelper.GetParent(dep);
        //    }

        //    if (dep is DataGridRow row)
        //    {
        //        var item = row.Item;

        //        if (DutyDtaGrid.SelectedItems.Contains(item))
        //        {
        //            DutyDtaGrid.SelectedItems.Remove(item); // Ẩn details
        //        }
        //        else
        //        {
        //            DutyDtaGrid.SelectedItems.Add(item); // Hiện details
        //        }

        //        e.Handled = true; // Ngăn chọn lại dòng
        //    }
        //}
    }
}
