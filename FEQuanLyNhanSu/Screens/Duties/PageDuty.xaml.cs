using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Screens.Duties;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static FEQuanLyNhanSu.ResponseModels.Duties;
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
            LoadDuty();
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
        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
                LoadDuty();
            else
            {
                var result = await SearchHelper.SearchAsync<DutyResultDto>("api/Duty", keyword, token);
                DutyDtaGrid.ItemsSource = result;
            }
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
        /// Create
        private void AdDutyBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateDuty(LoadDuty);
            window.Show();
        }

        /// Update
        private void btnDutyUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (DutyDtaGrid.SelectedItem is DutyResultDto selectedDuty)
            {
                var window = new UpdateDuty(selectedDuty.Id, LoadDuty);
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
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Không thể xóa công việc.\nStatusCode: {response.StatusCode}\nUrl: {baseUrl}\nChi tiết: {errorContent}");
            }
        }


        /// ///////////////////////////////////////
        /// /////////////////////////////////////// Duty Detail 

        /// Create Detail
        private void btnAddDetail_Click(object sender, RoutedEventArgs e)
        {
            if (DutyDtaGrid.SelectedItem is DutyResultDto selectedDuty)
            {
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
