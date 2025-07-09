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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static FEQuanLyNhanSu.ResponseModels.Departments;
using static FEQuanLyNhanSu.ResponseModels.Positions;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Users
{
    /// <summary>
    /// Interaction logic for UpdateInfo.xaml
    /// </summary>
    public partial class UpdateInfo : Window
    {
        private Guid _currentUserId;
        private string _imagePath;

        public UpdateInfo()
        {
            InitializeComponent();
            _ = LoadCurrentUserAsync();
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/Auth/current";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(baseUrl);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserResultDto>>(json);
                    var user = apiResponse?.Data;

                    if (user != null)
                    {
                        _currentUserId = user.UserId;
                        txtFullname.Text = user.Fullname;
                        txtPhoneNo.Text = user.PhoneNumber;
                        txtAddress.Text = user.Address;

                        if (!string.IsNullOrEmpty((string)user.ImageUrl))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri((string)user.ImageUrl, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            imgAvatar.Source = bitmap;
                        }
                        else
                        {
                            imgAvatar.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/user.png"));
                        }
                    }
                }
                else
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Không thể tải thông tin người dùng: {json}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            this.Close();
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                btnUpdate.IsEnabled = false;

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

                if (txtPhoneNo.Text.Length < 10 || txtPhoneNo.Text.Length > 11 || !txtPhoneNo.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Số điện thoại phải có độ dài từ 10 đến 11 ký tự và chỉ chứa số.");
                    return;
                }

                var formData = new MultipartFormDataContent
                {
                    { new StringContent(_currentUserId.ToString()), "UserId" },
                    { new StringContent(txtFullname.Text), "Fullname" },
                    { new StringContent(txtPhoneNo.Text), "PhoneNumber" },
                    { new StringContent(txtAddress.Text), "Address" },
                    { new StringContent(txtImage.Text), "ImageUrl" },
                };


                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    var filestream = File.OpenRead(_imagePath);
                    var fileContent = new StreamContent(filestream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    formData.Add(fileContent, "ImageUrl", System.IO.Path.GetFileName(_imagePath));
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await client.PutAsync(baseUrl, formData);
                var resultJson = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thông tin thành công!");
                    this.DialogResult = true; 
                    this.Close();
                }
                else
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(resultJson);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Lỗi khi cập nhật: {resultJson}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnUpdate.IsEnabled = true;
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
