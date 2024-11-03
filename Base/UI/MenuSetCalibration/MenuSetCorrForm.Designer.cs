using Library;

namespace Base.UI.MenuSet
{
    partial class MenuSetCorrForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuSetCorrForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.bt_ok = new DTiws.View.ButtonX();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBox1
            // 
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // bt_ok
            // 
            resources.ApplyResources(this.bt_ok, "bt_ok");
            this.bt_ok.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.bt_ok.EnterForeColor = System.Drawing.Color.White;
            this.bt_ok.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.bt_ok.HoverForeColor = System.Drawing.Color.White;
            this.bt_ok.Name = "bt_ok";
            this.bt_ok.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.bt_ok.PressForeColor = System.Drawing.Color.White;
            this.bt_ok.Radius = 6;
            this.bt_ok.UseVisualStyleBackColor = true;
            this.bt_ok.Click += new System.EventHandler(this.bt_ok_Click);
            // 
            // MenuSetCorrForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bt_ok);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MenuSetCorrForm";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuSetCorrForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuSetCorrForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DTiws.View.ButtonX bt_ok;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}