using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FEQuanLyNhanSu.Base;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;

namespace FEQuanLyNhanSu.Helpers
{
    public class PaginationHelper<T>
    {
        private readonly string _baseUrl;
        private readonly int _pageSize;
        private readonly string _token;
        private readonly Action<List<T>> _updateUI;
        private readonly Label _pageLabel;

        private int _currentPage = 1;
        private int _totalPages = 1;

        public PaginationHelper(string baseUrl, int pageSize, string token, Action<List<T>> updateUI, Label pageLabel)
        {
            _baseUrl = baseUrl;
            _pageSize = pageSize;
            _token = token;
            _updateUI = updateUI;
            _pageLabel = pageLabel;
        }

        public async Task LoadPageAsync(int page)
        {
            try
            {
                if (page < 1 || page > _totalPages)
                    return;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                string url = $"{_baseUrl}?pageSize={_pageSize}&pageIndex={page}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(errorJson);
                    var errorData = apiResponse?.Data ?? errorJson ?? "Có lỗi xảy ra";
                    //MessageBox.Show($"{url}");
                    //MessageBox.Show($"Lỗi phân trang: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<T>>>(json);

                if (apiResult?.Data == null)
                {
                    MessageBox.Show("Không lấy được dữ liệu từ phản hồi API", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentPage = page;
                _totalPages = (int)Math.Ceiling((double)apiResult.Data.TotalCount / _pageSize);
                _updateUI(apiResult.Data.Items.ToList());
                _pageLabel.Content = _currentPage.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi exception: {ex.Message}\n{ex.StackTrace}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task NextPageAsync()
        {
            await LoadPageAsync(_currentPage + 1);
        }
        public async Task PrevPageAsync()
        {
            await LoadPageAsync(_currentPage - 1);
        }
        public async Task RefreshAsync()
        {
            await LoadPageAsync(_currentPage);
        }
    }
}
