using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Screens.Users;
using FEQuanLyNhanSu.Services.UserService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class PageUser : Page
    {
        private ObservableCollection<UserResultDto> users = new();
        private PaginationHelper<UserResultDto> _paginationHelper;

        public PageUser()
        {
            InitializeComponent();
            LoadUser();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            
            _ = LoadPosition();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    break;
                case "Admin":
                    _ = LoadDepartments();
                    break;
            }
        }

        private async Task FilterAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            string keyword = txtSearch.Text?.Trim();

            Guid? departmentId = null;
            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
            {
                departmentId = selectedDept.DepartmentId;
            }

            Guid? positionId = null;
            if (cbPosition.SelectedItem is PositionResultDto selectedPos)
            {
                positionId = selectedPos.Id;
            }

            // Gọi API filter
            var items = await SearchAndFilterUsersAsync(
                baseUrl,
                token,
                keyword,
                departmentId,
                positionId
            );

            UserDtaGrid.ItemsSource = null; // clear cũ
            UserDtaGrid.ItemsSource = items;
        }
        public static async Task<List<UserResultDto>> SearchAndFilterUsersAsync(
    string baseUrl,
    string token,
    string searchKeyword,
    Guid? departmentId,
    Guid? positionId,
    int pageIndex = 1,
    int pageSize = 20)
        {
            try
            {
                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                    parameters.Add($"Search={Uri.EscapeDataString(searchKeyword.Trim())}");
                if (departmentId.HasValue)
                    parameters.Add($"departmentId={departmentId.Value}");
                if (positionId.HasValue)
                    parameters.Add($"positionId={positionId.Value}");
                parameters.Add($"pageIndex={pageIndex}");
                parameters.Add($"pageSize={pageSize}");

                var url = baseUrl + "/api/User"; 
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
                    return new List<UserResultDto>();
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<PagedResult<UserResultDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Items?.ToList() ?? new List<UserResultDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<UserResultDto>();
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();
        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadPositionsByDepartmentAsync();
            await FilterAsync();
        }
        private async Task LoadPositionsByDepartmentAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            Guid? departmentId = null;

            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                departmentId = selectedDept.DepartmentId;

            if (departmentId.HasValue)
            {
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl}?departmentId={departmentId.Value}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                    cbPosition.ItemsSource = result?.Data?.Items;
                }
                else
                {
                    cbPosition.ItemsSource = null;
                }
            }
            else
            {
                cbPosition.ItemsSource = null;
            }
        }

        private async void cbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();

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
                var comboBox = sender as ComboBox;
                if (comboBox?.Text == null) return;

                var keyword = comboBox.Text.Trim();
                if (keyword == "") return; 

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                    cbDepartment.ItemsSource = result.Data.Items;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    cbDepartment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPosition()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Possition";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                cbPosition.ItemsSource = result.Data.Items;
            }
        }
        private async void cbPosition_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox?.Text == null) return; 

                var keyword = comboBox.Text.Trim();
                if (keyword == "") return; 

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                    cbPosition.ItemsSource = result.Data.Items;
                    cbPosition.IsDropDownOpen = true;
                }
                else
                {
                    cbDepartment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnUserCreated(Users.UserResultDto newDept)
        {
            if (newDept != null)
            {
                var list = UserDtaGrid.ItemsSource as List<Users.UserResultDto> ?? new List<Users.UserResultDto>();
                list.Insert(0, newDept);
                UserDtaGrid.ItemsSource = null;
                UserDtaGrid.ItemsSource = list;

                UserDtaGrid.SelectedItem = newDept;
                UserDtaGrid.ScrollIntoView(newDept);
            }
        }

        private void OnUserUpdated(Users.UserResultDto updatedUser)
        {
            if (updatedUser != null)
            {
                var list = UserDtaGrid.ItemsSource as List<Users.UserResultDto> ?? new List<Users.UserResultDto>();

                var existing = list.FirstOrDefault(d => d.UserId == updatedUser.UserId);
                if (existing != null)
                {
                    list.Remove(existing);
                }

                list.Insert(0, updatedUser);

                UserDtaGrid.ItemsSource = null;
                UserDtaGrid.ItemsSource = list;

                UserDtaGrid.SelectedItem = updatedUser;
                UserDtaGrid.ScrollIntoView(updatedUser);
            }
        }

        /// Create
        private void CreateUser(object sender, RoutedEventArgs e)
        {
            var window = new CreateUser(OnUserCreated);
            window.Show();
        }

        /// Update
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid userId)
            {
                var editWindow = new UpdateUser(userId, OnUserUpdated);
                editWindow.ShowDialog();
            }
        }

        private void LoadUser()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";
                int pageSize = 20;

                _paginationHelper = new PaginationHelper<UserResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => UserDtaGrid.ItemsSource = items,
                    txtPage
                );

                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu người dùng: {ex.Message}");
            }
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
                LoadUser();
            else
            {
                var result = await SearchHelper.SearchAsync<UserResultDto>("api/User", keyword, token);
                UserDtaGrid.ItemsSource = result;
            }
        }



        /// Delete 
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Guid positionId)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa chức vụ này không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _ = DeleteUserAsync(positionId);
                }
            }
        }
        private async Task DeleteUserAsync(Guid userId)
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/" + userId;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.DeleteAsync("");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Xóa người dùng thành công.");
                        LoadUser();
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi xóa người dùng: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa người dùng: {ex.Message}");
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

    }
}
