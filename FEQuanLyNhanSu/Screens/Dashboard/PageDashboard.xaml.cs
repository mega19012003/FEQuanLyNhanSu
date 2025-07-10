using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static FEQuanLyNhanSu.ResponseModels.Dashboards;

namespace FEQuanLyNhanSu.Screens.Dashboard
{
    public partial class PageDashboard : Page
    {
        public DashboardOverviewDto Dashboard { get; set; }

        public PageDashboard()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadDashboard();
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
                    MessageBox.Show($"Không thể load dashboard: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

    }
}

