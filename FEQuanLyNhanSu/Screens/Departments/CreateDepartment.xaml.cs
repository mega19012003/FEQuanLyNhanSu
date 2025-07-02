using FEQuanLyNhanSu.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace FEQuanLyNhanSu.Screens.Departments
{
    /// <summary>
    /// Interaction logic for CreateDepartment.xaml
    /// </summary>
    public partial class CreateDepartment : Window
    {
        private Action _onDepartmentCreated;
        public CreateDepartment(Action onCreated)
        {
            InitializeComponent();
            _onDepartmentCreated = onCreated;
        }


        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên phòng ban.");
                return;
            }

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            var query = $"Name={Uri.EscapeDataString(name)}";

            var url = $"{baseUrl}/api/Department?{query}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Tạo phòng ban thành công.");
                _onDepartmentCreated?.Invoke();
                this.Close();
            }
            else
            {
                MessageBox.Show("Tạo phòng ban thất bại.");
            }
        }

        //Exit
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
