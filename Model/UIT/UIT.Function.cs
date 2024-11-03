using System;
using System.Text;

//未经过审批不得改动

//Alvin 20230414
//Junzhe 20231124

//浮点数和取字节转换

namespace Model
{
    public partial class UIT
    {
        public UIT()
        {
            this.i = 0;
        }

        //
        public Byte ConvertInt32ToByte(Int32 meDat)
        {
            this.i = meDat;
            return this.b0;
        }

        //
        public Byte ConvertFloatToByte(float meDat)
        {
            this.f = meDat;
            return this.b0;
        }

        //
        public float ConvertInt32ToFloat(Int32 meDat)
        {
            this.i = meDat;
            return this.f;
        }

        //
        public Int32 ConvertFloatToInt32(float meDat)
        {
            this.f = meDat;
            return this.i;
        }

        //
        public string ConvertHexToAscii(string meDat)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < meDat.Length; i += 2)
            {
                string hexChar = meDat.Substring(i, 2);
                sb.Append((char)Convert.ToByte(hexChar, 16));
            }
            return sb.ToString();
        }

        //
        public string ConvertAsciiToHex(string meDat)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in meDat)
            {
                sb.Append(Convert.ToInt32(c).ToString());
                sb.Append(' ');
            }
            return sb.ToString().Substring(0,sb.Length-1);
        }
    }
}

