using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PalWorldServerTool.Models
{
    public class Environment
    {
        /// <summary>
        /// 检测DirectX版本
        /// </summary>
        /// <returns></returns>
        public static string? CheckDirectX()
        {
            const string DirectXKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\DirectX";
            string? version = Registry.GetValue(DirectXKey, "Version", null)?.ToString();

            if (version != null)
            {
                return version;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 检查VC++ 2015 x86
        /// </summary>
        /// <returns></returns>
        public static bool CheckVC2015x64() 
        {
            string keyPath = $@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";
            using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    var installed = key.GetValue("Installed");
                    return installed != null && installed.ToString() == "1";
                }
            }
            return false;
        }
        /// <summary>
        /// 检查VC++ 2015 x64
        /// </summary>
        /// <returns></returns>
        public static bool CheckVC2015x86()
        {
            string keyPath = $@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86";
            using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    var installed = key.GetValue("Installed");
                    return installed != null && installed.ToString() == "1";
                }
            }
            return false;
        }
        /// <summary>
        /// 判断路径是否安全
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckPathSafety(string path)
        {
            Regex regex = new Regex(@"[^\x00-\x7F\s]+");
            return regex.IsMatch(path);
        }
    }
}
