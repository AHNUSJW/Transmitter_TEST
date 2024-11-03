using Model;
using System;
using System.Drawing;
using System.Windows.Forms;

//Alvin 20230414
//Junzhe 20230926

//注意问题
//发送指令的状态切换后,但是设备发送数据还没有切换,串口接收到的数据仍然是以前的类型
//需要根据数据标志位和数值变量来识别是什么数据

namespace Base.UI.MenuSet
{
    public partial class MenuSetFilterForm : Form
    {
        private XET actXET;//需要操作的设备

        private TASKS nextTask;//按键指令

        public MenuSetFilterForm()
        {
            InitializeComponent();
        }

        private void MenuSetFilterForm_Load(object sender, EventArgs e)
        {
            MyDevice.myUpdate += new freshHandler(update_FromUart);

            actXET = MyDevice.actDev;
            MyDevice.mePort_StopDacout();//解决接收=123/r/n时还进Protocol_mePort_ReceiveDacout软件闪退

            nextTask = TASKS.ADC;
            MyDevice.mePort_SendCOM(TASKS.ADC);//设备连续发送adcout
        }

        private void MenuSetFilterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDevice.myUpdate -= new freshHandler(update_FromUart);

            MyDevice.mePort_StopDacout();
        }

        private void setFilterOrder_Click(object sender, EventArgs e)
        {
            setFilterOrder.HoverBackColor = Color.Firebrick;
            Refresh();
            nextTask = TASKS.SFLT;
            MyDevice.mePort_SendCOM(TASKS.SFLT);//设置滤波范围
        }

        private void resetFilterOreder_Click(object sender, EventArgs e)
        {
            resetFilterOreder.HoverBackColor = Color.Firebrick;
            Refresh();
            nextTask = TASKS.RFLT;
            MyDevice.mePort_SendCOM(TASKS.RFLT);//重置滤波范围
        }

        private void lbAdcoutClear_Click(object sender, EventArgs e)
        {
            lbShowADC.Items.Clear();
            setFilterOrder.HoverBackColor = buttonX2.HoverBackColor;
            Refresh();
        }

        private void lbFilterClear_Click(object sender, EventArgs e)
        {
            lbShowFilter.Items.Clear();
            resetFilterOreder.HoverBackColor = buttonX3.HoverBackColor;
            Refresh();
        }

        private void lbAdcoutCopy_Click(object sender, EventArgs e)
        {
            //复制到剪贴板
            if (lbShowADC.Items.Count > 0)
            {
                String copyText = "";

                for (int i = 0; i < lbShowADC.Items.Count; i++)
                {
                    copyText = copyText + lbShowADC.Items[i].ToString() + Environment.NewLine;
                }

                Clipboard.SetText(copyText);
            }
        }

        public void update_FromUart()
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
                //
                switch (nextTask)
                {
                    case TASKS.ADC:
                        lbShowADC.Items.Add(actXET.R_adcout.ToString());
                        lbShowADC.SelectedIndex = lbShowADC.Items.Count - 1;
                        break;

                    case TASKS.SFLT:
                        if (MyDevice.protocol.rxData == (int)TASKS.SFLT)//都用rxDat = (int)TASKS.SFLT标志
                        {
                            lbShowFilter.Items.Add(actXET.E_filter.ToString());
                            lbShowFilter.SelectedIndex = lbShowFilter.Items.Count - 1;
                            setFilterOrder.HoverBackColor = Color.Green;
                            Refresh();

                            nextTask = TASKS.ADC;
                            MyDevice.mePort_SendCOM(TASKS.ADC);//转连续发送adcout
                        }
                        else
                        {
                            MyDevice.mePort_SendCOM(TASKS.SFLT);//重发指令
                        }
                        break;

                    case TASKS.RFLT:
                        if (MyDevice.protocol.rxData == (int)TASKS.SFLT)//都用rxDat = (int)TASKS.SFLT标志
                        {
                            lbShowFilter.Items.Add(actXET.E_filter.ToString());
                            lbShowFilter.SelectedIndex = lbShowFilter.Items.Count - 1;
                            resetFilterOreder.HoverBackColor = Color.Green;
                            Refresh();

                            nextTask = TASKS.ADC;
                            MyDevice.mePort_SendCOM(TASKS.ADC);//转连续发送adcout
                        }
                        else
                        {
                            MyDevice.mePort_SendCOM(TASKS.RFLT);//重发指令
                        }
                        break;
                }
            }
        }
    }
}
