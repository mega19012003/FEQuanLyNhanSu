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
using FEQuanLyNhanSu.Screens.Dashboard;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            StartClockTimer(); // Khởi tạo đồng hồ
            GetName(Application.Current.Properties["Fullname"]?.ToString());
            ApplyRolePermissions(Application.Current.Properties["UserRole"]?.ToString());
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
                case "Administrator":
                    break; 

                case "Manager":
                    btnDepartment.Visibility = Visibility.Collapsed;
                    break;

                case "Employee":
                    foreach (var control in new[] { /*btnDashboard,*/ btnUser, btnDepartment, btnPosition})
                        control.Visibility = Visibility.Collapsed;
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

        private void Dashboard(object sender, RoutedEventArgs e)
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
    }
}