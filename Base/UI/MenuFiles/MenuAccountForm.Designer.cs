using Library;

namespace Base.UI.MenuFile
{
    partial class MenuAccountForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuAccountForm));
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button4 = new DTiws.View.ButtonX();
            this.button3 = new DTiws.View.ButtonX();
            this.button2 = new DTiws.View.ButtonX();
            this.button1 = new DTiws.View.ButtonX();
            this.SuspendLayout();
            // 
            // textBox2
            // 
            resources.ApplyResources(this.textBox2, "textBox2");
            this.textBox2.Name = "textBox2";
            this.textBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.login_KeyDown);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.login_KeyDown);
            // 
            // comboBox1
            // 
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.login_KeyDown);
            // 
            // notifyIcon1
            // 
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button4
            // 
            resources.ApplyResources(this.button4, "button4");
            this.button4.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button4.EnterForeColor = System.Drawing.Color.White;
            this.button4.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button4.HoverForeColor = System.Drawing.Color.White;
            this.button4.Name = "button4";
            this.button4.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button4.PressForeColor = System.Drawing.Color.White;
            this.button4.Radius = 6;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button3.EnterForeColor = System.Drawing.Color.White;
            this.button3.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button3.HoverForeColor = System.Drawing.Color.White;
            this.button3.Name = "button3";
            this.button3.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button3.PressForeColor = System.Drawing.Color.White;
            this.button3.Radius = 6;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button2.EnterForeColor = System.Drawing.Color.White;
            this.button2.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button2.HoverForeColor = System.Drawing.Color.White;
            this.button2.Name = "button2";
            this.button2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button2.PressForeColor = System.Drawing.Color.White;
            this.button2.Radius = 6;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button1.EnterForeColor = System.Drawing.Color.White;
            this.button1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.button1.HoverForeColor = System.Drawing.Color.White;
            this.button1.Name = "button1";
            this.button1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.button1.PressForeColor = System.Drawing.Color.White;
            this.button1.Radius = 6;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MenuAccountForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MenuAccountForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MenuAccountForm_FormClosed);
            this.Load += new System.EventHandler(this.MenuAccountForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private DTiws.View.ButtonX button1;
        private DTiws.View.ButtonX button2;
        private DTiws.View.ButtonX button3;
        private DTiws.View.ButtonX button4;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Timer timer1;
    }
}