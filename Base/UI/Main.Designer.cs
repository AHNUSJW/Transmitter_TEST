using Base.UI;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Base
{
    partial class Main
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetComToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetRS485ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetCANopenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetModbusTCPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetWirelessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetCalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetParameterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetCorrectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetCalFiltToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetCheatingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetFactoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetFacManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetFacAutoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.工具ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.配置工具ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolParExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolParImportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolEepromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.通讯工具ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolRS485ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolCANopenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolReceiverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.数据工具ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolProductToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpGuidlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChineseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EnglishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.开机启动ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CloseAutoStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EnableAutoStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpAboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.LightSteelBlue;
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.设置ToolStripMenuItem,
            this.工具ToolStripMenuItem,
            this.帮助ToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileAccountToolStripMenuItem,
            this.FileExitToolStripMenuItem});
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            resources.ApplyResources(this.文件ToolStripMenuItem, "文件ToolStripMenuItem");
            // 
            // FileAccountToolStripMenuItem
            // 
            this.FileAccountToolStripMenuItem.Name = "FileAccountToolStripMenuItem";
            resources.ApplyResources(this.FileAccountToolStripMenuItem, "FileAccountToolStripMenuItem");
            this.FileAccountToolStripMenuItem.Click += new System.EventHandler(this.FileAccount_ToolStripMenuItem_Click);
            // 
            // FileExitToolStripMenuItem
            // 
            this.FileExitToolStripMenuItem.Name = "FileExitToolStripMenuItem";
            resources.ApplyResources(this.FileExitToolStripMenuItem, "FileExitToolStripMenuItem");
            this.FileExitToolStripMenuItem.Click += new System.EventHandler(this.FileExit_ToolStripMenuItem_Click);
            // 
            // 设置ToolStripMenuItem
            // 
            this.设置ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetConnectToolStripMenuItem,
            this.SetComToolStripMenuItem,
            this.SetCalToolStripMenuItem,
            this.SetFactoryToolStripMenuItem,
            this.SetFacManualToolStripMenuItem,
            this.SetFacAutoToolStripMenuItem});
            this.设置ToolStripMenuItem.Name = "设置ToolStripMenuItem";
            resources.ApplyResources(this.设置ToolStripMenuItem, "设置ToolStripMenuItem");
            this.设置ToolStripMenuItem.MouseHover += new System.EventHandler(this.设置ToolStripMenuItem_MouseHover);
            // 
            // SetConnectToolStripMenuItem
            // 
            this.SetConnectToolStripMenuItem.Name = "SetConnectToolStripMenuItem";
            resources.ApplyResources(this.SetConnectToolStripMenuItem, "SetConnectToolStripMenuItem");
            this.SetConnectToolStripMenuItem.Click += new System.EventHandler(this.SetConnect_ToolStripMenuItem_Click);
            // 
            // SetComToolStripMenuItem
            // 
            this.SetComToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetRS485ToolStripMenuItem,
            this.SetCANopenToolStripMenuItem,
            this.SetModbusTCPToolStripMenuItem,
            this.SetWirelessToolStripMenuItem});
            this.SetComToolStripMenuItem.Name = "SetComToolStripMenuItem";
            resources.ApplyResources(this.SetComToolStripMenuItem, "SetComToolStripMenuItem");
            // 
            // SetRS485ToolStripMenuItem
            // 
            this.SetRS485ToolStripMenuItem.Name = "SetRS485ToolStripMenuItem";
            resources.ApplyResources(this.SetRS485ToolStripMenuItem, "SetRS485ToolStripMenuItem");
            this.SetRS485ToolStripMenuItem.Click += new System.EventHandler(this.SetParRS485_ToolStripMenuItem_Click);
            // 
            // SetCANopenToolStripMenuItem
            // 
            this.SetCANopenToolStripMenuItem.Name = "SetCANopenToolStripMenuItem";
            resources.ApplyResources(this.SetCANopenToolStripMenuItem, "SetCANopenToolStripMenuItem");
            this.SetCANopenToolStripMenuItem.Click += new System.EventHandler(this.SetParCANopen_ToolStripMenuItem_Click);
            // 
            // SetModbusTCPToolStripMenuItem
            // 
            this.SetModbusTCPToolStripMenuItem.Name = "SetModbusTCPToolStripMenuItem";
            resources.ApplyResources(this.SetModbusTCPToolStripMenuItem, "SetModbusTCPToolStripMenuItem");
            this.SetModbusTCPToolStripMenuItem.Click += new System.EventHandler(this.SetParModbusTCP_ToolStripMenuItem_Click);
            // 
            // SetWirelessToolStripMenuItem
            // 
            this.SetWirelessToolStripMenuItem.Name = "SetWirelessToolStripMenuItem";
            resources.ApplyResources(this.SetWirelessToolStripMenuItem, "SetWirelessToolStripMenuItem");
            this.SetWirelessToolStripMenuItem.Click += new System.EventHandler(this.SetParWirelessToolStripMenuItem_Click);
            // 
            // SetCalToolStripMenuItem
            // 
            this.SetCalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetParameterToolStripMenuItem,
            this.SetCalibrationToolStripMenuItem,
            this.SetCorrectionToolStripMenuItem,
            this.SetCalFiltToolStripMenuItem,
            this.SetCheatingToolStripMenuItem});
            this.SetCalToolStripMenuItem.Name = "SetCalToolStripMenuItem";
            resources.ApplyResources(this.SetCalToolStripMenuItem, "SetCalToolStripMenuItem");
            // 
            // SetParameterToolStripMenuItem
            // 
            this.SetParameterToolStripMenuItem.Name = "SetParameterToolStripMenuItem";
            resources.ApplyResources(this.SetParameterToolStripMenuItem, "SetParameterToolStripMenuItem");
            this.SetParameterToolStripMenuItem.Click += new System.EventHandler(this.SetParCal_ToolStripMenuItem_Click);
            // 
            // SetCalibrationToolStripMenuItem
            // 
            this.SetCalibrationToolStripMenuItem.Name = "SetCalibrationToolStripMenuItem";
            resources.ApplyResources(this.SetCalibrationToolStripMenuItem, "SetCalibrationToolStripMenuItem");
            this.SetCalibrationToolStripMenuItem.Click += new System.EventHandler(this.SetCalibration_ToolStripMenuItem_Click);
            // 
            // SetCorrectionToolStripMenuItem
            // 
            this.SetCorrectionToolStripMenuItem.Name = "SetCorrectionToolStripMenuItem";
            resources.ApplyResources(this.SetCorrectionToolStripMenuItem, "SetCorrectionToolStripMenuItem");
            this.SetCorrectionToolStripMenuItem.Click += new System.EventHandler(this.SetCalCorrect_ToolStripMenuItem_Click);
            // 
            // SetCalFiltToolStripMenuItem
            // 
            this.SetCalFiltToolStripMenuItem.Name = "SetCalFiltToolStripMenuItem";
            resources.ApplyResources(this.SetCalFiltToolStripMenuItem, "SetCalFiltToolStripMenuItem");
            this.SetCalFiltToolStripMenuItem.Click += new System.EventHandler(this.SetParFilt_ToolStripMenuItem_Click);
            // 
            // SetCheatingToolStripMenuItem
            // 
            this.SetCheatingToolStripMenuItem.Name = "SetCheatingToolStripMenuItem";
            resources.ApplyResources(this.SetCheatingToolStripMenuItem, "SetCheatingToolStripMenuItem");
            this.SetCheatingToolStripMenuItem.Click += new System.EventHandler(this.SetParCheating_ToolStripMenuItem_Click);
            // 
            // SetFactoryToolStripMenuItem
            // 
            this.SetFactoryToolStripMenuItem.Name = "SetFactoryToolStripMenuItem";
            resources.ApplyResources(this.SetFactoryToolStripMenuItem, "SetFactoryToolStripMenuItem");
            this.SetFactoryToolStripMenuItem.Click += new System.EventHandler(this.SetFactory_ToolStripMenuItem_Click);
            // 
            // SetFacManualToolStripMenuItem
            // 
            this.SetFacManualToolStripMenuItem.Name = "SetFacManualToolStripMenuItem";
            resources.ApplyResources(this.SetFacManualToolStripMenuItem, "SetFacManualToolStripMenuItem");
            this.SetFacManualToolStripMenuItem.Click += new System.EventHandler(this.SetFacManualToolStripMenuItem_Click);
            // 
            // SetFacAutoToolStripMenuItem
            // 
            this.SetFacAutoToolStripMenuItem.Name = "SetFacAutoToolStripMenuItem";
            resources.ApplyResources(this.SetFacAutoToolStripMenuItem, "SetFacAutoToolStripMenuItem");
            this.SetFacAutoToolStripMenuItem.Click += new System.EventHandler(this.SetFacAutoToolStripMenuItem_Click);
            // 
            // 工具ToolStripMenuItem
            // 
            this.工具ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.配置工具ToolStripMenuItem,
            this.通讯工具ToolStripMenuItem,
            this.数据工具ToolStripMenuItem,
            this.ToolProductToolStripMenuItem});
            this.工具ToolStripMenuItem.Name = "工具ToolStripMenuItem";
            resources.ApplyResources(this.工具ToolStripMenuItem, "工具ToolStripMenuItem");
            this.工具ToolStripMenuItem.MouseHover += new System.EventHandler(this.设置ToolStripMenuItem_MouseHover);
            // 
            // 配置工具ToolStripMenuItem
            // 
            this.配置工具ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolParExportToolStripMenuItem,
            this.ToolParImportToolStripMenuItem,
            this.ToolEepromToolStripMenuItem});
            this.配置工具ToolStripMenuItem.Name = "配置工具ToolStripMenuItem";
            resources.ApplyResources(this.配置工具ToolStripMenuItem, "配置工具ToolStripMenuItem");
            // 
            // ToolParExportToolStripMenuItem
            // 
            this.ToolParExportToolStripMenuItem.Name = "ToolParExportToolStripMenuItem";
            resources.ApplyResources(this.ToolParExportToolStripMenuItem, "ToolParExportToolStripMenuItem");
            this.ToolParExportToolStripMenuItem.Click += new System.EventHandler(this.ToolCfgExport_ToolStripMenuItem_Click);
            // 
            // ToolParImportToolStripMenuItem
            // 
            this.ToolParImportToolStripMenuItem.Name = "ToolParImportToolStripMenuItem";
            resources.ApplyResources(this.ToolParImportToolStripMenuItem, "ToolParImportToolStripMenuItem");
            this.ToolParImportToolStripMenuItem.Click += new System.EventHandler(this.ToolCfgImport_ToolStripMenuItem_Click);
            // 
            // ToolEepromToolStripMenuItem
            // 
            this.ToolEepromToolStripMenuItem.Name = "ToolEepromToolStripMenuItem";
            resources.ApplyResources(this.ToolEepromToolStripMenuItem, "ToolEepromToolStripMenuItem");
            this.ToolEepromToolStripMenuItem.Click += new System.EventHandler(this.ToolEepromCfgToolStripMenuItem_Click);
            // 
            // 通讯工具ToolStripMenuItem
            // 
            this.通讯工具ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolRS485ToolStripMenuItem,
            this.ToolCANopenToolStripMenuItem,
            this.ToolReceiverToolStripMenuItem});
            this.通讯工具ToolStripMenuItem.Name = "通讯工具ToolStripMenuItem";
            resources.ApplyResources(this.通讯工具ToolStripMenuItem, "通讯工具ToolStripMenuItem");
            // 
            // ToolRS485ToolStripMenuItem
            // 
            this.ToolRS485ToolStripMenuItem.Name = "ToolRS485ToolStripMenuItem";
            resources.ApplyResources(this.ToolRS485ToolStripMenuItem, "ToolRS485ToolStripMenuItem");
            this.ToolRS485ToolStripMenuItem.Click += new System.EventHandler(this.ToolComRS485_ToolStripMenuItem_Click);
            // 
            // ToolCANopenToolStripMenuItem
            // 
            this.ToolCANopenToolStripMenuItem.Name = "ToolCANopenToolStripMenuItem";
            resources.ApplyResources(this.ToolCANopenToolStripMenuItem, "ToolCANopenToolStripMenuItem");
            this.ToolCANopenToolStripMenuItem.Click += new System.EventHandler(this.ToolComCANopen_ToolStripMenuItem_Click);
            // 
            // ToolReceiverToolStripMenuItem
            // 
            this.ToolReceiverToolStripMenuItem.Name = "ToolReceiverToolStripMenuItem";
            resources.ApplyResources(this.ToolReceiverToolStripMenuItem, "ToolReceiverToolStripMenuItem");
            this.ToolReceiverToolStripMenuItem.Click += new System.EventHandler(this.ToolReceiver_ToolStripMenuItem_Click);
            // 
            // 数据工具ToolStripMenuItem
            // 
            this.数据工具ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolLogToolStripMenuItem,
            this.ToolDataToolStripMenuItem});
            this.数据工具ToolStripMenuItem.Name = "数据工具ToolStripMenuItem";
            resources.ApplyResources(this.数据工具ToolStripMenuItem, "数据工具ToolStripMenuItem");
            // 
            // ToolLogToolStripMenuItem
            // 
            this.ToolLogToolStripMenuItem.Name = "ToolLogToolStripMenuItem";
            resources.ApplyResources(this.ToolLogToolStripMenuItem, "ToolLogToolStripMenuItem");
            this.ToolLogToolStripMenuItem.Click += new System.EventHandler(this.ToolDatLog_ToolStripMenuItem_Click);
            // 
            // ToolDataToolStripMenuItem
            // 
            this.ToolDataToolStripMenuItem.Name = "ToolDataToolStripMenuItem";
            resources.ApplyResources(this.ToolDataToolStripMenuItem, "ToolDataToolStripMenuItem");
            this.ToolDataToolStripMenuItem.Click += new System.EventHandler(this.ToolDatAny_ToolStripMenuItem_Click);
            // 
            // ToolProductToolStripMenuItem
            // 
            this.ToolProductToolStripMenuItem.Name = "ToolProductToolStripMenuItem";
            resources.ApplyResources(this.ToolProductToolStripMenuItem, "ToolProductToolStripMenuItem");
            this.ToolProductToolStripMenuItem.Click += new System.EventHandler(this.ToolProduct_ToolStripMenuItem_Click);
            // 
            // 帮助ToolStripMenuItem
            // 
            this.帮助ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpGuidlineToolStripMenuItem,
            this.HelpLanguageToolStripMenuItem,
            this.开机启动ToolStripMenuItem,
            this.HelpAboutToolStripMenuItem});
            this.帮助ToolStripMenuItem.Name = "帮助ToolStripMenuItem";
            resources.ApplyResources(this.帮助ToolStripMenuItem, "帮助ToolStripMenuItem");
            // 
            // HelpGuidlineToolStripMenuItem
            // 
            this.HelpGuidlineToolStripMenuItem.Name = "HelpGuidlineToolStripMenuItem";
            resources.ApplyResources(this.HelpGuidlineToolStripMenuItem, "HelpGuidlineToolStripMenuItem");
            this.HelpGuidlineToolStripMenuItem.Click += new System.EventHandler(this.HelpGuidline_ToolStripMenuItem_Click);
            // 
            // HelpLanguageToolStripMenuItem
            // 
            this.HelpLanguageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ChineseToolStripMenuItem,
            this.EnglishToolStripMenuItem});
            this.HelpLanguageToolStripMenuItem.Name = "HelpLanguageToolStripMenuItem";
            resources.ApplyResources(this.HelpLanguageToolStripMenuItem, "HelpLanguageToolStripMenuItem");
            // 
            // ChineseToolStripMenuItem
            // 
            this.ChineseToolStripMenuItem.Name = "ChineseToolStripMenuItem";
            resources.ApplyResources(this.ChineseToolStripMenuItem, "ChineseToolStripMenuItem");
            this.ChineseToolStripMenuItem.Click += new System.EventHandler(this.HelpLangCH_ToolStripMenuItem_Click);
            // 
            // EnglishToolStripMenuItem
            // 
            this.EnglishToolStripMenuItem.Name = "EnglishToolStripMenuItem";
            resources.ApplyResources(this.EnglishToolStripMenuItem, "EnglishToolStripMenuItem");
            this.EnglishToolStripMenuItem.Click += new System.EventHandler(this.HelpLangEN_ToolStripMenuItem_Click);
            // 
            // 开机启动ToolStripMenuItem
            // 
            this.开机启动ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CloseAutoStartToolStripMenuItem,
            this.EnableAutoStartToolStripMenuItem});
            this.开机启动ToolStripMenuItem.Name = "开机启动ToolStripMenuItem";
            resources.ApplyResources(this.开机启动ToolStripMenuItem, "开机启动ToolStripMenuItem");
            // 
            // CloseAutoStartToolStripMenuItem
            // 
            this.CloseAutoStartToolStripMenuItem.Name = "CloseAutoStartToolStripMenuItem";
            resources.ApplyResources(this.CloseAutoStartToolStripMenuItem, "CloseAutoStartToolStripMenuItem");
            this.CloseAutoStartToolStripMenuItem.Click += new System.EventHandler(this.CloseAutoStartToolStripMenuItem_Click);
            // 
            // EnableAutoStartToolStripMenuItem
            // 
            this.EnableAutoStartToolStripMenuItem.Name = "EnableAutoStartToolStripMenuItem";
            resources.ApplyResources(this.EnableAutoStartToolStripMenuItem, "EnableAutoStartToolStripMenuItem");
            this.EnableAutoStartToolStripMenuItem.Click += new System.EventHandler(this.EnableAutoStartToolStripMenuItem_Click);
            // 
            // HelpAboutToolStripMenuItem
            // 
            this.HelpAboutToolStripMenuItem.Name = "HelpAboutToolStripMenuItem";
            resources.ApplyResources(this.HelpAboutToolStripMenuItem, "HelpAboutToolStripMenuItem");
            this.HelpAboutToolStripMenuItem.Click += new System.EventHandler(this.HelpAbout_ToolStripMenuItem_Click);
            // 
            // Main
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 设置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetConnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetComToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetCalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetFactoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 工具ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 通讯工具ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 数据工具ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 配置工具ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolProductToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpGuidlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpAboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ChineseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EnglishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetParameterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetCalFiltToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetRS485ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetCANopenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolRS485ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolCANopenToolStripMenuItem;
        private ToolStripMenuItem SetCalibrationToolStripMenuItem;
        private ToolStripMenuItem SetCorrectionToolStripMenuItem;
        private ToolStripMenuItem SetCheatingToolStripMenuItem;
        private ToolStripMenuItem ToolParImportToolStripMenuItem;
        private ToolStripMenuItem ToolParExportToolStripMenuItem;
        private ToolStripMenuItem ToolDataToolStripMenuItem;
        private ToolStripMenuItem ToolLogToolStripMenuItem;
        private ToolStripMenuItem SetFacAutoToolStripMenuItem;
        private ToolStripMenuItem SetFacManualToolStripMenuItem;
        private ToolStripMenuItem ToolEepromToolStripMenuItem;
        private ToolStripMenuItem SetModbusTCPToolStripMenuItem;
        private ToolStripMenuItem SetWirelessToolStripMenuItem;
        private ToolStripMenuItem ToolReceiverToolStripMenuItem;
        private ToolStripMenuItem 开机启动ToolStripMenuItem;
        private ToolStripMenuItem CloseAutoStartToolStripMenuItem;
        private ToolStripMenuItem EnableAutoStartToolStripMenuItem;
    }
}

