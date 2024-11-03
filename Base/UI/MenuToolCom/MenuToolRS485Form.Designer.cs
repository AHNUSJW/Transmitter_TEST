namespace Base.UI.MenuTool
{
    partial class MenuToolRS485Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuToolRS485Form));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btSend = new DTiws.View.ButtonX();
            this.listView1 = new System.Windows.Forms.ListView();
            this.lb_show = new System.Windows.Forms.ListBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.btSend);
            this.groupBox2.Controls.Add(this.listView1);
            this.groupBox2.Controls.Add(this.lb_show);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btSend
            // 
            resources.ApplyResources(this.btSend, "btSend");
            this.btSend.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.btSend.EnterForeColor = System.Drawing.Color.White;
            this.btSend.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btSend.HoverForeColor = System.Drawing.Color.White;
            this.btSend.Name = "btSend";
            this.btSend.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.btSend.PressForeColor = System.Drawing.Color.White;
            this.btSend.Radius = 6;
            this.btSend.UseVisualStyleBackColor = true;
            this.btSend.Click += new System.EventHandler(this.btSend_Click);
            // 
            // listView1
            // 
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.AllowColumnReorder = true;
            this.listView1.AutoArrange = false;
            this.listView1.HideSelection = false;
            this.listView1.Name = "listView1";
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseClick);
            this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDoubleClick);
            // 
            // lb_show
            // 
            resources.ApplyResources(this.lb_show, "lb_show");
            this.lb_show.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lb_show.FormattingEnabled = true;
            this.lb_show.Name = "lb_show";
            this.lb_show.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lb_show_DrawItem);
            // 
            // MenuToolRS485Form
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.groupBox2);
            this.MaximizeBox = false;
            this.Name = "MenuToolRS485Form";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuToolRS485Form_FormClosing);
            this.Load += new System.EventHandler(this.MenuToolRS485Form_Load);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ListBox lb_show;
        private DTiws.View.ButtonX btSend;
    }
}