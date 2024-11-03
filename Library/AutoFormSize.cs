using System;
using System.Drawing;
using System.Windows.Forms;

//Ricardo 20231109

namespace Library
{
    public class AutoFormSize
    {
        public float x; //定义当前窗体的宽度
        public float y; //定义当前窗体的高度

        public void setTag(Control cons)
        {

            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                if (con.Controls.Count > 0) setTag(con);
            }
        }

        public void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
                //获取控件的Tag属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    var mytag = con.Tag.ToString().Split(';');
                    //根据窗体缩放的比例确定控件的值
                    con.Width = Convert.ToInt32(Convert.ToSingle(mytag[0]) * newx); //宽度
                    con.Height = Convert.ToInt32(Convert.ToSingle(mytag[1]) * newy); //高度
                    con.Left = Convert.ToInt32(Convert.ToSingle(mytag[2]) * newx); //左边距
                    con.Top = Convert.ToInt32(Convert.ToSingle(mytag[3]) * newy); //顶边距
                    var currentSize = Convert.ToSingle(mytag[4]) * newy; //字体大小                   
                    if (currentSize > 0) con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    con.Focus();
                    if (con.Controls.Count > 0) setControls(newx, newy, con);
                }
        }

        //用于宽变短但高不变导致字体大小不变时的情形
        public void setControlsType2(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
                //获取控件的Tag属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    var mytag = con.Tag.ToString().Split(';');
                    //根据窗体缩放的比例确定控件的值
                    con.Width = Convert.ToInt32(Convert.ToSingle(mytag[0]) * newx); //宽度
                    con.Height = Convert.ToInt32(Convert.ToSingle(mytag[1]) * newy); //高度
                    con.Left = Convert.ToInt32(Convert.ToSingle(mytag[2]) * newx); //左边距
                    con.Top = Convert.ToInt32(Convert.ToSingle(mytag[3]) * newy); //顶边距
                    var currentSize = Convert.ToSingle(mytag[4]) * newx; //字体大小                   
                    if (currentSize > 0) con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    con.Focus();
                    if (con.Controls.Count > 0) setControlsType2(newx, newy, con);
                }
        }

        public void ReWinformLayout(Form form, int type)
        {
            var newx = form.Width / x;
            var newy = form.Height / y;
            switch (type)
            {
                default:
                case 1:
                    setControls(newx, newy, form);
                    break;
                case 2:
                    setControlsType2(newx, newy, form);
                    break;
            }
        }

        public void ReWinformLayout(UserControl control, int type)
        {
            var newx = control.Width / x;
            var newy = control.Height / y;
            switch (type)
            {
                default:
                case 1:
                    setControls(newx, newy, control);
                    break;
                case 2:
                    setControlsType2(newx, newy, control);
                    break;
            }
        }

        public void UIComponetForm(Form form)
        {
            x = form.Width;
            y = form.Height;
            setTag(form);
        }

        public void UIComponetForm(UserControl control)
        {
            x = control.Width;
            y = control.Height;
            setTag(control);
        }

        public void UIComponetForm_Resize(Form form, int type = 1)
        {
            //重置窗口布局
            ReWinformLayout(form, type);
        }

        public void UIComponetForm_Resize(UserControl control, int type = 1)
        {
            //重置窗口布局
            ReWinformLayout(control, type);
        }
    }
}
