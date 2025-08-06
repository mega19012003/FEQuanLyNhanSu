using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Positions;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Companies;
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

        private async Task HandleUI(string Role)
        {
            switch(Role)
            {
                case "Manager":
                    cbDepartment.Visibility = Visibility.Collapsed;
                    cbCompany.Visibility = Visibility.Collapsed;
                    //_ = LoadDepartments();
                    break;
                case "Administrator":
                    await LoadDepartments();
                    cbCompany.Visibility = Visibility.Collapsed;
                    break;
                case "SystemAdmin":
                    await LoadCompanies();
                    //_ = LoadDepartments();
                    await LoadDepartmentByCompanyAsync();
                    DtaGridActionPosition.Visibility = Visibility.Collapsed;
                    AddPositionBtn.Visibility = Visibility.Collapsed;
                    btnDeleteSelected.Visibility = Visibility.Collapsed;
                    SelectedColumnHeader.Visibility = Visibility.Collapsed;
                    break;
            }
        } 
        private void LoadPosition()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Position";
            int pageSize = 10;

            _paginationHelper = new PaginationHelper<PositionResultDto>(
                baseUrl,
                pageSize,
                token,
                items => PositionDtaGrid.ItemsSource = items,
                txtPage,
                page => BuildPositionUrlWithFilter(page, pageSize)
            );

            _ = _paginationHelper.LoadPageAsync(1);
        }
        private string BuildPositionUrlWithFilter(int pageIndex, int pageSize)
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll/Position";
            var parameters = new List<string>();

            string keyword = txtSearch.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
                parameters.Add($"Search={Uri.EscapeDataString(keyword)}");

            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                parameters.Add($"companyId={selectedComp.CompanyId}");

            if (cbDepartment.SelectedItem is DepartmentResultDto selectedDept)
                parameters.Add($"departmentId={selectedDept.DepartmentId}");

            parameters.Add($"pageIndex={pageIndex}");
            parameters.Add($"pageSize={pageSize}");

            return parameters.Any() ? $"{baseUrl}?{string.Join("&", parameters)}" : baseUrl;
        }
        private async Task FilterAsync()
        {
            await LoadCompanies();
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
                    await LoadDepartmentByCompanyAsync();
                    //await LoadPositionByCompanyAsync();
                    await FilterAsync();
                }
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
        private async Task LoadDepartmentsByCompanyId(Guid? companyId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

            if (companyId.HasValue)
                baseUrl += $"?companyId={companyId.Value}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(baseUrl);

          
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result.Data.Items;
            }
        }
        private async Task LoadDepartmentByCompanyAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Department";

            Guid? companyId = null;
            if (cbCompany.SelectedItem is CompanyResultDto selectedComp)
                companyId = selectedComp.CompanyId;

            string url = baseUrl;
            if (companyId.HasValue)
            {
                url += $"?companyId={companyId.Value}";
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DepartmentResultDto>>>(json);
                cbDepartment.ItemsSource = result?.Data?.Items;
                if (result?.Data?.Items?.Any() == true)
                    cbDepartment.SelectedItem = result.Data.Items.First();
            }
            else
            {
                cbDepartment.ItemsSource = null;
            }
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e) => await FilterAsync();
        private async void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e) => await FilterAsync();
        private async void cbCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadDepartmentByCompanyAsync();
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
                    Guid? companyId = null;

                    if (cbCompany.SelectedItem is CompanyResultDto selectedCompany)
                        companyId = selectedCompany.CompanyId;

                    await LoadDepartmentsByCompanyId(companyId);
                    cbDepartment.SelectedItem = null;
                    cbDepartment.IsDropDownOpen = true;
                }
                else
                {
                    string url = $"{baseUrl}?Search={Uri.EscapeDataString(keyword)}";

                    if (cbCompany.SelectedItem is CompanyResultDto selectedCompany)
                        url += $"&companyId={selectedCompany.CompanyId}";

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
                    await LoadCompanies();               // load lại toàn bộ
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
                        cbCompany.ItemsSource = result?.Data?.Items;
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
                MessageBox.Show($"Lỗi khi tìm kiếm phòng ban: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                //LoadPosition();
                await FilterAsync();
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể xóa chức vụ: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedPositions = new List<PositionResultDto>();
            foreach (var item in PositionDtaGrid.Items)
            {
                var row = (DataGridRow)PositionDtaGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                {
                    var checkbox = FindVisualChild<CheckBox>(row);
                    if (checkbox != null && checkbox.IsChecked == true)
                    {
                        selectedPositions.Add(item as PositionResultDto);
                    }
                }
            }

            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa {selectedPositions.Count} chức vụ?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            var token = Application.Current.Properties["Token"]?.ToString();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            foreach (var position in selectedPositions)
            {
                var url = $"{AppsettingConfigHelper.GetBaseUrl()}/api/Position/{position.Id}";
                var response = await client.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Xóa thất bại chức vụ {position.Name}: {msg}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            MessageBox.Show("Xóa thành công các chức vụ đã chọn.");
            //await _paginationHelper.RefreshAsync();
            //_ = _paginationHelper.LoadPageAsync(1);
            await FilterAsync();
        }
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
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
