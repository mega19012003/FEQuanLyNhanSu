using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Screens.Checkins;
using FEQuanLyNhanSu.Screens.Positions;
using FEQuanLyNhanSu.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.ResponseModels.Positions;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption5.xaml
    /// </summary>
    public partial class PageCheckin : Page
    {
        private PaginationHelper<Checkins.CheckinResultDto> _paginationHelper;
        public PageCheckin()
        {
            InitializeComponent();
            HandleUI(Application.Current.Properties["UserRole"]?.ToString());
            LoadCheckin();
            LoadDateComboboxes();
        }

        private void HandleUI(string role)
        {
            switch (role)
            {
                case "Administrator":
                    break;
                case "Manager":
                    break;
                case "Employee":
                    DtaGridAction.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void LoadDateComboboxes()
        {
            var days = new List<string> { "Ngày" };
            days.AddRange(Enumerable.Range(1, 31).Select(i => i.ToString()));
            cbDay.ItemsSource = days;

            var months = new List<string> { "Tháng" };
            months.AddRange(Enumerable.Range(1, 12).Select(i => i.ToString()));
            cbMonth.ItemsSource = months;

            int currentYear = DateTime.Now.Year;
            var years = new List<string> { "Năm" };
            years.AddRange(Enumerable.Range(2000, currentYear - 2000 + 1).Select(i => i.ToString()).Reverse());
            cbYear.ItemsSource = years;

            cbDay.SelectedIndex = 0;
            cbMonth.SelectedIndex = 0;
            cbYear.SelectedIndex = 0;
        }

        private void LoadCheckin()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Checkin";
            int pageSize = 20;

            _paginationHelper = new PaginationHelper<Checkins.CheckinResultDto>(
                baseUrl,
                pageSize,
                token,
                items => CheckinDtaGrid.ItemsSource = items,
                txtPage
            );

            _ = _paginationHelper.LoadPageAsync(1);
        }

        private async Task FilterAsync()
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearch.Text?.Trim();
            int? day = cbDay.SelectedItem as int?;
            int? month = cbMonth.SelectedItem as int?;
            int? year = cbYear.SelectedItem as int?;

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(keyword))
                queryParams.Add($"Search={Uri.EscapeDataString(keyword)}");
            if (day.HasValue && day > 0)
                queryParams.Add($"Day={day.Value}");
            if (month.HasValue && month > 0)
                queryParams.Add($"Month={month.Value}");
            if (year.HasValue && year > 0)
                queryParams.Add($"Year={year.Value}");

            string url = "api/Checkin";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var result = await SearchHelper.Search2Async<Checkins.CheckinResultDto>(url, token);
            CheckinDtaGrid.ItemsSource = result;
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            await FilterAsync();
        }

        private async void cbDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //await FilterAsync();
        }

        private async void cbMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //await FilterAsync();
        }

        private async void cbYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //await FilterAsync();
        }

        //private async void txtTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var token = Application.Current.Properties["Token"].ToString();
        //    string keyword = txtSearch.Text?.Trim();

        //    if (string.IsNullOrWhiteSpace(keyword))
        //        LoadCheckin();
        //    else { 
        //        var result = await SearchHelper.SearchAsync<Checkins.CheckinResultDto>("api/Checkin", keyword, token);
        //        CheckinDtaGrid.ItemsSource = result;
        //    }
        //}

        private void AddCheckinBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new Checkin();
                window.Show();
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi chi tiết
                MessageBox.Show($"Đã xảy ra lỗi:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Hoặc nếu muốn xem cả stacktrace để debug
                // MessageBox.Show($"Đã xảy ra lỗi:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCheckouBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new Checkout();
            window.Show();
        }

        private async void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.NextPageAsync();
        }

        private async void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            await _paginationHelper.PrevPageAsync();
        }
    }
}
