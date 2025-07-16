using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Companies;

namespace FEQuanLyNhanSu.Screens.Companies
{
    /// <summary>
    /// Interaction logic for CreateCompany.xaml
    /// </summary>
    public partial class CreateCompany : Window
    {
        private string _imagePath;
        private Action<CompanyResultDto> _onCreated;
        public CreateCompany(Action<CompanyResultDto> onCreated)
        {
            InitializeComponent();
            _onCreated = onCreated;
        }


        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Chọn ảnh", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
            if (dialog.ShowDialog() == true)
            {
                txtImage.Text = System.IO.Path.GetFileName(dialog.FileName);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgAvatar.Source = bitmap;

                _imagePath = dialog.FileName;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var token = Application.Current.Properties["Token"]?.ToString();

            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrEmpty(_imagePath))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                return;
            }

            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(txtName.Text.Trim()), "Name");
            content.Add(new StringContent(txtAddress.Text.Trim()), "Address");

            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
            {
                var fileStream = File.OpenRead(_imagePath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); // hoặc tự detect
                content.Add(fileContent, "LogoUrl", System.IO.Path.GetFileName(_imagePath));
            }

            var response = await client.PostAsync(baseUrl, content);

            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CompanyResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Data != null)
                {
                    _onCreated?.Invoke(apiResponse.Data);
                }

                MessageBox.Show("Tạo công ty thành công.");
                this.Close();
            }
            else
            {
                var apiResponses = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponses?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Tạo công ty thất bại: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
