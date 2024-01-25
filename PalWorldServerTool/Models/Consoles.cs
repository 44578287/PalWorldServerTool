using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalWorldServerTool.Models
{
    public class Consoles
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
    }
}
