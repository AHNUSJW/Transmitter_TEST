using System;
using System.Text;
//Lumi 20240531

namespace Library
{
    public static class BaseConvert
    {
        // 将十进制转换为十六进制
        public static string ConvertDecToHex(double decimalNumber)
        {
            // 判断是否为负数
            bool isNegative = decimalNumber < 0;
            if (isNegative)
            {
                decimalNumber = Math.Abs(decimalNumber);
            }

            // 将小数部分乘以16，得到整数部分和小数部分
            double integerPart = Math.Floor(decimalNumber);
            double fractionalPart = decimalNumber - integerPart;

            // 将整数部分转换为十六进制
            string hexIntegerPart = Convert.ToInt32(integerPart).ToString("X");

            // 将小数部分转换为十六进制
            string hexFractionalPart = ConvertFractionalToHex(fractionalPart);

            // 拼接整数部分和小数部分的十六进制表示
            string hexNumber;
            if (hexFractionalPart.ToString().Length == 0)
            {
                hexNumber = hexIntegerPart;
            }
            else
            {
                hexNumber = hexIntegerPart + "." + hexFractionalPart;
            }

            // 添加负号
            if (isNegative)
            {
                hexNumber = "-" + hexNumber;
            }

            return hexNumber;
        }

        // 将十进制的小数部分转换为十六进制
        public static string ConvertFractionalToHex(double fractionalPart)
        {
            StringBuilder hexBuilder = new StringBuilder();

            while (fractionalPart > 0 && hexBuilder.Length < 6)
            {
                // 将小数部分乘以16，得到整数部分和新的小数部分
                double multipliedValue = fractionalPart * 16;
                double integerPart = Math.Floor(multipliedValue);
                fractionalPart = multipliedValue - integerPart;

                // 将整数部分转换为十六进制，并添加到结果中
                string hexDigit = Convert.ToInt32(integerPart).ToString("X");
                hexBuilder.Append(hexDigit);
            }

            return hexBuilder.ToString();
        }
    }
}
