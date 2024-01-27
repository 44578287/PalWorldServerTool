using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PalWorldServerTool.Models
{
    public static class Consoles
    {
        /// <summary>
        /// 自动转换文件大小单位
        /// </summary>
        /// <param name="bytes">字节</param>
        /// <param name="manualUnit">指定单位(未指定自动转换)</param>
        /// <param name="decimalPlaces">小数点位数</param>
        /// <returns>转换后字符串</returns>
        public static string ConvertByteUnits(double bytes, string manualUnit = "", int decimalPlaces = 2)
        {
            double convertedValue = bytes;
            string convertedUnit = "B";

            // 根据手动指定的单位进行转换
            if (!string.IsNullOrEmpty(manualUnit))
            {
                switch (manualUnit.ToLower())
                {
                    case "kb":
                        convertedValue /= 1024;
                        convertedUnit = "KB";
                        break;
                    case "mb":
                        convertedValue /= Math.Pow(1024, 2);
                        convertedUnit = "MB";
                        break;
                    case "gb":
                        convertedValue /= Math.Pow(1024, 3);
                        convertedUnit = "GB";
                        break;
                    case "tb":
                        convertedValue /= Math.Pow(1024, 4);
                        convertedUnit = "TB";
                        break;
                    default:
                        throw new ArgumentException("无效的单位");
                }

                return Math.Round(convertedValue, decimalPlaces).ToString() + " " + convertedUnit;
            }

            // 自动转换为推荐单位
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int unitIndex = 0;
            while (convertedValue >= 1024 && unitIndex < units.Length - 1)
            {
                convertedValue /= 1024;
                unitIndex++;
            }

            if (convertedValue < 0.01 && unitIndex > 0)
            {
                // 当转换后的值小于 0.01 且不是最小单位时，将单位回退一个级别
                convertedValue *= 1024;
                unitIndex--;
            }

            return Math.Round(convertedValue, decimalPlaces).ToString() + " " + units[unitIndex];
        }
        /// <summary>
        /// 转换单位并输出时间字符串
        /// </summary>
        /// <param name="milliseconds">毫秒</param>
        /// <param name="manualUnit">指定单位(未指定自动转换)</param>
        /// <param name="decimalPlaces">小数点位数</param>
        /// <returns>转换后的时间字符串</returns>
        public static string ConvertTimeUnits(double milliseconds, string manualUnit = "", int decimalPlaces = 2)
        {
            double convertedValue = milliseconds;
            string convertedUnit = "ms";

            // 根据手动指定的单位进行转换
            if (!string.IsNullOrEmpty(manualUnit))
            {
                switch (manualUnit.ToLower())
                {
                    case "s":
                        convertedValue /= 1000;
                        convertedUnit = "s";
                        break;
                    case "us":
                        convertedValue *= 1000;
                        convertedUnit = "us";
                        break;
                    case "min":
                        convertedValue /= 60000;
                        convertedUnit = "min";
                        break;
                    case "hr":
                        convertedValue /= 3600000;
                        convertedUnit = "hr";
                        break;
                    case "day":
                        convertedValue /= 86400000;
                        convertedUnit = "day";
                        break;
                    default:
                        throw new ArgumentException("无效的单位");
                }

                return Math.Round(convertedValue, decimalPlaces).ToString() + " " + convertedUnit;
            }

            // 自动转换为推荐单位
            string[] units = { "ms", "s", "us", "min", "hr", "day" };
            int unitIndex = 0;
            while (convertedValue >= 1 && unitIndex < units.Length - 1)
            {
                convertedValue /= 1000;
                unitIndex++;
            }

            if (convertedValue < 0.01 && unitIndex > 0)
            {
                // 当转换后的值小于 0.01 且不是最小单位时，将单位回退一个级别
                convertedValue *= 1000;
                unitIndex--;
            }

            return Math.Round(convertedValue, decimalPlaces).ToString() + " " + units[unitIndex];
        }
        /// <summary>
        /// 转换时间单位并输出时间字符串
        /// 0天 0小时 3分钟 40秒
        /// </summary>
        /// <param name="milliseconds">毫秒</param>
        /// <returns>转换后的时间字符串</returns>
        public static string ConvertTimeUnitsStr(double milliseconds)
        {
            double totalSeconds = milliseconds / 1000;

            int days = (int)(totalSeconds / 86400);
            int hours = (int)((totalSeconds % 86400) / 3600);
            int minutes = (int)(((totalSeconds % 86400) % 3600) / 60);
            int seconds = (int)(((totalSeconds % 86400) % 3600) % 60);

            return $"{days}天 {hours}小时 {minutes}分钟 {seconds}秒";
        }
        public static void ClearCMD(int startRow = 0, bool goHome = true)
        {
            Console.SetCursorPosition(0, startRow);
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;

            // 使用空格填充整个控制台屏幕
            for (int i = startRow; i < consoleHeight - 1; i++)
            {
                string line = new string(' ', consoleWidth);
                Console.WriteLine(line);
            }
            Console.SetCursorPosition(0, startRow);
        }
        public static void ClearCMDROW(int row)
        {
            Console.SetCursorPosition(0, row);
            string line = new string(' ', Console.WindowWidth);
            Console.WriteLine(line);
            Console.SetCursorPosition(0, row);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            // 计算需要多少字节才能在Base64编码后接近请求的长度
            int byteLength = (int)Math.Ceiling(length * 0.75);

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[byteLength];
                rng.GetBytes(data);
                // 将字节数组转换为Base64字符串
                string base64String = Convert.ToBase64String(data);
                // 截取所需长度的字符串
                return base64String.Substring(0, length);
            }
        }

        public static string SpectreColorConvert(bool data)
        {
            if (data)
                return "[green]True[/]";
            return "[red]False[/]";
        }

        public static string SpectreColorConvert(int data, int head, int middle, int tail)
        {
            switch (data)
            {
                case int n when n >= tail:
                    return $"[red]{n}[/]";
                case int n when n >= middle:
                    return $"[yellow]{n}[/]";
                case int n when n >= head:
                    return $"[green]{n}[/]";
            }
            return data.ToString();
        }
        public static string SpectreColorConvert(double data, int head, int middle, int tail)
        {
            switch (data)
            {
                case double n when n >= tail:
                    return $"[red]{n}[/]";
                case double n when n >= middle:
                    return $"[yellow]{n}[/]";
                case double n when n >= head:
                    return $"[green]{n}[/]";
            }
            return data.ToString();
        }

        public static string SpectreColorByteUnits(double bytes, double head, double middle, double tail, string manualUnit = "", int decimalPlaces = 2)
        {
            double convertedValue = bytes;
            string convertedUnit = "B";

            // 根据手动指定的单位进行转换
            if (!string.IsNullOrEmpty(manualUnit))
            {
                switch (manualUnit.ToLower())
                {
                    case "kb":
                        convertedValue /= 1024;
                        convertedUnit = "KB";
                        break;
                    case "mb":
                        convertedValue /= Math.Pow(1024, 2);
                        convertedUnit = "MB";
                        break;
                    case "gb":
                        convertedValue /= Math.Pow(1024, 3);
                        convertedUnit = "GB";
                        break;
                    case "tb":
                        convertedValue /= Math.Pow(1024, 4);
                        convertedUnit = "TB";
                        break;
                    default:
                        throw new ArgumentException("无效的单位");
                }

                return Math.Round(convertedValue, decimalPlaces).ToString() + " " + convertedUnit;
            }

            // 自动转换为推荐单位
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int unitIndex = 0;
            while (convertedValue >= 1024 && unitIndex < units.Length - 1)
            {
                convertedValue /= 1024;
                unitIndex++;
            }

            if (convertedValue < 0.01 && unitIndex > 0)
            {
                // 当转换后的值小于 0.01 且不是最小单位时，将单位回退一个级别
                convertedValue *= 1024;
                unitIndex--;
            }


            switch (bytes)
            {
                case double n when n >= tail:
                    return $"[red]{Math.Round(convertedValue, decimalPlaces)}[/] {units[unitIndex]}";
                case double n when n >= middle:
                    return $"[yellow]{Math.Round(convertedValue, decimalPlaces)}[/] {units[unitIndex]}";
                case double n when n >= head:
                    return $"[green]{Math.Round(convertedValue, decimalPlaces)}[/] {units[unitIndex]}";
            }

            return Math.Round(convertedValue, decimalPlaces).ToString() + " " + units[unitIndex];
        }


        public static void DisableMouseInteraction()
        {
            // 导入Windows API函数
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll")]
            static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll")]
            static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

            // 控制台句柄和控制台输入模式标志
            const int STD_INPUT_HANDLE = -10;
            const uint ENABLE_MOUSE_INPUT = 0x0010;
            // 获取标准输入句柄
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            if (consoleHandle != IntPtr.Zero)
            {
                // 获取当前控制台输入模式
                if (GetConsoleMode(consoleHandle, out uint consoleMode))
                {
                    // 禁用鼠标输入标志
                    consoleMode &= ~ENABLE_MOUSE_INPUT;

                    // 设置新的控制台输入模式
                    SetConsoleMode(consoleHandle, consoleMode);
                }
            }
        }


        public static Dictionary<Type, Func<object?, object?>> TypeTo = new()
        {
            { typeof(string), (obj) =>
            {
                if(obj?.GetType() == typeof(string))
                    return obj;
                return obj?.ToString();
            }},
            { typeof(bool), (obj) =>
            {
                if(obj?.GetType() == typeof(bool))
                    return obj;
                return bool.TryParse(obj?.ToString(), out bool result) ? result : null;
            }},
            { typeof(int), (obj) =>
            {
                if(obj?.GetType() == typeof(int))
                    return obj;
                return int.TryParse(obj?.ToString(), out int result) ? result : null;
            }},
            { typeof(float), (obj) =>
            {
                if(obj?.GetType() == typeof(float))
                    return obj;
                return float.TryParse(obj?.ToString(), out float result) ? result : null;
            }},
            { typeof(double), (obj) =>
            {
                if(obj?.GetType() == typeof(double))
                    return obj;
                return double.TryParse(obj?.ToString(), out double result) ? result : null;
            }},
            { typeof(long), (obj) =>
            {
                if(obj?.GetType() == typeof(long))
                    return obj;
                return long.TryParse(obj?.ToString(), out long result) ? result : null;
            }},
            { typeof(char), (obj) =>
            {
                if(obj?.GetType() == typeof(char))
                    return obj;
                return char.TryParse(obj?.ToString(), out char result) ? result : null;
            }}
        };

        /// <summary>
        /// 规范字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Standardize(this string? data)
        {
            return $"\"{data}\"";
        }
    }
}
