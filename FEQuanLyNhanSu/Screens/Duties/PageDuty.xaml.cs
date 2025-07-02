using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Screens.Duties;
using System.Windows;
using System.Windows.Controls;
using static FEQuanLyNhanSu.ResponseModels.Duties;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption4.xaml
    /// </summary>
    public partial class PageDuty : Page
    {
        private PaginationHelper<DutyResultDto> _paginationHelper;
        public PageDuty()
        {
            InitializeComponent();
            LoadDuty();
        }

        private void LoadDuty()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Duty";
                int pageSize = 20;
                _paginationHelper = new PaginationHelper<DutyResultDto>(
                    baseUrl,
                    pageSize,
                    token,
                    items => DutyDtaGrid.ItemsSource = items,
                    txtPage
                );
                _ = _paginationHelper.LoadPageAsync(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void txtTextChanged(object sender, TextChangedEventArgs e)
        {
            var token = Application.Current.Properties["Token"].ToString();
            string keyword = txtSearch.Text?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
                LoadDuty();
            else
            {
                var result = await SearchHelper.SearchAsync<DutyResultDto>("api/Duty", keyword, token);
                DutyDtaGrid.ItemsSource = result;
            }
        }


        private void AddDuty(object sender, RoutedEventArgs e)
        {
            var window = new CreateDuty();
            window.Show();
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
