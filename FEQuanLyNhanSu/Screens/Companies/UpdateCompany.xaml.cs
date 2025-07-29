using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Departments;
using FEQuanLyNhanSu.Models.Positions;
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
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Companies
{
    /// <summary>
    /// Interaction logic for CreateCompany.xaml
    /// </summary>
    public partial class UpdateCompany : Window
    {
        private Guid _companyId;
        private Action<CompanyResultDto> _onUpdated;
        private string _imagePath;
        public UpdateCompany(Guid companyId, Action<CompanyResultDto> onUpdated)
        {
            InitializeComponent();
            _companyId = companyId;
            _onUpdated = onUpdated;
            LoadComapany();
        }
        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async void LoadComapany()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();
                var url = $"{baseUrl}/api/Company/{_companyId}";

                using var client = CreateAuthorizedClient(token);
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<CompanyResultDto>>(json);
                    var user = result.Data;

                    txtName.Text = user.Name;
                    txtAddress.Text = user.Address;
                    txtImage.Text = user.LogoUrl;
                    chkIsActive.IsChecked = user.IsActive;

                    if (!string.IsNullOrEmpty(user.LogoUrl))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(user.LogoUrl, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        imgAvatar.Source = bitmap;
                    }
                    else
                    {
                        imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/none.jpg"));
                    }
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                    var errorMessage = apiResponse?.Data?.ToString() ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Không thể tải thông tin công ty. Lỗi: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
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
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                btnUpdate.IsEnabled = false;

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Company";

                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtAddress.Text) || string.IsNullOrWhiteSpace(txtImage.Text))
                {
                    MessageBox.Show("Vui lòng nhập thông tin đầy đủ.");
                    return;
                }

                var formData = new MultipartFormDataContent
                {
                    { new StringContent(_companyId.ToString()), "CompanyId" },
                    { new StringContent(txtName.Text), "Name" },
                    { new StringContent(txtAddress.Text), "Address" },
                    { new StringContent(chkIsActive.IsChecked == true ? "true" : "false"), "IsActive" },
                    { new StringContent(txtImage.Text), "LogoUrl" },
                };

                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    var filestream = File.OpenRead(_imagePath);
                    var fileContent = new StreamContent(filestream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formData.Add(fileContent, "ImageUrl", System.IO.Path.GetFileName(_imagePath));
                }

                using var client = CreateAuthorizedClient(token);
                var response = await client.PutAsync(baseUrl, formData);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CompanyResultDto>>(json);
                    if (apiResponse?.Data != null)
                    {
                        MessageBox.Show("Cập nhật thông tin công ty thành công.");
                        _onUpdated?.Invoke(apiResponse.Data);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thành công nhưng không nhận được dữ liệu công ty.");
                    }
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(errorJson);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                   
                    MessageBox.Show($"Không thể cập nhật thông tin công ty. Lỗi: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin công ty: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnUpdate.IsEnabled = true;
                btnExit.IsEnabled = true;
            }
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
    }
}
