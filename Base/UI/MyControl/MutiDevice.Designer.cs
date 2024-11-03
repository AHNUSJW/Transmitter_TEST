namespace Base.UI.MyControl
{
    partial class MutiDevice
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MutiDevice));
            this.ucSignalLamp1 = new HZH_Controls.Controls.UCSignalLamp();
            this.address = new System.Windows.Forms.Label();
            this.signalOutput1 = new System.Windows.Forms.Label();
            this.signalUnit1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button_tare = new DTiws.View.ButtonX();
            this.button_zero = new DTiws.View.ButtonX();
            this.SuspendLayout();
            // 
            // ucSignalLamp1
            // 
            resources.ApplyResources(this.ucSignalLamp1, "ucSignalLamp1");
            this.ucSignalLamp1.IsHighlight = true;
            this.ucSignalLamp1.IsShowBorder = false;
            this.ucSignalLamp1.LampColor = new System.Drawing.Color[] {
        System.Drawing.Color.Black};
            this.ucSignalLamp1.Name = "ucSignalLamp1";
            this.ucSignalLamp1.TwinkleSpeed = 0;
            // 
            // address
            // 
            resources.ApplyResources(this.address, "address");
            this.address.Name = "address";
            // 
            // signalOutput1
            // 
            resources.ApplyResources(this.signalOutput1, "signalOutput1");
            this.signalOutput1.Cursor = System.Windows.Forms.Cursors.Default;
            this.signalOutput1.ForeColor = System.Drawing.Color.Black;
            this.signalOutput1.Name = "signalOutput1";
            this.signalOutput1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.signalOutput1_MouseDoubleClick);
            // 
            // signalUnit1
            // 
            resources.ApplyResources(this.signalUnit1, "signalUnit1");
            this.signalUnit1.Name = "signalUnit1";
            // 
            // comboBox1
            // 
            resources.ApplyResources(this.comboBox1, "comboBox1");
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button_tare
            // 
            resources.ApplyResources(this.button_tare, "button_tare");
            this.button_tare.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_tare.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button_tare.EnterForeColor = System.Drawing.Color.White;
            this.button_tare.HoverBackColor = System.Drawing.Color.CornflowerBlue;
            this.button_tare.HoverForeColor = System.Drawing.Color.White;
            this.button_tare.Name = "button_tare";
            this.button_tare.PressBackColor = System.Drawing.Color.SkyBlue;
            this.button_tare.PressForeColor = System.Drawing.Color.White;
            this.button_tare.Radius = 6;
            this.button_tare.UseVisualStyleBackColor = true;
            this.button_tare.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // button_zero
            // 
            resources.ApplyResources(this.button_zero, "button_zero");
            this.button_zero.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_zero.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.button_zero.EnterForeColor = System.Drawing.Color.White;
            this.button_zero.HoverBackColor = System.Drawing.Color.CornflowerBlue;
            this.button_zero.HoverForeColor = System.Drawing.Color.White;
            this.button_zero.Name = "button_zero";
            this.button_zero.PressBackColor = System.Drawing.Color.SkyBlue;
            this.button_zero.PressForeColor = System.Drawing.Color.White;
            this.button_zero.Radius = 6;
            this.button_zero.UseVisualStyleBackColor = true;
            this.button_zero.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // MutiDevice
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.button_tare);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.signalUnit1);
            this.Controls.Add(this.signalOutput1);
            this.Controls.Add(this.button_zero);
            this.Controls.Add(this.address);
            this.Controls.Add(this.ucSignalLamp1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Name = "MutiDevice";
            this.Load += new System.EventHandler(this.MutiDevice_Load);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MutiDevice_MouseDoubleClick);
            this.Resize += new System.EventHandler(this.MutiDevice_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private HZH_Controls.Controls.UCSignalLamp ucSignalLamp1;
        private System.Windows.Forms.Label address;
        private DTiws.View.ButtonX button_zero;
        private System.Windows.Forms.Label signalOutput1;
        private System.Windows.Forms.Label signalUnit1;
        private System.Windows.Forms.ComboBox comboBox1;
        private DTiws.View.ButtonX button_tare;
    }
}
