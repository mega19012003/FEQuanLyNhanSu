using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
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
    /// Interaction logic for CreateDuty.xaml
    /// </summary>
    public partial class CreateDuty : Window
    {
        private Action<DutyResultDto> _onDutyCreated;
        public CreateDuty(Action<DutyResultDto> onDutyCreated)
        {
            InitializeComponent();
            //_ = LoadUsers();
            _onDutyCreated = onDutyCreated;
            Loaded += CreateDuty_Loaded;
        }
        private async void CreateDuty_Loaded(object sender, RoutedEventArgs e)
        {
            await HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            await FilterAsync();
        }
        private async Task HandleUI(string role)
        {
            switch (role)
            {
                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    _ = LoadPositions();
                    break;
                case "Administrator":
                    _ = LoadDepartments();
                    _ = LoadPositionsByDepartmentAsync();
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

        //private async Task LoadUsers()
        //{
        //    var token = Application.Current.Properties["Token"]?.ToString();
        //    var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager?employeeOnly=true";

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //    var response = await client.GetAsync(baseUrl);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var json = await response.Content.ReadAsStringAsync();
        //        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);

        //        cbEmployee.ItemsSource = result.Data.Items;
        //    }
        //    else
        //    {
        //        var json = await response.Content.ReadAsStringAsync();
        //        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
        //        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
        //        MessageBox.Show($"Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        //private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        //{
        //    string keyword = cbEmployee.Text.Trim();
        //    var token = Application.Current.Properties["Token"]?.ToString();
        //    var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager";

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


        //    HttpResponseMessage response;

        //    if (string.IsNullOrEmpty(keyword))
        //    {
        //        response = await client.GetAsync(baseUrl); // không có query Search
        //        cbEmployee.IsDropDownOpen = true;
        //    }
        //    else
        //    {
        //       /* var*/ 
        //        response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var json = await response.Content.ReadAsStringAsync();
        //            var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
        //            cbEmployee.ItemsSource = result?.Data?.Items;
        //            cbEmployee.SelectedItem = null;
        //            cbEmployee.IsDropDownOpen = true;
        //        }
        //        else
        //        {
        //            cbEmployee.ItemsSource = null;
        //        }
        //    }
        //}
        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(txtDescription.Text) || dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null || dpDeadline.SelectedDate == null || string.IsNullOrEmpty(txtDetailTitle.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                    return;
                }
                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                if (selectedUser == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên thực hiện");
                    return;
                }

                var startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                var endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
                var deadLine = DateOnly.FromDateTime(dpDeadline.SelectedDate.Value);

                var duty = new
                {
                    name = txtUsername.Text.Trim(),
                    startDate = startDate,
                    endDate = endDate,
                    dutyDetails = new[]
                    {
                        new {
                            userId = selectedUser.UserId,
                            title = txtDetailTitle.Text?.Trim(),
                            description = txtDescription.Text?.Trim(),
                            deadLine = deadLine,
                        }
                    }
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();

                var json = JsonConvert.SerializeObject(duty);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync($"{baseUrl}/api/Duty", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DutyResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse?.Data != null)
                    {
                        _onDutyCreated?.Invoke(apiResponse.Data);
                    }

                    MessageBox.Show("Tạo công việc thành công!");
                    this.Close();
                }
                else
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    dynamic? apiResponse = JsonConvert.DeserializeObject<dynamic>(jsonResult);
                    string errorMessage = apiResponse?.Data != null ? apiResponse.Data.ToString() : apiResponse?.Message != null ? apiResponse.Message.ToString() : "Có lỗi xảy ra";
                    MessageBox.Show($"Tạo công việc thất bại.  Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
            }
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            this.Close();
        }
    }
}
