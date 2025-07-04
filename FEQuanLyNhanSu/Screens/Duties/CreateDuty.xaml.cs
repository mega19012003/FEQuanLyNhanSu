﻿using FEQuanLyNhanSu.Base;
using FEQuanLyNhanSu.Helpers;
using Newtonsoft.Json;
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
using System.Windows.Shapes;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.Screens.Duties
{
    /// <summary>
    /// Interaction logic for CreateDuty.xaml
    /// </summary>
    public partial class CreateDuty : Window
    {
        private readonly Action _onDutyCreated;
        public CreateDuty(Action onDutyCreated)
        {
            InitializeComponent();
            LoadUsers();
            _onDutyCreated = onDutyCreated;
        }

        private async void cbEmployee_KeyUp(object sender, KeyEventArgs e)
        {
            string keyword = cbEmployee.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                cbEmployee.ItemsSource = null;
                return;
            }

            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{baseUrl}?Search={Uri.EscapeDataString(keyword)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);
                cbEmployee.ItemsSource = result.Data.Items;
                cbEmployee.IsDropDownOpen = true;
            }
            else
            {
                cbEmployee.ItemsSource = null;
            }
        }


        private async Task LoadUsers()
        {
            var token = Application.Current.Properties["Token"]?.ToString();
            var baseUrl = AppsettingConfigHelper.GetBaseUrl() + "/api/User";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(baseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserResultDto>>>(json);

                cbEmployee.ItemsSource = result.Data.Items;
            }
            else
            {
                MessageBox.Show("Không thể tải danh sách nhân viên.");
            }
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDescription.Text) || dpStartDate.Value == null || dpEndDate.Value == null)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                    return;
                }
                var selectedUser = cbEmployee.SelectedItem as UserResultDto;
                if (selectedUser == null)
                {
                    MessageBox.Show("Vui lòng chọn nhân viên thực hiện");
                    return;
                }
                var duty = new
                {
                    name = txtUsername.Text.Trim(),
                    startDate = dpStartDate.Value,
                    endDate = dpEndDate.Value,
                    dutyDetails = new[]
                    {
                        new {
                            userId = selectedUser.UserId,
                            description = txtDescription.Text?.Trim()
                        }
                    }
                };

                var token = Application.Current.Properties["Token"]?.ToString();
                var baseUrl = AppsettingConfigHelper.GetBaseUrl();

                var json = JsonConvert.SerializeObject(duty);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync($"{baseUrl}/api/Duty", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tạo công việc thành công!");
                    this.Close(); 
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Tạo thất bại: {error}");
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
                return;
            this.Close();
        }
    }
}
