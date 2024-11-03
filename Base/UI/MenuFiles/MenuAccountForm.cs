using Model;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Base.UI.MenuFile
{
    public partial class MenuAccountForm : Form
    {
        private Boolean isSave = false;
        private Boolean isNew = false;
        private String myUser = "user";
        private String myPSW = "";
        private String myDatPath = Application.StartupPath + @"\dat";
        private String myCfgPath = Application.StartupPath + @"\cfg";
        private String myLogPath = Application.StartupPath + @"\log";
        private String myFormText = "欢迎使用！";

        //
        #region set and get
        //

        public Boolean IsSave
        {
            get
            {
                return isSave;
            }
        }
        public Boolean IsNew
        {
            get
            {
                return isNew;
            }
        }
        public String MyUser
        {
            set
            {
                myUser = value;
            }
            get
            {
                return myUser;
            }
        }
        public String MyPSW
        {
            set
            {
                myPSW = value;
            }
            get
            {
                return myPSW;
            }
        }
        public String MyDatPath
        {
            set
            {
                myDatPath = value;
            }
            get
            {
                return myDatPath;
            }
        }
        public String MyCfgPath
        {
            set
            {
                myCfgPath = value;
            }
            get
            {
                return myCfgPath;
            }
        }
        public String MyLogPath
        {
            set
            {
                myLogPath = value;
            }
            get
            {
                return myLogPath;
            }
        }

        //
        #endregion
        //

        //
        public MenuAccountForm()
        {
            InitializeComponent();
        }

        //帐号登录加载
        private void MenuAccountForm_Load(object sender, EventArgs e)
        {
            //图标加载
            string logoIconPath = Path.Combine(Application.StartupPath, "pic", "logo.ico");
            if (File.Exists(logoIconPath))
            {
                this.Icon = new Icon(logoIconPath);
            }
            else
            {
                this.Icon = Properties.Resources.BCS16A;
            }

            //窗口标题加载
            load_FormText();

            //窗口元素调整
            if (this.Text == "欢迎使用！" || this.Text == "Welcome" || this.Text == myFormText)
            {
                button2.Visible = true;
                button3.Visible = false;
                label3.Visible = false;
                textBox2.Visible = false;
            }
            else
            {
                button2.Visible = false;
                button3.Visible = true;
                label3.Visible = false;
                textBox2.Visible = false;
            }

            //所有用户加载
            if (Directory.Exists(myDatPath))
            {
                //存在
                DirectoryInfo meDirectory = new DirectoryInfo(myDatPath);
                String meString;
                foreach (FileInfo meFiles in meDirectory.GetFiles("user.*.dat"))
                {
                    meString = meFiles.Name;
                    Regex regex = new Regex(@"user\.(.*?)\.dat");
                    Match match = regex.Match(meString);

                    if (match.Success)
                    {
                        string result = match.Groups[1].Value; // 获取"user."和".dat"之间的字符
                        comboBox1.Items.Add(result);
                    }
                }
            }
            else
            {
                //不存在则创建文件夹
                Directory.CreateDirectory(myDatPath);
                //不存在则创建文件
                myUser = "user";
                myPSW = "";
                myDatPath = Application.StartupPath + @"\dat";
                myCfgPath = Application.StartupPath + @"\cfg";
                myLogPath = Application.StartupPath + @"\log";
                isSave = true;
                isNew = true;
                //增加初始用户
                comboBox1.Items.Add("user");
            }

            //用户名加载
            if (myUser == "bohr" || myUser == "fac")
            {
                comboBox1.Text = "user";
            }
            else
            {
                comboBox1.Text = myUser;
            }
            textBox1.Text = "";
        }

        //退出账号登录
        private void MenuAccountForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //加载
            if (isSave)
            {
                MyDevice.D_username = MyUser;
                MyDevice.D_password = MyPSW;
                MyDevice.D_datPath = MyDatPath;
                MyDevice.D_cfgPath = MyCfgPath;
                MyDevice.D_logPath = MyLogPath;
            }

            //创建
            if (isNew)
            {
                MyDevice.SaveToDat();
            }

            isSave = false;
            isNew = false;
        }

        //登录创建保存按钮- 登录
        private void login_button1_Click()
        {
            //工厂使用超级账号密码
            if ((comboBox1.Text == "bohr") && (textBox1.Text == "bmc"))
            {
                myUser = "bohr";
                myPSW = "bmc";
                myDatPath = Application.StartupPath + @"\dat";
                myCfgPath = Application.StartupPath + @"\cfg";
                myLogPath = Application.StartupPath + @"\log";
                isSave = true;
                isNew = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            //客户超级账号密码
            else if ((comboBox1.Text == "fac") && (textBox1.Text == "woli"))
            {
                myUser = "fac";
                myPSW = "woli";
                myDatPath = Application.StartupPath + @"\dat";
                myCfgPath = Application.StartupPath + @"\cfg";
                myLogPath = Application.StartupPath + @"\log";
                isSave = true;
                isNew = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if ((comboBox1.Text == "fac2") && (textBox1.Text == "123456"))
            {
                myUser = "fac2";
                myPSW = "123456";
                myDatPath = Application.StartupPath + @"\dat";
                myCfgPath = Application.StartupPath + @"\cfg";
                myLogPath = Application.StartupPath + @"\log";
                isSave = true;
                isNew = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if ((comboBox1.Text == "admin") && (textBox1.Text == "123456"))
            {
                myUser = "admin";
                myPSW = "123456";
                myDatPath = Application.StartupPath + @"\dat";
                myCfgPath = Application.StartupPath + @"\cfg";
                myLogPath = Application.StartupPath + @"\log";
                isSave = true;
                isNew = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            //客户使用dat账号密码
            else
            {
                //用户文件
                String meString = myDatPath + @"\user." + comboBox1.Text + ".dat";

                //验证用户
                if (File.Exists(meString))
                {
                    //读取用户信息
                    FileStream meFS = new FileStream(meString, FileMode.Open, FileAccess.Read);
                    BinaryReader meRead = new BinaryReader(meFS);
                    if (meFS.Length > 0)
                    {
                        //有内容文件
                        myUser = meRead.ReadString();
                        myPSW = meRead.ReadString();
                        myDatPath = meRead.ReadString();
                        myDatPath = Application.StartupPath + @"\dat";
                        myCfgPath = meRead.ReadString();
                        myCfgPath = Application.StartupPath + @"\cfg";
                        myLogPath = meRead.ReadString();
                        myLogPath = Application.StartupPath + @"\log";
                        isNew = false;
                    }
                    else
                    {
                        //空文件
                        myUser = comboBox1.Text;
                        myPSW = "";
                        myDatPath = Application.StartupPath + @"\dat";
                        myCfgPath = Application.StartupPath + @"\cfg";
                        myLogPath = Application.StartupPath + @"\log";
                        isNew = true;
                    }
                    meRead.Close();
                    meFS.Close();

                    //验证密码
                    if (myPSW == textBox1.Text)
                    {
                        isSave = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        warning_NI("密码错误！");
                    }
                }
                else
                {
                    //不存在admin用户
                    if ((comboBox1.Text == "user") && (textBox1.Text == ""))
                    {
                        myUser = "user";
                        myPSW = "";
                        myDatPath = Application.StartupPath + @"\dat";
                        myCfgPath = Application.StartupPath + @"\cfg";
                        myLogPath = Application.StartupPath + @"\log";
                        isSave = true;
                        isNew = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    //不存在用户提示
                    else
                    {
                        warning_NI("不存在用户！");
                    }
                }
            }
        }

        //登录创建保存按钮- 创建
        private void create_button1_Click()
        {
            if (comboBox1.SelectedIndex < 0)//帐号验证
            {
                if (textBox1.Text == textBox2.Text)//密码验证
                {
                    myUser = comboBox1.Text;
                    myPSW = textBox1.Text;
                    myDatPath = Application.StartupPath + @"\dat";
                    myCfgPath = Application.StartupPath + @"\cfg";
                    myLogPath = Application.StartupPath + @"\log";
                    isSave = true;
                    isNew = true;

                    this.Close();
                }
                else
                {
                    warning_NI("密码错误！");
                }
            }
            else
            {
                warning_NI("已存在账号！");
            }
        }

        //登录创建保存按钮- 保存
        private void save_button1_Click()
        {
            if (comboBox1.Text == myUser)//帐号验证
            {
                if (textBox1.Text == myPSW)//密码验证
                {
                    myUser = comboBox1.Text;
                    myPSW = textBox2.Text;
                    myDatPath = Application.StartupPath + @"\dat";
                    myCfgPath = Application.StartupPath + @"\cfg";
                    myLogPath = Application.StartupPath + @"\log";
                    isSave = true;
                    isNew = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    warning_NI("密码验证错误！");
                }
            }
            else
            {
                warning_NI("账号错误！");
            }
        }

        //登录创建保存按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "登 录" || button1.Text == "Login")
            {
                login_button1_Click();
            }
            else if (button1.Text == "创 建" || button2.Text == "Sign up")
            {
                create_button1_Click();
            }
            else if (button1.Text == "保 存" || button3.Text == "Save")
            {
                save_button1_Click();
            }
        }

        //新建
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button3.Visible = false;
            label3.Visible = true;
            textBox2.Visible = true;
            if (MyDevice.languageType == 0)
            {
                button1.Text = "创 建";
            }
            else
            {

                button1.Text = "Sign up";
            }
        }

        //改密码
        private void button3_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button3.Visible = false;
            label3.Visible = true;
            textBox2.Visible = true;
            if (MyDevice.languageType == 0)
            {
                label2.Text = "新密码：";
                button1.Text = "保 存";
            }
            else
            {
                label2.Text = "new password：";
                button1.Text = "Save";
            }
        }

        //取消按钮
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.Text == "欢迎使用！" || this.Text == "Welcome" || this.Text == myFormText)
            {
                System.Environment.Exit(0);
            }
            else
            {
                isSave = false;
                isNew = false;
                this.Hide();
            }
        }

        //登陆
        private void login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                this.button1.Focus();
                button1_Click(sender, e);   //调用登录按钮的事件处理代码
            }
        }

        //时间控制
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            notifyIcon1.Visible = false;
        }

        //报警提示
        private void warning_NI(string meErr)
        {
            timer1.Enabled = true;
            timer1.Interval = 3000;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3000, notifyIcon1.Text, meErr, ToolTipIcon.Info);
            if (button3.Visible == true)
            {
                label4.Location = new Point(91, 105);
            }
            else
            {
                label4.Location = new Point(91, 144);
            }
            label4.Text = meErr;
            label4.Visible = true;
        }

        //读取窗口标题Text
        private void load_FormText()
        {
            try
            {
                //读取文件
                string logoIconPath = Path.Combine(Application.StartupPath, "pic", "FormText.ini");
                if (File.Exists(logoIconPath))
                {
                    String[] meLines = File.ReadAllLines(logoIconPath);
                    if (meLines.Length > 0)
                    {
                        myFormText = meLines[0].Split('=')[1];
                    }
                    else
                    {
                        myFormText = "欢迎使用！";
                    }
                }
                else
                {
                    myFormText = "欢迎使用！";
                }
            }
            catch
            {
                myFormText = "欢迎使用！";
            }

            //如果设置了自定义窗口标题Text
            if (myFormText != "欢迎使用！")
            {
                this.Text = myFormText;
            }
        }
    }
}
