namespace Base.UI.MenuSetCommunication
{
    partial class MenuParaWirelessForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new DTiws.View.ButtonX();
            this.label13 = new System.Windows.Forms.Label();
            this.tb_ADDL = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_rfRate = new HZH_Controls.Controls.UCCombox();
            this.cb_rfBaud = new HZH_Controls.Controls.UCCombox();
            this.cb_rfCheck = new HZH_Controls.Controls.UCCombox();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_rfChan = new HZH_Controls.Controls.UCCombox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.tb_ADDL);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cb_rfRate);
            this.groupBox2.Controls.Add(this.cb_rfBaud);
            this.groupBox2.Controls.Add(this.cb_rfCheck);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cb_rfChan);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(655, 295);
            this.groupBox2.TabIndex = 38;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "参数配置、写入";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 15.75F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(511, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 28);
            this.label1.TabIndex = 129;
            this.label1.Text = "(1-255)";
            this.label1.Visible = false;
            // 
            // button1
            // 
            this.button1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button1.EnterForeColor = System.Drawing.Color.White;
            this.button1.Font = new System.Drawing.Font("宋体", 12F);
            this.button1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button1.HoverForeColor = System.Drawing.Color.White;
            this.button1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button1.Location = new System.Drawing.Point(475, 227);
            this.button1.Name = "button1";
            this.button1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button1.PressForeColor = System.Drawing.Color.White;
            this.button1.Radius = 6;
            this.button1.Size = new System.Drawing.Size(139, 38);
            this.button1.TabIndex = 128;
            this.button1.Text = "确 定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(44, 242);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(29, 12);
            this.label13.TabIndex = 127;
            this.label13.Text = "提示";
            // 
            // tb_ADDL
            // 
            this.tb_ADDL.BackColor = System.Drawing.Color.White;
            this.tb_ADDL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_ADDL.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.tb_ADDL.Location = new System.Drawing.Point(448, 91);
            this.tb_ADDL.Name = "tb_ADDL";
            this.tb_ADDL.Size = new System.Drawing.Size(58, 33);
            this.tb_ADDL.TabIndex = 33;
            this.tb_ADDL.Visible = false;
            this.tb_ADDL.Leave += new System.EventHandler(this.textBox1_ADDL_Leave);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 15.75F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(368, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 28);
            this.label4.TabIndex = 32;
            this.label4.Text = "地址:";
            this.label4.Visible = false;
            // 
            // cb_rfRate
            // 
            this.cb_rfRate.BackColor = System.Drawing.Color.White;
            this.cb_rfRate.BackColorExt = System.Drawing.Color.White;
            this.cb_rfRate.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_rfRate.ConerRadius = 5;
            this.cb_rfRate.DropPanelHeight = -1;
            this.cb_rfRate.FillColor = System.Drawing.Color.White;
            this.cb_rfRate.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.cb_rfRate.IsRadius = true;
            this.cb_rfRate.IsShowRect = true;
            this.cb_rfRate.ItemWidth = 70;
            this.cb_rfRate.Location = new System.Drawing.Point(143, 96);
            this.cb_rfRate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb_rfRate.Name = "cb_rfRate";
            this.cb_rfRate.RectColor = System.Drawing.Color.White;
            this.cb_rfRate.RectWidth = 1;
            this.cb_rfRate.SelectedIndex = -1;
            this.cb_rfRate.SelectedValue = "";
            this.cb_rfRate.Size = new System.Drawing.Size(139, 32);
            this.cb_rfRate.Source = null;
            this.cb_rfRate.TabIndex = 31;
            this.cb_rfRate.TextValue = null;
            this.cb_rfRate.TriangleColor = System.Drawing.Color.LightSteelBlue;
            this.cb_rfRate.Visible = false;
            // 
            // cb_rfBaud
            // 
            this.cb_rfBaud.BackColor = System.Drawing.Color.White;
            this.cb_rfBaud.BackColorExt = System.Drawing.Color.White;
            this.cb_rfBaud.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_rfBaud.ConerRadius = 5;
            this.cb_rfBaud.DropPanelHeight = -1;
            this.cb_rfBaud.FillColor = System.Drawing.Color.White;
            this.cb_rfBaud.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.cb_rfBaud.IsRadius = true;
            this.cb_rfBaud.IsShowRect = true;
            this.cb_rfBaud.ItemWidth = 70;
            this.cb_rfBaud.Location = new System.Drawing.Point(475, 156);
            this.cb_rfBaud.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb_rfBaud.Name = "cb_rfBaud";
            this.cb_rfBaud.RectColor = System.Drawing.Color.White;
            this.cb_rfBaud.RectWidth = 1;
            this.cb_rfBaud.SelectedIndex = -1;
            this.cb_rfBaud.SelectedValue = "";
            this.cb_rfBaud.Size = new System.Drawing.Size(139, 32);
            this.cb_rfBaud.Source = null;
            this.cb_rfBaud.TabIndex = 25;
            this.cb_rfBaud.TextValue = null;
            this.cb_rfBaud.TriangleColor = System.Drawing.Color.LightSteelBlue;
            this.cb_rfBaud.Visible = false;
            // 
            // cb_rfCheck
            // 
            this.cb_rfCheck.BackColor = System.Drawing.Color.White;
            this.cb_rfCheck.BackColorExt = System.Drawing.Color.White;
            this.cb_rfCheck.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_rfCheck.ConerRadius = 5;
            this.cb_rfCheck.DropPanelHeight = -1;
            this.cb_rfCheck.FillColor = System.Drawing.Color.White;
            this.cb_rfCheck.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.cb_rfCheck.IsRadius = true;
            this.cb_rfCheck.IsShowRect = true;
            this.cb_rfCheck.ItemWidth = 70;
            this.cb_rfCheck.Location = new System.Drawing.Point(143, 156);
            this.cb_rfCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb_rfCheck.Name = "cb_rfCheck";
            this.cb_rfCheck.RectColor = System.Drawing.Color.White;
            this.cb_rfCheck.RectWidth = 1;
            this.cb_rfCheck.SelectedIndex = -1;
            this.cb_rfCheck.SelectedValue = "";
            this.cb_rfCheck.Size = new System.Drawing.Size(139, 32);
            this.cb_rfCheck.Source = null;
            this.cb_rfCheck.TabIndex = 24;
            this.cb_rfCheck.TextValue = null;
            this.cb_rfCheck.TriangleColor = System.Drawing.Color.LightSteelBlue;
            this.cb_rfCheck.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 15.75F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(41, 157);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 28);
            this.label2.TabIndex = 19;
            this.label2.Text = "校验位：";
            this.label2.Visible = false;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 15.75F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(368, 157);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 28);
            this.label6.TabIndex = 23;
            this.label6.Text = "波特率：";
            this.label6.Visible = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 15.75F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(19, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 28);
            this.label3.TabIndex = 20;
            this.label3.Text = "空中速率：";
            this.label3.Visible = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 15.75F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(19, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 28);
            this.label5.TabIndex = 22;
            this.label5.Text = "通信信道:";
            // 
            // cb_rfChan
            // 
            this.cb_rfChan.BackColor = System.Drawing.Color.White;
            this.cb_rfChan.BackColorExt = System.Drawing.Color.White;
            this.cb_rfChan.BoxStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_rfChan.ConerRadius = 5;
            this.cb_rfChan.DropPanelHeight = -1;
            this.cb_rfChan.FillColor = System.Drawing.Color.White;
            this.cb_rfChan.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.cb_rfChan.IsRadius = true;
            this.cb_rfChan.IsShowRect = true;
            this.cb_rfChan.ItemWidth = 70;
            this.cb_rfChan.Location = new System.Drawing.Point(145, 45);
            this.cb_rfChan.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cb_rfChan.Name = "cb_rfChan";
            this.cb_rfChan.RectColor = System.Drawing.Color.White;
            this.cb_rfChan.RectWidth = 1;
            this.cb_rfChan.SelectedIndex = -1;
            this.cb_rfChan.SelectedValue = "";
            this.cb_rfChan.Size = new System.Drawing.Size(139, 32);
            this.cb_rfChan.Source = null;
            this.cb_rfChan.TabIndex = 25;
            this.cb_rfChan.TextValue = null;
            this.cb_rfChan.TriangleColor = System.Drawing.Color.LightSteelBlue;
            // 
            // MenuParaWirelessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 326);
            this.Controls.Add(this.groupBox2);
            this.Name = "MenuParaWirelessForm";
            this.ShowIcon = false;
            this.Text = "Wireless通讯参数";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuParaWirelessForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuParaWirelessForm_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tb_ADDL;
        private System.Windows.Forms.Label label4;
        private HZH_Controls.Controls.UCCombox cb_rfRate;
        private HZH_Controls.Controls.UCCombox cb_rfBaud;
        private HZH_Controls.Controls.UCCombox cb_rfCheck;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private HZH_Controls.Controls.UCCombox cb_rfChan;
        private System.Windows.Forms.Label label13;
        private DTiws.View.ButtonX button1;
        private System.Windows.Forms.Label label1;
    }
}