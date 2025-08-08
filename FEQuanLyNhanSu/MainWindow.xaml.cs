using EmployeeAPI.Models;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Screens.Companies;
using FEQuanLyNhanSu.Screens.Dashboard;
using FEQuanLyNhanSu.Screens.Users;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static FEQuanLyNhanSu.ResponseModels.Companies;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public string CompanyImagePath { get; set; } = "assets/images.png";
        public MainWindow()
        {
            InitializeComponent();
            StartClockTimer();
            GetName(Application.Current.Properties["Fullname"]?.ToString());
            ApplyRolePermissions(Application.Current.Properties["UserRole"]?.ToString());

            var role = Application.Current.Properties["UserRole"]?.ToString();
            if (role != "SystemAdmin")
            {
                _ = LoadCompanyImageAsync();
                _ = InitChecksAsync(role);
            }

            this.DataContext = this;
        }

        private async Task InitChecksAsync(string role)
        {
            var userInfo = await GetCurrentUserInfoAsync();
            if (userInfo == null) return;

            HasCompany(userInfo.CompanyId);
            if (userInfo.CompanyId.HasValue)
                await CheckCompanyStatusAsync(userInfo.CompanyId.Value);

            if (role != "Administrator")
                HasDepartment(userInfo.DepartmentId);
        }

        private async Task<Guid?> GetCurrentUserCompanyIdAsync()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var userIdStr = Application.Current.Properties["UserId"]?.ToString();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr))
                    return null;

                if (!Guid.TryParse(userIdStr, out var userId))
                    return null;

                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + $"/api/User/{userId}";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<UserResultDetailDto>>(json);

                    if (result != null && result.Data?.CompanyId != null)
                    {
                        //MessageBox.Show("Lấy được CompanyId: " + result.Data.CompanyId); 
                        return result.Data.CompanyId;
                    }
                }
                //else
                //{
                //    MessageBox.Show($"Get /api/User/{userId} failed: {response.StatusCode}");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user info: {ex.Message}");
            }

            return null;
        }

        private async Task LoadCompanyImageAsync()
        {
            try
            {
                var companyId = await GetCurrentUserCompanyIdAsync();
                //if (companyId == null)
                //{
                //    MessageBox.Show("Không tìm thấy CompanyId");
                //    return;
                //}

                //MessageBox.Show("CompanyId: " + companyId);

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + $"/api/Company/{companyId}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<CompanyResultDto>>(json);

                    if (result != null && !string.IsNullOrEmpty(result.Data?.LogoUrl))
                    {
                        //MessageBox.Show("LogoUrl: " + result.Data.LogoUrl); 
                        CompanyImagePath = result.Data.LogoUrl;

                        this.DataContext = null;
                        this.DataContext = this;
                    }
                    //else
                    //{
                    //    MessageBox.Show("Không có LogoUrl trong dữ liệu trả về");
                    //}
                }
                //else
                //{
                //    MessageBox.Show($"Get /api/Company/{companyId} failed: {response.StatusCode}");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading company image: {ex.Message}");
            }
        }

        private void CompanyImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var img = sender as Image;
            img.Source = new BitmapImage(new Uri("assets/images.png", UriKind.Relative));
        }
        private async Task<UserResultDetailDto?> GetCurrentUserInfoAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var userIdStr = Application.Current.Properties["UserId"]?.ToString();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdStr))
                return null;

            if (!Guid.TryParse(userIdStr, out var userId))
                return null;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var userApiUrl = AppsettingConfigHelper.GetBaseUrl() + $"/api/User/{userId}";
            var userResponse = await client.GetAsync(userApiUrl);

            if (!userResponse.IsSuccessStatusCode)
            {
                MessageBox.Show($"Không thể lấy thông tin người dùng. Status: {userResponse.StatusCode}");
                return null;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userResult = JsonConvert.DeserializeObject<ApiResponse<UserResultDetailDto>>(userJson);
            return userResult?.Data;
        }
        public async Task CheckCompanyStatusAsync(Guid companyId)
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var companyApiUrl = AppsettingConfigHelper.GetBaseUrl() + $"/api/Company/{companyId}";
            var companyResponse = await client.GetAsync(companyApiUrl);

            if (!companyResponse.IsSuccessStatusCode)
            {
                MessageBox.Show("Bạn chưa có công ty. Vui lòng liên hệ admin.",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var companyJson = await companyResponse.Content.ReadAsStringAsync();
            var companyResult = JsonConvert.DeserializeObject<ApiResponse<CompanyResultDto>>(companyJson);

            if (companyResult?.Data?.IsDeleted == true || companyResult?.Data?.IsActive == false)
            {
                MessageBox.Show("Công ty của bạn đã bị vô hiệu hóa. Vui lòng liên hệ admin.",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }
        }
        public void HasCompany(Guid? companyId)
        {
            if (companyId == null)
            {
                MessageBox.Show("Bạn chưa có công ty. Vui lòng liên hệ admin.",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void HasDepartment(Guid? departmentId)
        {
            if (departmentId == null)
            {
                MessageBox.Show("Bạn chưa có phòng ban. Vui lòng liên hệ admin.",
                                "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void StartClockTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // cập nhật mỗi giây
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void GetName(string name)
        {
            txtFullName.Text = $"Xin chào, {name}!";
        }

        private void ApplyRolePermissions(string role)
        {
            switch (role)
            {
                case "SystemAdmin":
                    btnDepartment.Content = "Xem phòng ban";
                    btnPosition.Content = "Xem chức vụ";
                    btnDuty.Content = "Xem công việc";
                    btnCheckin.Content = "Xem Checkin";
                    btnPayroll.Content = "Xem chấm công";
                    btnConfig.Content = "Xem cấu hình";
                    break;

                case "Administrator":
                    btnCompany.Visibility = Visibility.Collapsed;
                    break; 

                case "Manager":
                    btnDepartment.Visibility = Visibility.Collapsed;
                    btnCompany.Visibility = Visibility.Collapsed;
                    btnConfig.Content = "Xem cấu hình";
                    break;

                case "Employee":
                    foreach (var control in new[] { btnCompany, btnDashboard, btnUser, btnDepartment, btnPosition})
                        control.Visibility = Visibility.Collapsed;
                    btnPayroll.Content = "Chấm công";
                    btnCheckin.Content = "Checkin";
                    btnDuty.Content = "Công việc";
                    btnConfig.Content = "Cấu hình";
                    btnConfig.Content = "Xem cấu hình";
                    break;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            DateTimeNow.Text = vnTime.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void ShowDefaultContent()
        {
            MainFrame.Content = null;
            MainFrame.Visibility = Visibility.Collapsed;
            DefaultContent.Visibility = Visibility.Visible;
        }

        private void ShowPage(Page page)
        {
            MainFrame.Navigate(page);
            DefaultContent.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
        }

        private void TrangChu_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageDashboard());  // Quay về giao diện ban đầu
        }
        private void Company(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageCompany());
        }
        private void User(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageUser()); // Chuyển sang page Tài khoản
        }
        private void Department(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageDepartment()); 
        }
        private void Position(object sender, RoutedEventArgs e)
        {
            ShowPage(new PagePosition());
        }
        private void Duty(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageDuty());
        }
        private void Checkin(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageCheckin());
        }
        private void Payroll(object sender, RoutedEventArgs e)
        {
            ShowPage(new PagePayroll());
        }
        private void Config(object sender, RoutedEventArgs e)
        {
            ShowPage(new PageConfig());
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return; 
            }
            Application.Current.Shutdown(); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserInfo();
            window.ShowDialog();
        }


    }
}