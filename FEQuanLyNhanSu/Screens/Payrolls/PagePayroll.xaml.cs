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
using FEQuanLyNhanSu.Screens.Payrolls;
using Newtonsoft.Json;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PagePayroll.xaml
    /// </summary>
    public partial class PagePayroll : Page
    {

        private PaginationHelper<Payrolls.PayrollResultDto> _paginationHelper;
        public PagePayroll()
        {
            InitializeComponent();
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Payroll";
            int pageSize = 20;
            _paginationHelper = new PaginationHelper<Payrolls.PayrollResultDto>(
                baseUrl,
                pageSize,
                token,
                items => PayrollDtaGrid.ItemsSource = items,
                txtPage
            );
            _ = _paginationHelper.LoadPageAsync(1);
        }

        private void AddPayroll(object sender, RoutedEventArgs e)
        {
            var window = new CreatePayroll();
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
