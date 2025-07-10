//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace FEQuanLyNhanSu.Helpers
//{
//    class ApiResponseHelper
//    {
//        private async Task<T> ReadResponseOrThrowAsync<T>(HttpResponseMessage response)
//        {
//            var content = await response.Content.ReadAsStringAsync();

//            if (response.IsSuccessStatusCode)
//            {
//                return JsonConvert.DeserializeObject<T>(content);
//            }
//            else
//            {
//                try
//                {
//                    dynamic errorObj = JsonConvert.DeserializeObject<dynamic>(content);
//                    string message = errorObj?.message ?? errorObj?.error ?? "Đã xảy ra lỗi!";
//                    throw new Exception(message);
//                }
//                catch
//                {
//                    throw new Exception($"API trả về lỗi: {response.StatusCode} - {content}");
//                }
//            }
//        }
//    }
//}
