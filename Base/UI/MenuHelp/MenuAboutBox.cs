using System;
using System.Windows.Forms;

//Ricardo 20230616
//Lumi 20240906

namespace Base.UI.MenuHelp
{
    public partial class MenuAboutBox : Form
    {
        public MenuAboutBox()
        {
            InitializeComponent();
        }

        private void MenuAboutBox_Load(object sender, EventArgs e)
        {
            Main main = new Main();
            labelProductName.Text = "Professional " + main.Text;
        }

        //打开软件激活窗口
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (Form form in this.MdiChildren)
            {
                if (form.GetType().Name == "menuHelpLicenseForm")
                {
                    form.BringToFront();
                    return;
                }
                else
                {
                    form.Close();
                }
            }

            Main.LicenseForm.StartPosition = FormStartPosition.CenterScreen;
            Main.LicenseForm.MaximizeBox = false;
            Main.LicenseForm.ShowDialog();
        }
    }
}
