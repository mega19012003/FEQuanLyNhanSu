using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.Panels;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.Checkins;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Checkins
{
    /// <summary>
    /// Interaction logic for Checkin.xaml
    /// </summary>
    public partial class Checkin : Window
    {
        private Action<CheckinResultDto> _onCheckinCreated;
        public Checkin(Action<CheckinResultDto> onCheckinCreated)
        {
            InitializeComponent();
            _onCheckinCreated = onCheckinCreated;
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
        }

        private async Task HandleUI(string userRole)
        {
            switch (userRole)
            {
                case "Administrator":
                    cbEmployee.Visibility = Visibility.Visible;
                    lblName.Visibility = Visibility.Visible;
                    await LoadDepartments();
                    await LoadPositionsByDepartmentAsync();
                    await FilterAsync();
                    break;

                case "Manager":
                    cbEmployee.Visibility = Visibility.Visible;
                    lblName.Visibility = Visibility.Visible;
                    cbDepartment.Visibility = Visibility.Collapsed;
                    await LoadPositions();    
                    await FilterAsync();
                    break;

                case "Employee":
                    cbEmployee.Visibility = Visibility.Collapsed;
                    lblName.Visibility = Visibility.Collapsed;
                    lblNote.Visibility = Visibility.Collapsed;
                    txtNote.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async Task FilterAsync()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/User/employee-manager?";

                if (cbDepartment.SelectedItem is DepartmentResultDto selectedDepartment && selectedDepartment?.DepartmentId != Guid.Empty)
                    url += $"departmentId={selectedDepartment.DepartmentId}&";

                if (cbPosition.SelectedItem is PositionResultDto selectedPosition && selectedPosition?.Id != Guid.Empty)
                    url += $"positionId={selectedPosition.Id}&";

                var keyword = cbEmployee.Text.Trim();
                if (!string.IsNullOrWhiteSpace(keyword))
                    url += $"Search={Uri.EscapeDataString(keyword)}";

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
                    cbEmployee.IsDropDownOpen = true;
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
                    cbEmployee.IsDropDownOpen = true;
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
        //    var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User/employee-manager";

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //    var response = await client.GetAsync(baseUrl);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var json = await response.Content.ReadAsStringAsync();
        //        var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);

        //        cbEmployee.ItemsSource = result.Data.Items;
        //        cbEmployee.SelectedItem = null;
        //        cbEmployee.IsDropDownOpen = true;
        //    }
        //    else
        //    {
        //        var json = await response.Content.ReadAsStringAsync();
        //        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
        //        var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
        //        MessageBox.Show("Không thể tải danh sách nhân viên: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        //private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        string keyword = cbEmployee.Text.Trim();
        //        var token = Application.Current.Properties["Token"]?.ToString();
        //        var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

        //        using var client = new HttpClient();
        //        client.DefaultRequestHeaders.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //        HttpResponseMessage response;

        //        if (string.IsNullOrEmpty(keyword))
        //        {
        //            response = await client.GetAsync(baseUrl); // không có query Search
        //            cbEmployee.IsDropDownOpen = true;
        //        }
        //        else
        //        {
        //            /*var */response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var json = await response.Content.ReadAsStringAsync();
        //                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
        //                cbEmployee.ItemsSource = result?.Data?.Items;
        //                cbEmployee.SelectedItem = null;
        //                cbEmployee.IsDropDownOpen = true;
        //            }
        //            else
        //            {
        //                cbEmployee.ItemsSource = null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi khi tìm kiếm nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            btnCreate.IsEnabled = false;
            btnExit.IsEnabled = false;

            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin/Checkin";

                var form = new MultipartFormDataContent();
                //var deviceInfo = Environment.MachineName;
                //form.Add(new StringContent(deviceInfo), "DeviceInfo");
                var note = txtNote.Text ?? "";
                form.Add(new StringContent(note), "Note");

                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                //if (selectedUser != null)
                //{
                //    form.Add(new StringContent(selectedUser.UserId.ToString()), "userId");
                //}

                var currentUserId = Application.Current.Properties["UserId"]?.ToString();

                if (selectedUser != null)
                {
                    form.Add(new StringContent(selectedUser.UserId.ToString()), "userId");
                    if (selectedUser.UserId == Guid.Parse(currentUserId))
                    {
                        var deviceInfo = Environment.MachineName;
                        form.Add(new StringContent(deviceInfo), "DeviceInfo");
                    }
                }
                else
                {
                    // Trường hợp employee, không có ComboBox
                    var deviceInfo = Environment.MachineName;
                    form.Add(new StringContent(deviceInfo), "DeviceInfo");
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(baseUrl, form);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CheckinResultDto>>(json);
                    if (apiResponse?.Data != null)
                    {
                        lblCheckinMor.Content = apiResponse.Data.CheckinTime;
                        lblCheckoutMor.Content = apiResponse.Data.CheckoutTime;
                        lblCheckinAft.Content = apiResponse.Data.LogStatus;
                        lblNoteResult.Content = apiResponse.Data.Note;

                        _onCheckinCreated?.Invoke(apiResponse.Data);
                        MessageBox.Show("Checkin thành công.");
                    }
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorMessage = errorResponse?.Data ?? errorResponse?.Message ?? "Có lỗi xảy ra.";
                    MessageBox.Show($"Checkin thất bại: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCreate.IsEnabled = true;
                btnExit.IsEnabled = true;
            }
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }
    }
}
