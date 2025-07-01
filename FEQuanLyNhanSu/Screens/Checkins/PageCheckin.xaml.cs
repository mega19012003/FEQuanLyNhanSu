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
using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.ResponseModels;
using FEQuanLyNhanSu.Services;
using Newtonsoft.Json;
using static FEQuanLyNhanSu.ResponseModels.Duties;

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
