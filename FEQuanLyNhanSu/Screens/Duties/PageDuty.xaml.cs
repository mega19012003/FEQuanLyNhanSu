using System.Windows;
using System.Windows.Controls;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Screens.Duties;
using static FEQuanLyNhanSu.ResponseModels.Duties;

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
