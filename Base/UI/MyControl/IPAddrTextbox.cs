using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

namespace Base.UI.MyControl
{
    [Description("允许用户输入 IPV4 地址。")]
    public partial class IPAddrTextbox : UserControl
    {
        /// <summary>用于保存所有字段对象的数组</summary>
        private readonly TextBox[] fields;

        public IPAddrTextbox()
        {
            InitializeComponent();
            fields = new TextBox[] { field1, field2, field3, field4 };
        }

        /// <summary>
        /// 获取或设置以允许使用环回地址
        /// </summary>
        [Category("扩展属性"), Description("允许使用环回地址")]
        public bool EnableLoopbackAddr { get; set; } = true;
        /// <summary>
        /// 获取 IP 地址
        /// </summary>
        [Category("扩展属性"), Description("获取 IP 地址")]
        public IPAddress IPAddr
        {
            get
            {
                IPAddress.TryParse(IPAddrStr, out var address);
                return address;
            }
        }
        /// <summary>
        /// 获取 IP 地址字符串（若要设置，请调用 <see cref="SetIPAddrStr(string)"/>）
        /// </summary>
        [Category("扩展属性"), Description("获取 IP 地址字符串")]
        public string IPAddrStr
        {
            get
            {
                string ipAddr = string.Empty;
                foreach (var field in fields)
                {
                    int.TryParse(field.Text, out var value);
                    ipAddr += (value + label1.Text);
                }
                ipAddr = ipAddr.Substring(0, ipAddr.Length - 1);
                return ipAddr;
            }
        }

        /// <summary>在 <see cref="IPAddrStr"/> 属性更改后发生</summary>
        [Category("属性已更改"), Description("在控件上更改 IPAddrStr 属性的值时引发的事件。")]
        public event EventHandler IPAddrStrChanged;

        /// <summary>
        /// 设置 IP 地址字符串
        /// </summary>
        /// <remarks>
        /// 不使用 <see cref="IPAddrStr"/> 的 <c>set</c> 访问器主要是出于默认情况下四个字段应为空文本的考虑
        /// </remarks>
        /// <param name="ipString">要设置的字符串</param>
        public void SetIPAddrStr(string ipString)
        {
            // 检查是否为有效 IPV4 地址
            if (IPAddress.TryParse(ipString, out IPAddress address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var parts = address.ToString().Split('.');
                for (int i = 0; i < parts.Length; i++)
                {
                    fields[i].Text = parts[i];
                }
            }
            // 对所有字段的数据进行验证
            this.ValidateChildren();
        }

        #region 移动焦点相关方法
        /// <summary>
        /// 将焦点移动至上一个字段
        /// </summary>
        /// <param name="index">当前字段索引，允许范围 1~3</param>
        private void FocusPrevious(int index)
        {
            if (index >= 1 && index <= 3)
            {
                index--;
                fields[index].Focus();
                fields[index].Select(fields[index].TextLength, 0);
            }
        }

        /// <summary>
        /// 将焦点移动至下一个字段
        /// </summary>
        /// <param name="index">当前字段索引，允许范围 0~2</param>
        private void FocusNext(int index)
        {
            if (index >= 0 && index <= 2)
            {
                index++;
                fields[index].Focus();
                fields[index].Select();
            }
        }

        /// <summary>
        /// 将焦点移动至下一个字段并选定所有文本
        /// </summary>
        /// <param name="index">当前字段索引，允许范围 0~2</param>
        private void FocusNextAndSelectAll(int index)
        {
            if (index >= 0 && index <= 2)
            {
                index++;
                fields[index].Focus();
                fields[index].SelectAll();
            }
        }
        #endregion

        #region 字段一事件处理函数
        private void field1_KeyDown(object sender, KeyEventArgs e)
        {
            // 右箭头和下箭头可将焦点移动至下一个字段
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                if (field1.SelectionStart == field1.TextLength)
                {
                    FocusNext(0);
                }
            }
            // 同时两个句点键可将焦点移动至下一个字段
            else if ((e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod) && field1.SelectionStart != 0)
            {
                FocusNextAndSelectAll(0);
            }
        }

        private void field1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许数字和控制字符
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // 已经输入了三个数字了还要继续输入的话则将焦点移动至下个字段
            else if (char.IsDigit(e.KeyChar) && field1.SelectionStart == field1.MaxLength)
            {
                e.Handled = true;
                FocusNextAndSelectAll(0);
            }
        }

        private void field1_TextChanged(object sender, EventArgs e)
        {
            // 任何文本更改都检查字段的文本是否能解析为整数，以避免出现不期望的字符
            if (!int.TryParse(field1.Text, out int value) && field1.Text != string.Empty)
            {
                field1.Text = string.Empty;
            }
            // 输入满三个数字移动焦点至下一个字段
            if (field1.TextLength == field1.MaxLength && field1.SelectionStart == field1.MaxLength)
            {
                FocusNextAndSelectAll(0);
            }
            IPAddrStrChanged?.Invoke(this, EventArgs.Empty);
        }

        //只校验ip
        private void field1_Validating(object sender, CancelEventArgs e)
        {
            if (field1.Text != string.Empty)
            {
                // 验证字段的值是否在有效范围内
                int.TryParse(field1.Text, out int value);
                if (value < 1)
                {
                    field1.Text = "1";
                    field1.Select(0, 0);
                    e.Cancel = true;        //取消验证事件
                    MessageBox.Show($"{value} 不是有效项。请指定一个介于 1 和 223 间的值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (value > 223)
                {
                    field1.Text = "223";
                    field1.Select(0, 0);
                    e.Cancel = true;        //取消验证事件
                    MessageBox.Show($"{value} 不是有效项。请指定一个介于 1 和 223 间的值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (value == 127 && !EnableLoopbackAddr)
                {
                    field1.Text = "1";
                    field1.Select(0, 0);
                    e.Cancel = true;
                    MessageBox.Show($"以 {value} 开头的 IP 地址无效，因为这些地址是为环回地址保留的。请指定介于 1 和 223 间的其他某个有效值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        //可校验子网掩码
        private void field1_Validating2(object sender, CancelEventArgs e)
        {
            if (field1.Text != string.Empty)
            {
                // 验证字段的值是否在有效范围内
                int.TryParse(field2.Text, out int value);
                if (value > 255)
                {
                    field2.Text = "255";
                    field2.Select(0, 0);
                    e.Cancel = true;        //取消验证事件
                    MessageBox.Show($"{value} 不是有效项。请指定一个介于 0 和 255 间的值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region 字段二事件处理函数
        private void field2_KeyDown(object sender, KeyEventArgs e)
        {
            // 左箭头和上箭头可将焦点移动至上一个字段
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                if (field2.SelectionStart + field2.SelectionLength == 0)     //获取光标的位置，无论是否有选中的文本
                {
                    FocusPrevious(1);
                }
            }
            // 右箭头和下箭头可将焦点移动至下一个字段
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                if (field2.SelectionStart == field2.TextLength)
                {
                    FocusNext(1);
                }
            }
            // 同时两个句点键可将焦点移动至下一个字段
            else if ((e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod) && field2.SelectionStart != 0)
            {
                FocusNextAndSelectAll(1);
            }
        }

        private void field2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许数字和控制字符
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // 已经输入了三个数字了还要继续输入的话则将焦点移动至下个字段
            else if (char.IsDigit(e.KeyChar) && field2.SelectionStart == field2.MaxLength)
            {
                e.Handled = true;
                FocusNextAndSelectAll(1);
            }
            // 如果在字段的开头按下了退格键，则前往上一个字段继续执行删除
            else if (e.KeyChar == '\b' && field2.SelectionStart == 0 && field2.SelectionLength == 0)
            {
                FocusPrevious(1);
                // SendKeys.Send() 方法会将键盘消息发送到具有焦点的控件，所以首先要将焦点移动至上一个字段
                SendKeys.Send("{BACKSPACE}");
            }
        }

        private void field2_TextChanged(object sender, EventArgs e)
        {
            // 任何文本更改都检查字段的文本是否能解析为整数，以避免出现不期望的字符
            if (!int.TryParse(field2.Text, out int value) && field2.Text != string.Empty)
            {
                field2.Text = string.Empty;
            }
            // 输入满三个数字移动焦点至下一个字段
            if (field2.TextLength == field2.MaxLength && field2.SelectionStart == field2.MaxLength)
            {
                FocusNextAndSelectAll(1);
            }
            IPAddrStrChanged?.Invoke(this, EventArgs.Empty);
        }

        private void field2_Validating(object sender, CancelEventArgs e)
        {
            // 验证字段的值是否在有效范围内
            int.TryParse(field2.Text, out int value);
            if (value > 255)
            {
                field2.Text = "255";
                field2.Select(0, 0);
                e.Cancel = true;        //取消验证事件
                MessageBox.Show($"{value} 不是有效项。请指定一个介于 0 和 255 间的值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 字段三事件处理函数
        private void field3_KeyDown(object sender, KeyEventArgs e)
        {
            // 左箭头和上箭头可将焦点移动至上一个字段
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                if (field3.SelectionStart + field3.SelectionLength == 0)     //获取光标的位置，无论是否有选中的文本
                {
                    FocusPrevious(2);
                }
            }
            // 右箭头和下箭头可将焦点移动至下一个字段
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                if (field3.SelectionStart == field3.TextLength)
                {
                    FocusNext(2);
                }
            }
            // 同时两个句点键可将焦点移动至下一个字段
            else if ((e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod) && field3.SelectionStart != 0)
            {
                FocusNextAndSelectAll(2);
            }
        }

        private void field3_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许数字和控制字符
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // 已经输入了三个数字了还要继续输入的话则将焦点移动至下个字段
            else if (char.IsDigit(e.KeyChar) && field3.SelectionStart == field3.MaxLength)
            {
                e.Handled = true;
                FocusNextAndSelectAll(2);
            }
            // 如果在字段的开头按下了退格键，则前往上一个字段继续执行删除
            else if (e.KeyChar == '\b' && field3.SelectionStart == 0 && field3.SelectionLength == 0)
            {
                FocusPrevious(2);
                // SendKeys.Send() 方法会将键盘消息发送到具有焦点的控件，所以首先要将焦点移动至上一个字段
                SendKeys.Send("{BACKSPACE}");
            }
        }

        private void field3_TextChanged(object sender, EventArgs e)
        {
            // 任何文本更改都检查字段的文本是否能解析为整数，以避免出现不期望的字符
            if (!int.TryParse(field3.Text, out int value) && field3.Text != string.Empty)
            {
                field3.Text = string.Empty;
            }
            // 输入满三个数字移动焦点至下一个字段
            if (field3.TextLength == field3.MaxLength && field3.SelectionStart == field3.MaxLength)
            {
                FocusNextAndSelectAll(2);
            }
            IPAddrStrChanged?.Invoke(this, EventArgs.Empty);
        }

        private void field3_Validating(object sender, CancelEventArgs e)
        {
            // 验证字段的值是否在有效范围内
            int.TryParse(field3.Text, out int value);
            if (value > 255)
            {
                field3.Text = "255";
                field3.Select(0, 0);
                e.Cancel = true;        //取消验证事件
                MessageBox.Show($"{value} 不是有效项。请指定一个介于 0 和 255 间的值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 字段四事件处理函数
        private void field4_KeyDown(object sender, KeyEventArgs e)
        {
            // 左箭头和上箭头可将焦点移动至上一个字段
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                if (field4.SelectionStart + field4.SelectionLength == 0)     //获取光标的位置，无论是否有选中的文本
                {
                    FocusPrevious(3);
                }
            }
        }

        private void field4_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许数字和控制字符
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // 已经输入了三个数字了还要继续输入的话则移动焦点(以触发验证事件)
            else if (char.IsDigit(e.KeyChar) && field4.SelectionStart == field4.MaxLength)
            {
                e.Handled = true;
                label3.Focus();
                field4.Focus();
            }
            // 如果在字段的开头按下了退格键，则前往上一个字段继续执行删除
            else if (e.KeyChar == '\b' && field4.SelectionStart == 0 && field4.SelectionLength == 0)
            {
                FocusPrevious(3);
                // SendKeys.Send() 方法会将键盘消息发送到具有焦点的控件，所以首先要将焦点移动至上一个字段
                SendKeys.Send("{BACKSPACE}");
            }
        }

        private void field4_TextChanged(object sender, EventArgs e)
        {
            // 任何文本更改都检查字段的文本是否能解析为整数，以避免出现不期望的字符
            if (!int.TryParse(field4.Text, out int value) && field4.Text != string.Empty)
            {
                field4.Text = string.Empty;
            }
            // 输入满三个数字移动焦点(以触发验证事件)
            if (field4.TextLength == field4.MaxLength && field4.SelectionStart == field4.MaxLength)
            {
                label3.Focus();
                field4.Focus();
            }
            IPAddrStrChanged?.Invoke(this, EventArgs.Empty);
        }

        private void field4_Validating(object sender, CancelEventArgs e)
        {
            // 验证字段的值是否在有效范围内
            int.TryParse(field4.Text, out int value);
            if (value > 255)
            {
                field4.Text = "255";
                field4.Select(0, 0);
                e.Cancel = true;        //取消验证事件
                MessageBox.Show($"{value} 不是有效项。请指定一个介于 0 和 255 间的值。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 复制与粘贴相关
        /// <summary>
        /// 重写 <see href="https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.forms.control.processcmdkey?view=netframework-4.8">ProcessCmdKey()</see> 方法以处理 Ctrl+C 和 Ctrl+V 命令。实现在任意字段内可直接复制或粘贴 IP 地址字符串
        /// </summary>
        /// <remarks>
        /// 请注意，ProcessCmdKey() 主要用于处理键盘消息。如果您想处理所有的 Windows 消息（包括键盘、鼠标和其他设备的消息），请重写 <see href="https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.forms.control.wndproc?view=netframework-4.8">WndProc()</see> 方法。
        /// </remarks>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 检查是否按下了 Ctrl+C
            if (keyData == (Keys.Control | Keys.C))
            {
                // 获取当前拥有焦点的控件
                Control focusedControl = this.ActiveControl;

                // 检查是否是 TextBox 拥有焦点
                if (focusedControl is TextBox)
                {
                    TextBox focusedTextBox = focusedControl as TextBox;

                    // 检查是否有文本被选中
                    if (string.IsNullOrEmpty(focusedTextBox.SelectedText))
                    {
                        // 如果没有文本被选中，则将 IP 地址字符串复制到剪贴板
                        Clipboard.SetText(IPAddrStr);
                        // 返回 true 表示已经处理了这个命令
                        return true;
                    }
                }
            }
            // 检查是否按下了 Ctrl+V
            else if (keyData == (Keys.Control | Keys.V))
            {
                // 获取剪贴板中的内容
                var str = Clipboard.GetText();

                // 检查剪贴板的内容是否为无符号整数
                if (uint.TryParse(str, out uint value))
                {
                    // 返回 false 表示未处理这个命令（将由字段自行处理命令）
                    return false;
                }
                // 检查剪贴板的内容是否为有效 IPV4 地址，如果是则设置 IP 地址字符串
                SetIPAddrStr(str);

                // 返回 true 表示已经处理了这个命令
                return true;
            }

            // 如果没有处理命令，调用基类的 ProcessCmdKey 方法
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}
