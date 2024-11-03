
namespace Base.UI
{
    partial class MultipleDevice
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultipleDevice));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox1_dataCurve = new System.Windows.Forms.CheckBox();
            this.textBoxEx_timeInterval = new HZH_Controls.Controls.TextBoxEx();
            this.enTimeInterval = new System.Windows.Forms.CheckBox();
            this.enAutoRecord = new System.Windows.Forms.CheckBox();
            this.buttonX1 = new DTiws.View.ButtonX();
            this.label_timeInterval = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1_dataTip = new System.Windows.Forms.Label();
            this.dbListView1 = new Base.UI.MyControl.DoubleBufferListView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label2_dataSum = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.panel1.Controls.Add(this.checkBox1_dataCurve);
            this.panel1.Controls.Add(this.textBoxEx_timeInterval);
            this.panel1.Controls.Add(this.enTimeInterval);
            this.panel1.Controls.Add(this.enAutoRecord);
            this.panel1.Controls.Add(this.buttonX1);
            this.panel1.Controls.Add(this.label_timeInterval);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // checkBox1_dataCurve
            // 
            resources.ApplyResources(this.checkBox1_dataCurve, "checkBox1_dataCurve");
            this.checkBox1_dataCurve.Name = "checkBox1_dataCurve";
            this.checkBox1_dataCurve.UseVisualStyleBackColor = true;
            this.checkBox1_dataCurve.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // textBoxEx_timeInterval
            // 
            resources.ApplyResources(this.textBoxEx_timeInterval, "textBoxEx_timeInterval");
            this.textBoxEx_timeInterval.DecLength = 2;
            this.textBoxEx_timeInterval.InputType = HZH_Controls.TextInputType.PositiveInteger;
            this.textBoxEx_timeInterval.MaxValue = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.textBoxEx_timeInterval.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.textBoxEx_timeInterval.MyRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.textBoxEx_timeInterval.Name = "textBoxEx_timeInterval";
            this.textBoxEx_timeInterval.OldText = null;
            this.textBoxEx_timeInterval.PromptColor = System.Drawing.Color.Gray;
            this.textBoxEx_timeInterval.PromptFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxEx_timeInterval.PromptText = "";
            this.textBoxEx_timeInterval.RegexPattern = "";
            this.textBoxEx_timeInterval.TextChanged += new System.EventHandler(this.textBoxEx_timeInterval_TextChanged);
            // 
            // enTimeInterval
            // 
            resources.ApplyResources(this.enTimeInterval, "enTimeInterval");
            this.enTimeInterval.Name = "enTimeInterval";
            this.enTimeInterval.UseVisualStyleBackColor = true;
            this.enTimeInterval.Click += new System.EventHandler(this.enTimeInterval_Click);
            // 
            // enAutoRecord
            // 
            resources.ApplyResources(this.enAutoRecord, "enAutoRecord");
            this.enAutoRecord.Name = "enAutoRecord";
            this.enAutoRecord.UseVisualStyleBackColor = true;
            this.enAutoRecord.Click += new System.EventHandler(this.enAutoRecord_Click);
            // 
            // buttonX1
            // 
            this.buttonX1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonX1.EnterBackColor = System.Drawing.Color.DodgerBlue;
            this.buttonX1.EnterForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.buttonX1, "buttonX1");
            this.buttonX1.HoverBackColor = System.Drawing.Color.CornflowerBlue;
            this.buttonX1.HoverForeColor = System.Drawing.Color.White;
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.PressBackColor = System.Drawing.Color.SkyBlue;
            this.buttonX1.PressForeColor = System.Drawing.Color.White;
            this.buttonX1.Radius = 6;
            this.buttonX1.UseVisualStyleBackColor = true;
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // label_timeInterval
            // 
            resources.ApplyResources(this.label_timeInterval, "label_timeInterval");
            this.label_timeInterval.Name = "label_timeInterval";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Controls.Add(this.dbListView1);
            this.panel3.Controls.Add(this.panel4);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.label1_dataTip);
            this.panel2.Name = "panel2";
            // 
            // label1_dataTip
            // 
            resources.ApplyResources(this.label1_dataTip, "label1_dataTip");
            this.label1_dataTip.Name = "label1_dataTip";
            // 
            // dbListView1
            // 
            resources.ApplyResources(this.dbListView1, "dbListView1");
            this.dbListView1.HideSelection = false;
            this.dbListView1.Name = "dbListView1";
            this.dbListView1.UseCompatibleStateImageBehavior = false;
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Controls.Add(this.label2_dataSum);
            this.panel4.Name = "panel4";
            // 
            // label2_dataSum
            // 
            resources.ApplyResources(this.label2_dataSum, "label2_dataSum");
            this.label2_dataSum.Name = "label2_dataSum";
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chart1);
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            resources.ApplyResources(this.chart1, "chart1");
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            // 
            // MultipleDevice
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "MultipleDevice";
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MultipleDevice_FormClosed);
            this.Load += new System.EventHandler(this.MultipleDevice_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private DTiws.View.ButtonX buttonX1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox enAutoRecord;
        private MyControl.DoubleBufferListView dbListView1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label2_dataSum;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1_dataTip;
        private System.Windows.Forms.CheckBox enTimeInterval;
        private HZH_Controls.Controls.TextBoxEx textBoxEx_timeInterval;
        private System.Windows.Forms.Label label_timeInterval;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox checkBox1_dataCurve;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}