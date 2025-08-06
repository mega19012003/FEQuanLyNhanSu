using EmployeeAPI.Enums;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Duties
{
    /// <summary>
    /// Interaction logic for UpdateDuty.xaml
    /// </summary>
    public partial class UpdateDetail : Window
    {
        private Guid _detailId;
        private Action _onDetailUpdated;
        //private Action<DutyDetailResultDto> _onDetailUpdated;
        public UpdateDetail(Guid detailId, Action onDetailUpdated)
        {
            InitializeComponent();
            //_ = LoadUsers();
            _detailId = detailId;
            _onDetailUpdated = onDetailUpdated;
            _ = LoadDetailAsync();
            LoadDutyStatus();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
        }

        private async Task HandleUI(string userRole)
        {
            switch (userRole)
            {
                case "Employee":
                    cbEmployee.Visibility = Visibility.Collapsed;
                    txtEmployee.Visibility = Visibility.Collapsed;
                    txtDescription.IsEnabled = false;
                    txtTitle.IsEnabled = false;
                    dpEndDate.IsEnabled = false;
                    break;

                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    await LoadPositions();
                    await FilterAsync();
                    break;

                case "Administrator":
                    await LoadDepartments();
                    await LoadPositionsByDepartmentAsync();
                    await FilterAsync();
                    break;
            }
        }

        private async Task FilterAsync()
        {
            try
            {
                //string oldText = cbEmployee.Text;
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var queryParams = new List<string>();

                if (cbDepartment.SelectedItem is DepartmentResultDto selectedDepartment && selectedDepartment?.DepartmentId != Guid.Empty)
                    queryParams.Add($"departmentId={selectedDepartment.DepartmentId}");

                if (cbPosition.SelectedItem is PositionResultDto selectedPosition && selectedPosition?.Id != Guid.Empty)
                    queryParams.Add($"positionId={selectedPosition.Id}");

                var keyword = cbEmployee.Text.Trim();
                if (!string.IsNullOrWhiteSpace(keyword))
                    queryParams.Add($"Search={Uri.EscapeDataString(keyword)}");

                var queryString = string.Join("&", queryParams);
                var url = $"{baseUrl}/api/User/employee-manager?employeeOnly=true";
                if (queryParams.Any())
                    url += "&" + queryString;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
                    cbEmployee.ItemsSource = result.Data.Items;
                    cbEmployee.SelectedValuePath = "UserId";
                    cbEmployee.IsDropDownOpen = false;

                    //cbEmployee.Text = oldText;
                }
                else
                {
                    cbEmployee.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi Filter: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            string keyword = cbEmployee.Text.Trim();

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            //var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
            //HttpResponseMessage response;

            if (string.IsNullOrEmpty(keyword))
            {
                await FilterAsync();
                cbEmployee.IsDropDownOpen = false;
            }
            else
            {
                var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
                    cbEmployee.ItemsSource = result.Data.Items;
                    cbEmployee.IsDropDownOpen = false;
                }
                else
                {
                    cbEmployee.ItemsSource = null;
                }
            }
        }
        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadPositionsByDepartmentAsync();
            await FilterAsync();
        }
        private async void cbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await FilterAsync();
        }
        private async void cbDepartment_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbDepartment.Text.Trim();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadDepartments();
                    cbDepartment.SelectedItem = null;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                        cbDepartment.ItemsSource = result?.Data?.Items;
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
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void cbPosition_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var keyword = cbPosition.Text.Trim();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                Guid? departmentId = null;
                if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                    departmentId = selectedDept.DepartmentId;

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (string.IsNullOrEmpty(keyword))
                {
                    if (!departmentId.HasValue)
                    {
                        cbPosition.ItemsSource = null;
                        cbPosition.SelectedItem = null;
                        cbPosition.IsDropDownOpen = false;
                        return;
                    }

                    string url = $"{baseUrl}?departmentId={departmentId.Value}";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                        cbPosition.ItemsSource = result?.Data?.Items;
                        cbPosition.SelectedItem = null;
                        cbPosition.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbPosition.ItemsSource = null;
                    }
                }
                else
                {
                    string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";
                    if (departmentId.HasValue)
                        url += $"&departmentId={departmentId.Value}";

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                        cbPosition.ItemsSource = result?.Data?.Items;
                        cbPosition.SelectedItem = null;
                        cbPosition.IsDropDownOpen = true;
                    }
                    else
                    {
                        cbPosition.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadDepartments()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;

                if (result.Data.Items != null && result.Data.Items.Any())
                {
                    cbDepartment.SelectedItem = result.Data.Items.First();
                    await FilterAsync();
                }
            }
        }
        private async Task LoadPositions()
        {
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                    cbPosition.ItemsSource = result.Data.Items;
                    if (result.Data.Items != null && result.Data.Items.Any())
                    {
                        cbPosition.SelectedItem = result.Data.Items.First();
                        await FilterAsync();
                    }
                }
            }
        }
        private async Task LoadPositionsByDepartmentAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";

            Guid? departmentId = null;
            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                departmentId = selectedDept.DepartmentId;

            string url = baseUrl;
            if (departmentId.HasValue)
            {
                url += $"?departmentId={departmentId.Value}";
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<PositionResultDto>>>(json);
                cbPosition.ItemsSource = result?.Data?.Items;
                if (result?.Data?.Items?.Any() == true)
                    cbPosition.SelectedItem = result.Data.Items.First();
            }
            else
            {
                cbPosition.ItemsSource = null;
            }
        }


        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        //private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        //{
        //    string keyword = cbEmployee.Text.Trim();
        //    if (string.IsNullOrEmpty(keyword))
        //    {
        //        cbEmployee.ItemsSource = null;
        //        return;
        //    }

        //    var token = Application.Current.Properties["Token"]?.ToString();
        //    var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //    var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var json = await response.Content.ReadAsStringAsync();
        //        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
        //        cbEmployee.ItemsSource = result.Data.Items;
        //        cbEmployee.IsDropDownOpen = true;
        //    }
        //    else
        //    {
        //        cbEmployee.ItemsSource = null;
        //    }
        //}

        private async Task LoadUsers()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager?employeeOnly=true";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);

                cbEmployee.ItemsSource = result.Data.Items;
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadDutyStatus()
        {
            cb.ItemsSource = Enum.GetValues(typeof(DutyStatus)).Cast<DutyStatus>();
        }
        private async Task LoadDetailAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/Duty/detail/{_detailId}";
            //MessageBox.Show("Url: " + url);
            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<DutyDetailResultDto>>(json);

                //cbEmployee.Text = result.Data.Name;
                txtDescription.Text = result.Data.Description;
                txtTitle.Text = result.Data.Title;
                dpEndDate.SelectedDate = result.Data.Deadline.ToDateTime(TimeOnly.MinValue); ;
                if (Enum.TryParse<DutyStatus>(result.Data.Status, out var dutyStatus))
                {
                    cb.SelectedItem = dutyStatus;
                }

                var role = Application.Current.Properties["UserRole"]?.ToString();
                if (role != "Employee")
                {
                    await FilterAsync();
                }

                var userList = cbEmployee.ItemsSource as List<UserResultDto>;
                var selectedUser = userList?.FirstOrDefault(u => u.UserId == result.Data.UserId);
                if (selectedUser != null)
                {
                    cbEmployee.SelectedItem = selectedUser;
                }
                else
                {
                    cbEmployee.Text = result.Data.Name; 
                }
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải chi tiết thông tin chức vụ. Lỗi: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Update
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                var role = Application.Current.Properties["UserRole"]?.ToString();
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty/DutyDetail";

                object dutyDetail;

                if (role == "Employee")
                {
                    dutyDetail = new
                    {
                        DutyDetailId = _detailId,
                        Status = (int)(DutyStatus)cb.SelectedItem,
                        Note = txtNote.Text.Trim()
                    };
                }
                else 
                {
                    if (cbEmployee.SelectedItem == null)
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên.");
                        return;
                    }
                    var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                    if (selectedUser == null)
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên hợp lệ.");
                        return;
                    }

                    dutyDetail = new
                    {
                        DutyDetailId = _detailId,
                        UserId = selectedUser.UserId,
                        Description = txtDescription.Text.Trim(),
                        Title = txtTitle.Text.Trim(),
                        Note = txtNote.Text.Trim(),
                        Deadline = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value),
                        Status = (int)(DutyStatus)cb.SelectedItem,
                    };
                }

                using var client = CreateAuthorizedClient(token);
                var content = new StringContent(JsonConvert.SerializeObject(dutyDetail), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thông tin thành công.");
                    _onDetailUpdated?.Invoke();
                    this.Close();
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    //dynamic? apiResponse = JsonConvert.DeserializeObject<dynamic>(json);
                    //string errorMessage = apiResponse?.Data != null ? apiResponse.Data.ToString() : apiResponse?.Message?.ToString() ?? "Có lỗi xảy ra";
                    //MessageBox.Show($"Cập nhật thông tin thất bại. Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    dynamic? apiResponse = JsonConvert.DeserializeObject<dynamic>(json);

                    // Ưu tiên theo thứ tự: Data > Message > Errors > Title > Detail
                    string? errorMessage = null;

                    if (apiResponse?.Data != null)
                        errorMessage = apiResponse.Data.ToString();
                    else if (apiResponse?.Message != null)
                        errorMessage = apiResponse.Message.ToString();
                    else if (apiResponse?.errors != null)
                        errorMessage = string.Join("; ", ((IDictionary<string, JToken>)apiResponse.errors).Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));
                    else if (apiResponse?.title != null)
                        errorMessage = apiResponse.title.ToString();
                    else if (apiResponse?.detail != null)
                        errorMessage = apiResponse.detail.ToString();
                    else
                        errorMessage = json; // fallback, show full raw content nếu không parse được

                    MessageBox.Show($"Cập nhật thông tin thất bại. Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
            }
        }
        // Exit
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }
    }
}
