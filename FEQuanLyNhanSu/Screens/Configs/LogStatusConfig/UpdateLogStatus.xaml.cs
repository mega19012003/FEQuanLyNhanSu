﻿using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using FEQuanLyNhanSu.Models.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

namespace FEQuanLyNhanSu.Screens.Configs.LogStatusConfig
{
    /// <summary>
    /// Interaction logic for UpdateLogStatus.xaml
    /// </summary>
    public partial class UpdateLogStatus : Window
    {
        private Guid _logStatusId;
        private Action _onLogStatusUpdated;
        public UpdateLogStatus(Guid logStatusId, Action onCreated)
        {
            InitializeComponent();
            _logStatusId = logStatusId;
            _onLogStatusUpdated = onCreated;
            _ = LoadLogStatusAsync();
        }
        private HttpClient CreateAuthorizedClient(string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
        private async Task LoadLogStatusAsync()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();
            var url = $"{baseUrl}/api/LogStatusConfig/{_logStatusId}";

            using var client = CreateAuthorizedClient(token);

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<FEQuanLyNhanSu.Models.Configs.LogStatusConfig>>(json);

                txtName.Text = result.Data.Name;
                //txtMultiply.Text = result.Data.SalaryMultiplier.ToString();
                txtNote.Text = result.Data.Note;
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                MessageBox.Show($"Không thể tải thông tin chức vụ: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnUpdate.IsEnabled = false;
            btnExit.IsEnabled = false;
            try
            {
                string LogStatusName = txtName.Text.Trim();
                string LogStatusMultiply = txtMultiply.Text.Trim();
                string LogStatusNote = txtNote.Text.Trim();
                if (string.IsNullOrEmpty(LogStatusName) || string.IsNullOrEmpty(LogStatusNote))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                    return;
                }
                if (!double.TryParse(LogStatusMultiply, out double multiplier))
                {
                    MessageBox.Show("Hệ số lương phải là số hợp lệ");
                    return;
                }

                if (multiplier < 0)
                {
                    MessageBox.Show("Hệ số lương không được nhỏ hơn 0");
                    return;
                }

                var result = new FEQuanLyNhanSu.Models.Configs.LogStatusConfig
                {
                    Id = _logStatusId,
                    Name = LogStatusName,
                    //SalaryMultiplier = double.Parse(LogStatusMultiply),
                    Note = LogStatusNote
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();




                var query = $"IPAddress={Uri.EscapeDataString(LogStatusName)}";
                var url = $"{baseUrl}/api/LogStatusConfig"; //?id={_logStatusId}&{query}";

                var json = JsonConvert.SerializeObject(result);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = CreateAuthorizedClient(token);

                var response = await client.PutAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật log status thành công.");
                    _onLogStatusUpdated?.Invoke();
                    this.Close();
                }
                else
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(json);
                    var errorData = apiResponse?.Data ?? "Có lỗi xảy ra";
                    MessageBox.Show($"Lỗi khi cập nhật: {errorData}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                btnUpdate.IsEnabled = true;
                btnExit.IsEnabled = true;
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
    }
}
