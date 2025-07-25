using EmployeeAPI.Models;
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static FEQuanLyNhanSu.ResponseModels.Dashboards;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FEQuanLyNhanSu.Screens.Dashboard
{
    public partial class PageDashboard : Page
    {
        public DashboardOverviewDto Dashboard { get; set; }

        public PageDashboard()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            this.DataContext = this;
            LoadDashboard();
        }
        private void HandleUI(string role)
        {
            switch (role)
            {
                case "Manager":
                    BrdrCompany.Visibility = Visibility.Collapsed;
                    BrdrDepartment.Visibility = Visibility.Collapsed;
                    break;
                case "Administrator":
                    BrdrCompany.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private async void LoadDashboard()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var apiUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Dashboard/overview";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    //MessageBox.Show($"API response JSON: {json}");
                    var apiResponse = JsonConvert.DeserializeObject<Base.ApiResponse<DashboardOverviewDto>>(json);
                    Dashboard = apiResponse.Data;

                    this.DataContext = null;
                    this.DataContext = this;
                    //MessageBox.Show($"Calling API: {apiUrl}");
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? apiResponse = JsonConvert.DeserializeObject<dynamic>(json);
                    string errorMessage = apiResponse?.Data != null ? apiResponse.Data.ToString() : apiResponse?.Message != null ? apiResponse.Message.ToString() : "Có lỗi xảy ra";
                    MessageBox.Show($"Không thể load dashboard. Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }
    }
}

