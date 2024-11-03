using System;
using System.Runtime.InteropServices;

//未经过审批不得改动

//Alvin 20230414

//浮点数和取字节转换

//MyDevice.myUIT.I = 0x12345678;
//B0 = 0x78
//B1 = 0x56
//B2 = 0x34
//B3 = 0x12
//MessageBox.Show(string.Format(("B0={0}, B1={1}, B2={2}, B3={3}"),
//    MyDevice.myUIT.B0,
//    MyDevice.myUIT.B1,
//    MyDevice.myUIT.B2,
//    MyDevice.myUIT.B3,
//    ));

namespace Model
{
    //数据转换使用
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public partial class UIT //小端模式
    {
        [FieldOffset(0)]
        private Byte b0;
        [FieldOffset(1)]
        private Byte b1;
        [FieldOffset(2)]
        private Byte b2;
        [FieldOffset(3)]
        private Byte b3;

        [FieldOffset(0)]
        private UInt16 s;

        [FieldOffset(0)]
        private Int32 i;

        [FieldOffset(0)]
        private UInt32 ui;

        [FieldOffset(0)]
        private float f;

        //
        #region set and get
        //

        public Byte B0 //LSB
        {
            set
            {
                b0 = value;
            }
            get
            {
                return b0;
            }
        }
        public Byte B1
        {
            set
            {
                b1 = value;
            }
            get
            {
                return b1;
            }
        }
        public Byte B2
        {
            set
            {
                b2 = value;
            }
            get
            {
                return b2;
            }
        }
        public Byte B3 //MSB
        {
            set
            {
                b3 = value;
            }
            get
            {
                return b3;
            }
        }
        public UInt16 S
        {
            set
            {
                s = value;
            }
            get
            {
                return s;
            }
        }
        public Int32 I
        {
            set
            {
                i = value;
            }
            get
            {
                return i;
            }
        }
        public UInt32 UI
        {
            set
            {
                ui = value;
            }
            get
            {
                return ui;
            }
        }
        public float F
        {
            set
            {
                f = value;
            }
            get
            {
                return f;
            }
        }

        //
        #endregion
        //

    }
}
