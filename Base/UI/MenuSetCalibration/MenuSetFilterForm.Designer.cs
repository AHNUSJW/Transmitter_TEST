namespace Base.UI.MenuSet
{
    partial class MenuSetFilterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuSetFilterForm));
            this.resetFilterOreder = new DTiws.View.ButtonX();
            this.setFilterOrder = new DTiws.View.ButtonX();
            this.lbShowFilter = new System.Windows.Forms.ListBox();
            this.lbShowADC = new System.Windows.Forms.ListBox();
            this.buttonX1 = new DTiws.View.ButtonX();
            this.buttonX2 = new DTiws.View.ButtonX();
            this.buttonX3 = new DTiws.View.ButtonX();
            this.SuspendLayout();
            // 
            // resetFilterOreder
            // 
            resources.ApplyResources(this.resetFilterOreder, "resetFilterOreder");
            this.resetFilterOreder.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.resetFilterOreder.EnterForeColor = System.Drawing.Color.White;
            this.resetFilterOreder.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.resetFilterOreder.HoverForeColor = System.Drawing.Color.White;
            this.resetFilterOreder.Name = "resetFilterOreder";
            this.resetFilterOreder.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.resetFilterOreder.PressForeColor = System.Drawing.Color.White;
            this.resetFilterOreder.Radius = 6;
            this.resetFilterOreder.UseVisualStyleBackColor = true;
            this.resetFilterOreder.Click += new System.EventHandler(this.resetFilterOreder_Click);
            // 
            // setFilterOrder
            // 
            resources.ApplyResources(this.setFilterOrder, "setFilterOrder");
            this.setFilterOrder.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.setFilterOrder.EnterForeColor = System.Drawing.Color.White;
            this.setFilterOrder.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.setFilterOrder.HoverForeColor = System.Drawing.Color.White;
            this.setFilterOrder.Name = "setFilterOrder";
            this.setFilterOrder.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.setFilterOrder.PressForeColor = System.Drawing.Color.White;
            this.setFilterOrder.Radius = 6;
            this.setFilterOrder.UseVisualStyleBackColor = true;
            this.setFilterOrder.Click += new System.EventHandler(this.setFilterOrder_Click);
            // 
            // lbShowFilter
            // 
            resources.ApplyResources(this.lbShowFilter, "lbShowFilter");
            this.lbShowFilter.FormattingEnabled = true;
            this.lbShowFilter.Name = "lbShowFilter";
            // 
            // lbShowADC
            // 
            resources.ApplyResources(this.lbShowADC, "lbShowADC");
            this.lbShowADC.FormattingEnabled = true;
            this.lbShowADC.Name = "lbShowADC";
            // 
            // buttonX1
            // 
            resources.ApplyResources(this.buttonX1, "buttonX1");
            this.buttonX1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX1.EnterForeColor = System.Drawing.Color.White;
            this.buttonX1.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX1.HoverForeColor = System.Drawing.Color.White;
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX1.PressForeColor = System.Drawing.Color.White;
            this.buttonX1.Radius = 6;
            this.buttonX1.UseVisualStyleBackColor = true;
            this.buttonX1.Click += new System.EventHandler(this.lbAdcoutCopy_Click);
            // 
            // buttonX2
            // 
            resources.ApplyResources(this.buttonX2, "buttonX2");
            this.buttonX2.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX2.EnterForeColor = System.Drawing.Color.White;
            this.buttonX2.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX2.HoverForeColor = System.Drawing.Color.White;
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX2.PressForeColor = System.Drawing.Color.White;
            this.buttonX2.Radius = 6;
            this.buttonX2.UseVisualStyleBackColor = true;
            this.buttonX2.Click += new System.EventHandler(this.lbAdcoutClear_Click);
            // 
            // buttonX3
            // 
            resources.ApplyResources(this.buttonX3, "buttonX3");
            this.buttonX3.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX3.EnterForeColor = System.Drawing.Color.White;
            this.buttonX3.HoverBackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonX3.HoverForeColor = System.Drawing.Color.White;
            this.buttonX3.Name = "buttonX3";
            this.buttonX3.PressBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX3.PressForeColor = System.Drawing.Color.White;
            this.buttonX3.Radius = 6;
            this.buttonX3.UseVisualStyleBackColor = true;
            this.buttonX3.Click += new System.EventHandler(this.lbFilterClear_Click);
            // 
            // MenuSetFilterForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.buttonX2);
            this.Controls.Add(this.resetFilterOreder);
            this.Controls.Add(this.buttonX3);
            this.Controls.Add(this.buttonX1);
            this.Controls.Add(this.setFilterOrder);
            this.Controls.Add(this.lbShowFilter);
            this.Controls.Add(this.lbShowADC);
            this.Name = "MenuSetFilterForm";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MenuSetFilterForm_FormClosing);
            this.Load += new System.EventHandler(this.MenuSetFilterForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DTiws.View.ButtonX resetFilterOreder;
        private DTiws.View.ButtonX setFilterOrder;
        private System.Windows.Forms.ListBox lbShowFilter;
        private System.Windows.Forms.ListBox lbShowADC;
        private DTiws.View.ButtonX buttonX1;
        private DTiws.View.ButtonX buttonX2;
        private DTiws.View.ButtonX buttonX3;
    }
}