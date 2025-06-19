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
using Newtonsoft.Json;
using static FEQuanLyNhanSu.ResponseModels.Duties;

namespace FEQuanLyNhanSu
{
    /// <summary>
    /// Interaction logic for PageOption4.xaml
    /// </summary>
    public partial class PageDuty : Page
    {
        public PageDuty()
        {
            InitializeComponent();
            LoadDuty();
        }

        private async void LoadDuty()
        {
            var token = Application.Current.Properties["Token"]?.ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("https://demonhanvienapi.duckdns.org/api/Duty");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var apiResult = JsonConvert.DeserializeObject<ApiResponse<PagedResult<DutyDto>>>(json);

                    var duties = apiResult?.Data?.Items ?? new List<DutyDto>();

                    // Đảm bảo DutyDetails luôn khởi tạo để tránh crash
                    foreach (var duty in duties)
                    {
                        if (duty.DutyDetails == null)
                            duty.DutyDetails = new List<DutyDetailDto>();
                    }

                    DutyDtaGrid.ItemsSource = duties;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách công việc.");
                }
            }
        }

    }
}
