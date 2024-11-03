using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

//Lumi 20240827

namespace Library
{
    public enum UNIT : byte
    {
        //Description 显示到界面
        //针对无和其他，显示到combobox时显示UnitCategory
        [Description(""), UnitCategory("无")]
        无 = 0x00,

        // 重量单位 (0x01 - 0x0F)
        [Description("kg"), UnitCategory("重量")]
        kg = 0x01,
        [Description("lb"), UnitCategory("重量")]
        lb = 0x02,
        [Description("oz"), UnitCategory("重量")]
        oz = 0x03,
        [Description("g"), UnitCategory("重量")]
        g = 0x04,
        [Description("mg"), UnitCategory("重量")]
        mg = 0x05,
        [Description("t"), UnitCategory("重量")]
        t = 0x06,
        [Description("ct"), UnitCategory("重量")]
        ct = 0x07,

        // 预留空间
        // 0x08 - 0x1F

        // 力单位 (0x20 - 0x2F)
        [Description("N"), UnitCategory("力")]
        N = 0x20,
        [Description("kN"), UnitCategory("力")]
        kN = 0x21,
        [Description("lbf"), UnitCategory("力")]
        lbf = 0x22,
        [Description("kgf"), UnitCategory("力")]
        kgf = 0x23,

        // 预留空间
        // 0x24 - 0x2F

        // 扭矩单位 (0x30 - 0x3F)
        [Description("N·m"), UnitCategory("扭矩")]
        Nm = 0x30,
        [Description("lbf·in"), UnitCategory("扭矩")]
        lbfin = 0x31,
        [Description("lbf·ft"), UnitCategory("扭矩")]
        lbfft = 0x32,
        [Description("kgf·cm"), UnitCategory("扭矩")]
        kgfcm = 0x33,
        [Description("kgf·m"), UnitCategory("扭矩")]
        kgfm = 0x34,

        // 预留空间
        // 0x35 - 0x3F

        // 压力单位 (0x40 - 0x4F)
        [Description("Pa"), UnitCategory("压力")]
        Pa = 0x40,
        [Description("kPa"), UnitCategory("压力")]
        kPa = 0x41,
        [Description("MPa"), UnitCategory("压力")]
        MPa = 0x42,
        [Description("bar"), UnitCategory("压力")]
        bar = 0x43,
        [Description("mbar"), UnitCategory("压力")]
        mbar = 0x44,
        [Description("kgf/cm2"), UnitCategory("压力")]
        kgfcm2 = 0x45,
        [Description("psi"), UnitCategory("压力")]
        psi = 0x46,
        [Description("atm"), UnitCategory("压力")]
        atm = 0x47,
        [Description("inHg"), UnitCategory("压力")]
        inHg = 0x48,
        [Description("mmHg"), UnitCategory("压力")]
        mmHg = 0x49,
        [Description("Torr"), UnitCategory("压力")]
        Torr = 0x4A,


        // 预留空间
        // 0x4A - 0x4F

        // 温度单位 (0x50 - 0x5F)
        [Description("℃"), UnitCategory("温度")]
        C = 0x50,
        [Description("℉"), UnitCategory("温度")]
        F = 0x51,
        [Description("K"), UnitCategory("温度")]
        K = 0x52,

        // 预留空间
        // 0x53 - 0xEF

        // 其他单位 (0xF0 - 0xFF)
        [Description("mV/V"), UnitCategory("其他")]
        mvdv = 0xF0,
        [Description(""), UnitCategory("其他")]
        内码 = 0xF1,
        // 预留空间
        // 0xF1 - 0xFF
        [Description(""), UnitCategory("其他")]
        其他 = 0xFF,
    }

    public static class UnitHelper
    {
        //获取类别
        public static string GetUnitCategory(Enum unit)
        {
            var type = unit.GetType();
            var memberInfo = type.GetMember(unit.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                if (memberInfo.GetCustomAttributes(typeof(UnitCategoryAttribute), false).FirstOrDefault() is UnitCategoryAttribute attribute)
                {
                    return attribute.Category;
                }
            }
            return "未知";
        }

        //获取描述
        public static string GetUnitDescription(Enum unit)
        {
            var type = unit.GetType();
            var memberInfo = type.GetMember(unit.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                if (memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute attribute)
                {
                    return attribute.Description;
                }
            }
            return "";
        }

        //汉字单位，返回特定字符，非汉字，返回描述
        public static string GetUnitAdjustedDescription(Enum unit)
        {
            var type = unit.GetType();
            var memberInfo = type.GetMember(unit.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                string unitName = unit.ToString();
                if (unitName == "无" || unitName == "其他")
                {
                    if (memberInfo.GetCustomAttributes(typeof(UnitCategoryAttribute), false).FirstOrDefault() is UnitCategoryAttribute categoryAttribute)
                    {
                        return categoryAttribute.Category;
                    }
                }
                else if (unitName == "内码")
                {
                    return "内码";
                }
                else
                {
                    if (memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }
            return "无";
        }

        //获取特定类型的description
        public static List<string> GetUnitDescriptionsByCategory(Enum unit, string category)
        {
            var descriptions = new List<string>();
            var enumType = unit.GetType();

            if (!enumType.IsEnum)
            {
                return null;
            }

            var members = enumType.GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (var member in members)
            {
                var unitCategoryAttribute = member.GetCustomAttributes(typeof(UnitCategoryAttribute), false).FirstOrDefault() as UnitCategoryAttribute;
                var descriptionAttribute = member.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                if (unitCategoryAttribute != null && unitCategoryAttribute.Category == category)
                {
                    var description = descriptionAttribute != null ? descriptionAttribute.Description : member.Name;
                    descriptions.Add(description);
                }
            }

            return descriptions;
        }

        //获取特定类型的值
        public static List<byte> GetUnitValuesByCategory(Enum unit, string category)
        {
            var values = new List<byte>();
            var enumType = unit.GetType();

            if (!enumType.IsEnum)
            {
                return null;
            }

            var members = enumType.GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (var member in members)
            {
                if (member.GetCustomAttributes(typeof(UnitCategoryAttribute), false).FirstOrDefault() is
                    UnitCategoryAttribute unitCategoryAttribute && unitCategoryAttribute.Category == category)
                {
                    var value = (byte)Enum.Parse(enumType, member.Name);
                    values.Add(value);
                }
            }

            return values;
        }

        public static UNIT FindUnitByDescription(string description)
        {
            foreach (var field in typeof(UNIT).GetFields())
            {
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    var attribute = (DescriptionAttribute)attributes[0];
                    if (attribute.Description == description)
                    {
                        return (UNIT)field.GetValue(null);
                    }
                }
            }
            return UNIT.无; // 如果没有找到匹配的项，返回 无
        }

        public static string ConvertUnit(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, bool isE = true)
        {
            try
            {
                return ConvertUnitInternal(originalData, decimalPoint, originalUnit, convertUnit, isE);
            }
            catch
            {
                Console.WriteLine("unit convert error");
                return originalData;
            }
        }

        public static string ConvertUnit(string originalData, byte decimalPoint, UNIT originalUnit, string convertUnitStr, bool isE = true)
        {
            try
            {
                UNIT convertUnit = FindUnitByDescription(convertUnitStr);
                return ConvertUnitInternal(originalData, decimalPoint, originalUnit, convertUnit, isE);
            }
            catch
            {
                Console.WriteLine("unit convert error");
                return originalData;
            }
        }

        public static string ConvertUnit(string originalData, byte decimalPoint, string originalUnitStr, string convertUnitStr, bool isE = true)
        {
            try
            {
                UNIT originalUnit = FindUnitByDescription(originalUnitStr);
                UNIT convertUnit = FindUnitByDescription(convertUnitStr);
                return ConvertUnitInternal(originalData, decimalPoint, originalUnit, convertUnit, isE);
            }
            catch
            {
                Console.WriteLine("unit convert error");
                return originalData;
            }
        }

        private static string ConvertUnitInternal(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, bool isE)
        {
            // 转换前后单位一致，不换算
            if (originalUnit.Equals(convertUnit))
            {
                return originalData;
            }

            string originalUnitCategory = GetUnitCategory(originalUnit);
            double convertValue;   // 转换后的数值
            int convertDecimalPoint; // 转换后的小数位

            //计算数值
            switch (originalUnitCategory)
            {
                case "重量":
                    ConvertWeightUnit(originalData, decimalPoint, originalUnit, convertUnit, out convertValue, out convertDecimalPoint);
                    break;
                case "力":
                    ConvertForceUnit(originalData, decimalPoint, originalUnit, convertUnit, out convertValue, out convertDecimalPoint);
                    break;
                case "扭矩":
                    ConvertTorqueUnit(originalData, decimalPoint, originalUnit, convertUnit, out convertValue, out convertDecimalPoint);
                    break;
                case "压力":
                    ConvertPressureUnit(originalData, decimalPoint, originalUnit, convertUnit, out convertValue, out convertDecimalPoint);
                    break;
                case "温度":
                    ConvertTemperatureUnit(originalData, decimalPoint, originalUnit, convertUnit, out convertValue, out convertDecimalPoint);
                    break;
                default:
                    return originalData;
            }

            //返回科学计数法时，小数位大于7科学计数法
            //不返回科学计数法时，补小数位
            string convertedData;    //转换后的数值
            if (isE && convertDecimalPoint >= 7)
            {
                convertedData = convertValue.ToString();
            }
            else
            {
                convertedData = convertValue.ToString($"F{convertDecimalPoint}");
            }

            return convertedData;
        }

        //重量单位及精度转换 isE:false不转换科学计数法 true小数位超过7位时转换为科学计数法
        private static void ConvertWeightUnit(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, out double convertValue, out int convertDecimalPoint)
        {
            double originalValue = double.Parse(originalData);  //原数值
            convertValue = originalValue;                     //转换后的数值
            convertDecimalPoint = decimalPoint;                 //转换后的小数位
            switch (originalUnit)
            {
                case UNIT.kg:
                    switch (convertUnit)
                    {
                        case UNIT.lb:
                            convertValue = originalValue * 2.20462;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.oz:
                            convertValue = originalValue * 35.274;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.g:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.mg:
                            convertValue = originalValue * 1000000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 6);
                            break;
                        case UNIT.t:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = decimalPoint + 3;
                            break;
                        case UNIT.ct:
                            convertValue = originalValue * 5000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.lb:
                    switch (convertUnit)
                    {
                        case UNIT.kg:
                            convertValue = originalValue * 0.453592;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.oz:
                            convertValue = originalValue * 16;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.g:
                            convertValue = originalValue * 453.592;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.mg:
                            convertValue = originalValue * 453592;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 5);
                            break;
                        case UNIT.t:
                            convertValue = originalValue * 0.000454;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.ct:
                            convertValue = originalValue * 2267.96;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.oz:
                    switch (convertUnit)
                    {
                        case UNIT.kg:
                            convertValue = originalValue * 0.02835;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.lb:
                            convertValue = originalValue * 0.0625;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.g:
                            convertValue = originalValue * 28.3495;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.mg:
                            convertValue = originalValue * 28349.5;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 4);
                            break;
                        case UNIT.t:
                            convertValue = originalValue * 0.000028;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 5);
                            break;
                        case UNIT.ct:
                            convertValue = originalValue * 141.7475;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.g:
                    switch (convertUnit)
                    {
                        case UNIT.kg:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = decimalPoint + 3;
                            break;
                        case UNIT.lb:
                            convertValue = originalValue * 0.002205;
                            convertDecimalPoint = decimalPoint + 3;
                            break;
                        case UNIT.oz:
                            convertValue = originalValue * 0.035274;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.mg:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.t:
                            convertValue = originalValue * 0.000001;
                            convertDecimalPoint = decimalPoint + 6;
                            break;
                        case UNIT.ct:
                            convertValue = originalValue * 5;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.mg:
                    switch (convertUnit)
                    {
                        case UNIT.kg:
                            convertValue = originalValue * 0.000001;
                            convertDecimalPoint = decimalPoint + 6;
                            break;
                        case UNIT.lb:
                            convertValue = originalValue * 0.000002;
                            convertDecimalPoint = decimalPoint + 6;
                            break;
                        case UNIT.oz:
                            convertValue = originalValue * 0.000035;
                            convertDecimalPoint = decimalPoint + 5;
                            break;
                        case UNIT.g:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = decimalPoint + 3;
                            break;
                        case UNIT.t:
                            convertValue = originalValue * 0.000000001;
                            convertDecimalPoint = decimalPoint + 9;
                            break;
                        case UNIT.ct:
                            convertValue = originalValue * 0.005;
                            convertDecimalPoint = decimalPoint + 3;
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.t:
                    switch (convertUnit)
                    {
                        case UNIT.kg:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.lb:
                            convertValue = originalValue * 2204.62;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.oz:
                            convertValue = originalValue * 35274;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 4);
                            break;
                        case UNIT.g:
                            convertValue = originalValue * 1000000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 6);
                            break;
                        case UNIT.mg:
                            convertValue = originalValue * 1000000000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 9);
                            break;
                        case UNIT.ct:
                            convertValue = originalValue * 5000000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 6);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.ct:
                    switch (convertUnit)
                    {
                        case UNIT.kg:
                            convertValue = originalValue * 0.0002;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.lb:
                            convertValue = originalValue * 0.000441;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.oz:
                            convertValue = originalValue * 0.007055;
                            convertDecimalPoint = decimalPoint + 3;
                            break;
                        case UNIT.g:
                            convertValue = originalValue * 0.2;
                            convertDecimalPoint = decimalPoint + 1;
                            break;
                        case UNIT.mg:
                            convertValue = originalValue * 200;
                            convertDecimalPoint = decimalPoint - 2;
                            break;
                        case UNIT.t:
                            convertValue = originalValue * 0.0000002;
                            convertDecimalPoint = decimalPoint + 7;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        //力学单位及精度转换 isE:false不转换科学计数法 true小数位超过7位时转换为科学计数法
        private static void ConvertForceUnit(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, out double convertValue, out int convertDecimalPoint)
        {
            double originalValue = double.Parse(originalData);  //原数值
            convertValue = originalValue;                     //转换后的数值
            convertDecimalPoint = decimalPoint;                 //转换后的小数位
            switch (originalUnit)
            {
                case UNIT.N:
                    switch (convertUnit)
                    {
                        case UNIT.kN:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.lbf:
                            convertValue = originalValue * 0.224809;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.kgf:
                            convertValue = originalValue * 0.101972;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.kN:
                    switch (convertUnit)
                    {
                        case UNIT.N:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.lbf:
                            convertValue = originalValue * 224.809;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.kgf:
                            convertValue = originalValue * 101.972;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.lbf:
                    switch (convertUnit)
                    {
                        case UNIT.N:
                            convertValue = originalValue * 4.44822;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.kN:
                            convertValue = originalValue * 0.004448;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.kgf:
                            convertValue = originalValue * 0.453592;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.kgf:
                    switch (convertUnit)
                    {
                        case UNIT.N:
                            convertValue = originalValue * 9.80665;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kN:
                            convertValue = originalValue * 0.0098066;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.lbf:
                            convertValue = originalValue * 2.20462;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;

            }
        }

        //扭矩单位及精度转换 isE:false不转换科学计数法 true小数位超过7位时转换为科学计数法
        private static void ConvertTorqueUnit(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, out double convertValue, out int convertDecimalPoint)
        {
            double originalValue = double.Parse(originalData);  //原数值
            convertValue = originalValue;                     //转换后的数值
            convertDecimalPoint = decimalPoint;                 //转换后的小数位
            switch (originalUnit)
            {
                case UNIT.Nm:
                    switch (convertUnit)
                    {
                        case UNIT.lbfin:
                            convertValue = originalValue * 8.85075;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.lbfft:
                            convertValue = originalValue * 0.737562;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.kgfcm:
                            convertValue = originalValue * 10.1972;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kgfm:
                            convertValue = originalValue * 0.101972;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.lbfin:
                    switch (convertUnit)
                    {
                        case UNIT.Nm:
                            convertValue = originalValue * 0.113;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.lbfft:
                            convertValue = originalValue * 0.083333;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.kgfcm:
                            convertValue = originalValue * 1.15212;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.kgfm:
                            convertValue = originalValue * 0.011521;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.lbfft:
                    switch (convertUnit)
                    {
                        case UNIT.Nm:
                            convertValue = originalValue * 1.35582;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.lbfin:
                            convertValue = originalValue * 12;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kgfcm:
                            convertValue = originalValue * 13.8255;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kgfm:
                            convertValue = originalValue * 0.138255;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.kgfcm:
                    switch (convertUnit)
                    {
                        case UNIT.Nm:
                            convertValue = originalValue * 0.098066;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.lbfin:
                            convertValue = originalValue * 0.867961;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.lbfft:
                            convertValue = originalValue * 0.07233;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.kgfm:
                            convertValue = originalValue * 0.01;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.kgfm:
                    switch (convertUnit)
                    {
                        case UNIT.Nm:
                            convertValue = originalValue * 9.80665;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.lbfin:
                            convertValue = originalValue * 86.7962;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.lbfft:
                            convertValue = originalValue * 7.23301;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.kgfcm:
                            convertValue = originalValue * 100;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;

            }
        }

        //压力单位及精度转换 isE:false不转换科学计数法 true小数位超过7位时转换为科学计数法
        private static void ConvertPressureUnit(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, out double convertValue, out int convertDecimalPoint)
        {
            double originalValue = double.Parse(originalData);  //原数值
            convertValue = originalValue;                     //转换后的数值
            convertDecimalPoint = decimalPoint;                 //转换后的小数位
            switch (originalUnit)
            {
                case UNIT.Pa:
                    switch (convertUnit)
                    {
                        case UNIT.kPa:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.000001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 6);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.00001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 5);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 0.01;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 0.0000102;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 5);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 0.000145;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.0000098;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 5);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 0.000295;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 0.0075006;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 0.0075006;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.kPa:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.01;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 10;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 0.010197;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 0.145038;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.009869;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 0.2953;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 7.50062;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 7.50062;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.MPa:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 1000000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 6);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 10;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 10000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 4);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 10.1972;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 145.038;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 9.86923;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 295.3;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 7500.62;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 7500.62;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.bar:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 100000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 5);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 100;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.1;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 1000;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 1.01972;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 14.5038;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.986923;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 29.53;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 750.062;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 750.062;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.mbar:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 100;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 0.1;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.0001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.001;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 0.0010197;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 0.0145038;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.0009869;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 0.02953;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 0.750062;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 0.750062;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.kgfcm2:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 98066.5;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 4);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 98.0665;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.0980665;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.980665;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 980.665;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 14.2233;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.967841;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 28.959;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 735.559;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 735.559;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.psi:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 6894.76;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 6.89476;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.006895;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.0689476;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 68.9476;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 0.070307;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.068046;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 2.03602;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 51.7149;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 51.7149;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.atm:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 101325;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 5);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 101.325;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.101325;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 1.01325;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 1013.25;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 1.03323;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 14.6959;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 29.9213;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 760;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 760;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.inHg:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 3386.39;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 3);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 3.38639;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.003386;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.033864;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 33.8639;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 0.0345316;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 0.491154;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.033421;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.mmHg:
                            convertValue = originalValue * 25.4;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 25.4;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 1);
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.mmHg:
                case UNIT.Torr:
                    switch (convertUnit)
                    {
                        case UNIT.Pa:
                            convertValue = originalValue * 133.322;
                            convertDecimalPoint = Math.Max(0, decimalPoint - 2);
                            break;
                        case UNIT.kPa:
                            convertValue = originalValue * 0.133322;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 1);
                            break;
                        case UNIT.MPa:
                            convertValue = originalValue * 0.000133;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 4);
                            break;
                        case UNIT.bar:
                            convertValue = originalValue * 0.001333;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.mbar:
                            convertValue = originalValue * 1.33322;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.kgfcm2:
                            convertValue = originalValue * 0.0013595;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.psi:
                            convertValue = originalValue * 0.019337;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.atm:
                            convertValue = originalValue * 0.001316;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 3);
                            break;
                        case UNIT.inHg:
                            convertValue = originalValue * 0.03937;
                            convertDecimalPoint = Math.Min(7, decimalPoint + 2);
                            break;
                        case UNIT.Torr:
                            convertValue = originalValue * 1;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        //压力单位及精度转换 isE:false不转换科学计数法 true小数位超过7位时转换为科学计数法
        private static void ConvertTemperatureUnit(string originalData, byte decimalPoint, UNIT originalUnit, UNIT convertUnit, out double convertValue, out int convertDecimalPoint)
        {
            double originalValue = double.Parse(originalData);  //原数值
            convertValue = originalValue;                     //转换后的数值
            convertDecimalPoint = decimalPoint;                 //转换后的小数位
            switch (originalUnit)
            {
                case UNIT.C:
                    switch (convertUnit)
                    {
                        case UNIT.F:
                            convertValue = originalValue * 1.8 + 32;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.K:
                            convertValue = originalValue + 273.15;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.F:
                    switch (convertUnit)
                    {
                        case UNIT.C:
                            convertValue = (originalValue - 32) * 5 / 9;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.K:
                            convertValue = (originalValue - 32) * 5 / 9 + 273.15;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                case UNIT.K:
                    switch (convertUnit)
                    {
                        case UNIT.C:
                            convertValue = originalValue - 273.15;
                            convertDecimalPoint = decimalPoint;
                            break;
                        case UNIT.F:
                            convertValue = (originalValue - 273.15) * 1.8 + 32;
                            convertDecimalPoint = decimalPoint;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;

            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class UnitCategoryAttribute : Attribute
    {
        public string Category { get; }

        public UnitCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}

