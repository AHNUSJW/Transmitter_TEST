using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Base.UI.MenuTool
{
    public partial class MenuToolRS485Form : Form
    {
        private XET actXET;//需要操作的设备

        [DllImport("user32")]
        public static extern int GetScrollPos(int hwnd, int nBar);

        private TextBox txtInput;
        public bool IsSave;
        private List<string> information = new List<string>();//表格内容
        private int Index;//选中的表格行索引
        private ListViewItem item;
        int ColumnIndex;

        public MenuToolRS485Form()
        {
            InitializeComponent();
        }

        //串口通讯响应
        private void update_FromUart()
        {
            //其它线程的操作请求
            if (this.InvokeRequired)
            {
                try
                {
                    freshHandler meDelegate = new freshHandler(update_FromUart);
                    this.Invoke(meDelegate, new object[] { });
                }
                catch
                {
                }
            }
            //本线程的操作请求
            else
            {
                //更新界面
                UpdateForm();
            }
        }

        //发送测试指令
        private void btSend_Click(object sender, EventArgs e)
        {
            Byte[] data;
            Int64 dat;

            //写指令
            if (listView1.Items[Index].SubItems[3].Text.ToLower() == "w")
            {
                dat = Convert.ToInt64(listView1.Items[Index].SubItems[2].Text.Trim());
                switch (Index)
                {
                    case 0:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X00, 0X00, 0X01, 0x00, 0x00 };
                        if (dat <= 0 || dat > 255)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 1:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X01, 0X00, 0X01, 0x00, 0x00 };
                        if (dat < 0 || dat > 8)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 2:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X02, 0X00, 0X01, 0x00, 0x00 };
                        if (dat != 1 && dat != 2)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        else if (dat == 2 && listView1.Items[3].SubItems[2].Text.Trim() != "0")
                        {
                            MessageBox.Show("停止位 2 时只支持 None 无校验 ");
                            return;
                        }
                        break;
                    case 3:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X03, 0X00, 0X01, 0x00, 0x00 };
                        if (dat < 0 || dat > 4)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 4:
                        data = new byte[] { 0X01, 0X10, 0X00, 0X10, 0X00, 0X02, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        if (dat == 0 || dat > 2147483647 || dat < -2147483648)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 5:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X12, 0X00, 0X01, 0x00, 0x00 };
                        break;
                    case 6:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X13, 0X00, 0X01, 0x00, 0x00 };
                        if (dat < 0 || dat > 10)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 7:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X14, 0X00, 0X01, 0x00, 0x00 };
                        if (dat < 0 || dat > 2)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 8:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X36, 0X00, 0X01, 0x00, 0x00 };
                        if (dat < 0 || dat > 3)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    case 9:
                        data = new byte[] { 0X01, 0X06, 0X00, 0X37, 0X00, 0X01, 0x00, 0x00 };
                        if (dat < 0 || dat > 3)
                        {
                            MessageBox.Show("数据输入超出范围，请重新输入");
                            return;
                        }
                        break;
                    default: return;
                }
                //设备地址
                data[0] = actXET.E_addr;

                //连续写指令
                if (Index == 4)
                {
                    MyDevice.myUIT.I = Convert.ToInt32(dat);
                    data[7] = MyDevice.myUIT.B3;
                    data[8] = MyDevice.myUIT.B2;
                    data[9] = MyDevice.myUIT.B1;
                    data[10] = MyDevice.myUIT.B0;
                    //actXET.AP_CRC16_MODBUS(data, 11).CopyTo(data, 11);
                    //MyDevice.protocol.mePort.Write(data, 0, 13);
                    lb_show.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " => " + GetBytesString(data, 0, 13, " "));
                }
                else
                {
                    MyDevice.myUIT.I = Convert.ToInt32(dat);
                    data[4] = MyDevice.myUIT.B1;
                    data[5] = MyDevice.myUIT.B0;
                    //actXET.AP_CRC16_MODBUS(data, 6).CopyTo(data, 6);
                    //MyDevice.protocol.mePort.Write(data, 0, 8);
                    lb_show.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " => " + GetBytesString(data, 0, 8, " "));
                    if (Index == 0)
                    {
                        actXET.E_addr = (byte)dat;
                    }
                }
            }
            //读指令
            else if (listView1.Items[Index].SubItems[3].Text.ToLower() == "r")
            {
                switch (Index)
                {
                    case 0: data = new byte[] { 0X01, 0X03, 0X00, 0X00, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 1: data = new byte[] { 0X01, 0X03, 0X00, 0X01, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 2: data = new byte[] { 0X01, 0X03, 0X00, 0X02, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 3: data = new byte[] { 0X01, 0X03, 0X00, 0X03, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 4: data = new byte[] { 0X01, 0X03, 0X00, 0X10, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 5: data = new byte[] { 0X01, 0X03, 0X00, 0X12, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 6: data = new byte[] { 0X01, 0X03, 0X00, 0X13, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 7: data = new byte[] { 0X01, 0X03, 0X00, 0X14, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 8: data = new byte[] { 0X01, 0X03, 0X00, 0X36, 0X00, 0X01, 0x00, 0x00 }; break;
                    case 9: data = new byte[] { 0X01, 0X03, 0X00, 0X37, 0X00, 0X01, 0x00, 0x00 }; break;
                    default: return;
                }
                //设备地址
                data[0] = actXET.E_addr;
                //actXET.AP_CRC16_MODBUS(data, 6).CopyTo(data, 6);
                //MyDevice.protocol.mePort.Write(data, 0, 8);
                lb_show.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " => " + GetBytesString(data, 0, 8, " "));
            }
            else
            {
                MessageBox.Show("读写输入有误，请输入R/W");
            }
        }

        //自定义输出字符数组方法（更简洁）
        static string GetBytesString(byte[] bytes, int index, int count, string sep)
        {
            return String.Join(sep, bytes.Skip(index).Take(count).Select(b => b.ToString("X2")));
        }

        #region 列表处理

        //初始化表格
        private void ShowListView()
        {
            listView1.GridLines = true;//表格是否显示网格线
            listView1.FullRowSelect = true;//是否选中整行

            listView1.View = View.Details;//设置显示方式
            listView1.Scrollable = true;//是否自动显示滚动条
            listView1.MultiSelect = false;//是否可以选择多行

            //添加表头（列）
            listView1.Columns.Add("address", "内部地址");
            listView1.Columns.Add("addressName", "地址名称");
            listView1.Columns.Add("about", "参数");
            listView1.Columns.Add("readOrWrite", "读写");
            listView1.Columns.Add("meaning", "数据范围/含义");

            //初始化表格内容
            information.Add("0" + "\t设备地址\t" + actXET.E_addr + "\tW\t" + "范围为1~255（0x01~0xFF，注意不要使用0x00）");
            information.Add("1" + "\t通讯波特率\t" + actXET.E_baud + "\tW\t" + "范围为0~8（0x00 = 1200，0x01 = 2400，\r\n0x02 = 4800，0x03 = 9600，\r\n0x04 = 14400，0x05 = 19200，\r\n0x06 = 38400，0x07 = 57600，\r\n0x08 = 115200）");
            information.Add("2" + "\t通讯停止位\t" + actXET.E_stopbit + "\tW\t" + "1，2（只支持None无校验）");
            information.Add("3" + "\t通讯校验位\t" + actXET.E_parity + "\tW\t" + "0x00 = None 无校验 \r\n0x01 = Odd 奇校验 \r\n0x02 = Even 偶校验 \r\n0x03 = Mark 固定 1 校验 \r\n0x04 = Space 固定 0 校验");
            information.Add("10" + "\t数字量满点\t" + actXET.T_wt_full + "\tW\t" + "标定量程对应的数字量满点");
            information.Add("12" + "\t数字量小数点\t" + actXET.E_wt_decimal + "\tW\t" + "表示当前数字量小数位数（读电压值固定 4 位小数）");
            information.Add("13" + "\t数字量单位\t" + actXET.E_wt_unit + "\tW\t" + "0 = 无单位；1 = kg；2 = lb；3 = oz；\r\n4 = g；5 = mg；6 = t；7 = ct");
            information.Add("14" + "\t连续发送格式\t" + actXET.E_wt_ascii + "\tW\t" + "0 = 关闭连续发送 1 = 连续发送毛重\r\n 2 = 连续发送净重3 = 可定制其它连续发送");
            information.Add("36" + "\t滤波深度\t" + actXET.E_wt_spfilt + "\tW\t" + "范围为0~3");
            information.Add("37" + "\t稳定次数\t" + actXET.E_wt_sptime + "\tW\t" + "范围为0~3");

            //添加表格内容
            foreach (string row in information)
            {
                if (row == null) continue;
                ListViewItem lvi = new ListViewItem(row.Split('\t'));
                listView1.Items.Add(lvi);
            }

            //listView1.Sorting = SortOrder.Ascending;
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            int span = 40;
            foreach (ColumnHeader ch in listView1.Columns)
            {
                ch.Width += span;
            }
        }

        //双击列表
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                item = this.listView1.GetItemAt(e.X, e.Y);
                //找到文本框
                Rectangle rect = item.GetBounds(ItemBoundsPortion.Entire);
                int StartX = rect.Left;//获取文本框位置的X坐标
                ColumnIndex = 0;//文本框的索引
                //获取列的索引
                //得到滑块的位置
                int pos = GetScrollPos(this.listView1.Handle.ToInt32(), 0);
                foreach (ColumnHeader Column in listView1.Columns)
                {
                    if (e.X + pos >= StartX + Column.Width)
                    {
                        StartX += Column.Width;
                        ColumnIndex += 1;
                    }
                }

                if (ColumnIndex != 2 && ColumnIndex != 3)//第一列为序号，不修改。如果双击为第一列则不可以进入修改
                {
                    return;
                }

                this.txtInput = new TextBox();

                //locate the txtinput and hide it. txtInput为TextBox
                this.txtInput.Parent = this.listView1;

                //begin edit
                if (item != null)
                {
                    rect.X = StartX;
                    rect.Width = this.listView1.Columns[ColumnIndex].Width;//得到长度和ListView的列的长度相同
                    this.txtInput.Bounds = rect;
                    this.txtInput.Multiline = true;
                    //显示文本框
                    this.txtInput.Text = item.SubItems[ColumnIndex].Text;
                    this.txtInput.Tag = item.SubItems[ColumnIndex];
                    this.txtInput.KeyPress += new KeyPressEventHandler(txtInput_KeyPress);
                    this.txtInput.Focus();
                }
            }
            catch
            {

            }
        }

        //添加回车保存内容
        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if ((int)e.KeyChar == 13)
                {
                    if (this.txtInput != null)
                    {
                        ListViewItem.ListViewSubItem lvst = (ListViewItem.ListViewSubItem)this.txtInput.Tag;
                        check(lvst);
                        lvst.Text = this.txtInput.Text;
                        this.txtInput.Dispose();
                    }
                }
            }
            catch
            {

            }
        }

        private void check(ListViewItem.ListViewSubItem lvst)
        {
            if (ColumnIndex == 2)
            {
                if (item.Text == "12")
                {
                    double num = 2147483647 / (Convert.ToInt32(listView1.Items[4].SubItems[2].Text.ToString()));
                    int max = (int)Math.Log10(num);
                    if (Convert.ToInt32(this.txtInput.Text) > max)
                    {
                        this.txtInput.Text = max.ToString();
                        MessageBox.Show("数字量小数点超过范围，最大只能选取到" + max);
                    }
                }
                else if (item.Text == "10")
                {
                    int num = Convert.ToInt32(listView1.Items[5].SubItems[2].Text.ToString());
                    int max = (int)(2147483647 / Math.Pow(10, num));
                    if (Convert.ToInt32(this.txtInput.Text) > max)
                    {
                        this.txtInput.Text = max.ToString();
                        MessageBox.Show("数字量满点超过范围，最大只能选取到" + max);
                    }
                }
                //}
                //else if (item.Text == "2")
                //{
                //    if (this.txtInput.Text == "2")
                //    {
                //        listView1.Items[3].SubItems[2].Text = "0";
                //    }
                //}
                //else if (item.Text == "3")
                //{
                //    if (this.txtInput.Text != "0")
                //    {
                //        listView1.Items[2].SubItems[2].Text = "1";
                //    }
                //}
            }
            else if (ColumnIndex == 3)
            {
                if (this.txtInput.Text.ToLower() != "w" && this.txtInput.Text.ToLower() != "r")
                {
                    this.txtInput.Text = lvst.Text;
                }
            }
        }

        //添加事件SelectedIndexChanged，释放文本框内容
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.txtInput != null)
                {
                    if (this.txtInput.Text.Length > 0)
                    {
                        ListViewItem.ListViewSubItem lvst = (ListViewItem.ListViewSubItem)this.txtInput.Tag;
                        lvst.Text = this.txtInput.Text;

                    }

                    this.txtInput.Dispose();
                }
            }
            catch
            {

            }
        }

        //单击列表
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {

            try
            {
                if (this.listView1.SelectedItems.Count > 0)//判断listview有被选中项
                {
                    Index = this.listView1.SelectedItems[0].Index;//取当前选中项的index,SelectedItems[0]这必须为0
                    btSend.Text = listView1.Items[Index].SubItems[1].Text;//用我们刚取到的index取被选中的某一列的值从0开始
                }

                if (this.txtInput != null)
                {
                    if (this.txtInput.Text.Length > 0)
                    {
                        ListViewItem.ListViewSubItem lvst = (ListViewItem.ListViewSubItem)this.txtInput.Tag;
                        lvst.Text = this.txtInput.Text;
                    }

                    this.txtInput.Dispose();
                }
            }
            catch
            {

            }
        }

        #endregion

        #region 界面更改

        private void MenuToolRS485Form_Load(object sender, EventArgs e)
        {
            //加载接收触发
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            //
            actXET = MyDevice.actDev;

            //初始化
            IsSave = false;

            //初始化表格
            ShowListView();
        }

        /// <summary>
        /// 关闭窗口时取消事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuToolRS485Form_FormClosing(object sender, FormClosingEventArgs e)
        {

            //取消串口事件
            MyDevice.myUpdate -= new freshHandler(update_FromUart);
        }

        //更新界面
        private void UpdateForm()
        {
            //更新列表(可去掉)
            listView1.Items[0].SubItems[2].Text = actXET.E_addr.ToString();
            listView1.Items[1].SubItems[2].Text = actXET.E_baud.ToString();
            listView1.Items[2].SubItems[2].Text = actXET.E_stopbit.ToString();
            listView1.Items[3].SubItems[2].Text = actXET.E_parity.ToString();
            listView1.Items[4].SubItems[2].Text = actXET.T_wt_full;
            listView1.Items[5].SubItems[2].Text = actXET.E_wt_decimal.ToString();
            listView1.Items[6].SubItems[2].Text = actXET.E_wt_unit.ToString();
            listView1.Items[7].SubItems[2].Text = actXET.E_wt_ascii.ToString();
            //listView1.Items[8].SubItems[2].Text = actXET.E_wt_spfilt.ToString();
            //listView1.Items[9].SubItems[2].Text = actXET.E_wt_sptime.ToString();

            //更新listbox
            //lb_show.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " <= " + GetBytesString(actXET.getRXD, 0, actXET.testCount, " "));
            lb_show.SelectedIndex = lb_show.Items.Count - 1;
            btSend.Focus();
        }

        //listBox绘制颜色
        private void lb_show_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                Brush mybsh = Brushes.Black;
                // 判断是什么类型的标签
                if (lb_show.Items[e.Index].ToString().Contains("<="))
                {
                    mybsh = Brushes.LightGreen;
                }
                else
                {
                    mybsh = Brushes.Black;
                }
                // 焦点框
                e.DrawFocusRectangle();
                //文本
                e.Graphics.DrawString(lb_show.Items[e.Index].ToString(), e.Font, mybsh, e.Bounds, StringFormat.GenericDefault);
            }
        }

        #endregion

    }
}
