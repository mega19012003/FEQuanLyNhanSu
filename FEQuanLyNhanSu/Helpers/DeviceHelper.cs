using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.Helpers
{
    public static class DeviceHelper
    {
        public static string GetDeviceSignature()
        {
            try
            {
                var cpuId = GetWmiProperty("Win32_Processor", "ProcessorId");
                var biosSerial = GetWmiProperty("Win32_BIOS", "SerialNumber");
                var diskSerial = GetWmiProperty("Win32_DiskDrive", "SerialNumber");

                string raw = $"{cpuId}|{biosSerial}|{diskSerial}";
                return Sha256Hash(raw);
            }
            catch
            {
                return Environment.MachineName; 
            }
        }

        private static string GetWmiProperty(string wmiClass, string property)
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj[property]?.ToString() ?? "";
            }
            return "";
        }

        private static string Sha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash); // .NET 5+; use BitConverter for older
        }
    }
}
