using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Services;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption3.xaml
    /// </summary>
    public partial class PagePosition : Page
    {
        private PaginationHelper<PositionResultDto> _paginationHelper;

        public PagePosition()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadPosition();
        }

        private void HandleUI(string Role)
        {
            switch(Role)
            {
                case "Admin":
                    _ = LoadDepartments();
                    break;
                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    break;
            }
        } 
        private void LoadPosition()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<PositionResultDto>(
                baseUrl,
                pageSize,
                token,
                items => PositionDtaGrid.ItemsSource = items,
                txtPage
            );

            _ = _paginationHelper.LoadPageAsync(1);
        }

        private async Task FilterAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            string keyword = txtSearch.Text?.Trim();

            Guid? departmentId = null;
            string departmentText = cbDepartment.Text?.Trim();

            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
            {
                departmentId = selectedDept.DepartmentId;
            }
            else if (!string.IsNullOrEmpty(departmentText))
            {
                var found = (cbDepartment.ItemsSource as IEnumerable<DepartmentResultDto>)
                    ?.FirstOrDefault(d => d.Name.Equals(departmentText, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    departmentId = found.DepartmentId;
                }
            }

            // MessageBox.Show($"keyword={keyword}\ndepartmentId={departmentId}");

            var items = await SearchAndFilterPositionsAsync(
                baseUrl,
                token,
                keyword,
                departmentId
            );

            PositionDtaGrid.ItemsSource = null;
            PositionDtaGrid.ItemsSource = items;
        }

        public static async Task<List<PositionResultDto>> SearchAndFilterPositionsAsync(
            string baseUrl,
            string token,
            string searchKeyword,
            Guid? departmentId,
            int pageIndex = 1,
            int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (departmentId.HasValue)
                    parameters.Add($"departmentId={departmentId}");
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/Position";
                if (parameters.Any())
                    url += "?" + string.Join("&", parameters);

                using var client = new HttpClient();
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API Error: {response.StatusCode}");
                    return new List<PositionResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<PositionResultDto>>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Data?.Items?.ToList() ?? new List<PositionResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<PositionResultDto>();
            }
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();
        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();

        private async Task LoadDepartments()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;
            }
        }
        private async void cbDepartment_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                //var comboBox = sender as ComboBox;
                //if (comboBox?.Text == null)
                //{
                //    //cbDepartment.ItemsSource = null;
                //    return;
                //}

                //var keyword = comboBox.Text.Trim();
                //if (keyword == "")
                //{
                //    //cbDepartment.ItemsSource = null;
                //    return;
                //}
                var keyword = cbDepartment.Text.Trim();
                //if (keyword == "")
                //{
                //    cbDepartment.ItemsSource = null;
                //    keyword = cbDepartment.Text.Trim();
                //    return;
                //}
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                //var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                //if (response.IsSuccessStatusCode)
                //{
                //    var json = await response.Content.ReadAsStringAsync();
                //    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                //    cbDepartment.ItemsSource = result.Data.Items;
                //    cbDepartment.IsDropDownOpen = true;
                //}
                //else
                //{
                //    cbDepartment.ItemsSource = null;
                //}
                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadDepartments();            
                    cbDepartment.SelectedItem = null;      
                    cbDepartment.IsDropDownOpen = true;     

                    LoadPosition(); 
                }
                else
                {
                    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                        cbDepartment.ItemsSource = result.Data.Items;

                        cbDepartment.SelectedItem = null;
                        cbDepartment.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbDepartment.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPositionCreated(Positions.PositionResultDto newDept)
        {
            if (newDept != null)
            {
                var list = PositionDtaGrid.ItemsSource as List<Positions.PositionResultDto> ?? new List<Positions.PositionResultDto>();
                list.Insert(0, newDept);
                PositionDtaGrid.ItemsSource = null;
                PositionDtaGrid.ItemsSource = list;

                PositionDtaGrid.SelectedItem = newDept;
                PositionDtaGrid.ScrollIntoView(newDept);
            }
        }

        private void OnPositionUpdated(Positions.PositionResultDto updatedDept)
        {
            if (updatedDept != null)
            {
                var list = PositionDtaGrid.ItemsSource as List<Positions.PositionResultDto> ?? new List<Positions.PositionResultDto>();

                var existing = list.FirstOrDefault(d => d.Id == updatedDept.Id);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedDept);

                PositionDtaGrid.ItemsSource = null;
                PositionDtaGrid.ItemsSource = list;

                PositionDtaGrid.SelectedItem = updatedDept;
                PositionDtaGrid.ScrollIntoView(updatedDept);
            }
        }

        private void AddPosition(object sender, RoutedEventArgs e)
        {
            var window = new CreatePosition(OnPositionCreated);
            window.Show();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var editWindow = new UpdatePosition(positionId, OnPositionUpdated);
                editWindow.ShowDialog();
            }
        }

        //private async void txtTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var token = Application.Current.Properties["Token"].ToString();
        //    string keyword = txtSearch.Text?.Trim();

        //    if (string.IsNullOrWhiteSpace(keyword))
        //        LoadPosition();
        //    else
        //    {
        //        var result = await SearchHelper.SearchAsync<PositionResultDto>("api/Position", keyword, token);
        //        PositionDtaGrid.ItemsSource = result;
        //    }
        //}

        //public void LoadPosition()
        //{
        //    try
        //    {
        //        var token = Application.Current.Properties["Token"]?.ToString();
        //        var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";
        //        int pageSize = 20;

        //        _paginationHelper = new PaginationHelper<Positions.PositionResultDto>(
        //            baseUrl,
        //            pageSize,
        //            token,
        //            items => PositionDtaGrid.ItemsSource = items,
        //            txtPage
        //        );

        //        _ = _paginationHelper.LoadPageAsync(1);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi khi tải dữ liệu chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa chức vụ này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeletePositionAsync(positionId);
                }
            }
        }
        private async Task DeletePositionAsync(Guid positionId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position/" + positionId;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.DeleteAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Xóa chức vụ thành công.");
                LoadPosition(); 
            }
            else
            {
                MessageBox.Show("Không thể xóa chức vụ. Vui lòng thử lại sau.");
            }
        }

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
