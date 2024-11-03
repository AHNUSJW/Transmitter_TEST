using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

//Tong Ziyun 
//Ricardo 20230616
//Lumi 20240410

namespace Base.UI.MenuHelp
{
    public partial class MenuModelSelect : Form
    {
        #region 变量定义
        DataView modelData = new DataView();
        ComboBox cb_selectItems = new ComboBox();//代码生成一个Combox控件
        List<int> listSelected = new List<int>();//存储每个combox选项
        List<string> listSelectedText = new List<string>();//记录选择的内容
        int index;//记录操作的表列号
        #endregion

        public MenuModelSelect()
        {
            InitializeComponent();
        }

        //加载界面
        private void MenuModelSelect_Load(object sender, EventArgs e)
        {
            //导入数据
            string dataPath = "";
            if (MyDevice.languageType == 0)
            {
                dataPath = MyDevice.D_picPath + @"\Modelcn.tmp";
            }
            else
            {
                dataPath = MyDevice.D_picPath + @"\Modelen.tmp";
            }
            string err = "";
            DataTable dt = TxtToDataTable(dataPath, '\t', ref err);

            if (dt == null) return;
            modelData = dt.DefaultView;
            //只取前9列
            DataTable newTable = modelData.ToTable(false, dt.Columns.Cast<DataColumn>().Take(9).Select(c => c.ColumnName).ToArray());
            dgvdata.DataSource = newTable;


            //去除首列
            dgvdata.RowHeadersVisible = false;
            //解决用户点击复选框，表格自动增加一行
            dgvdata.AllowUserToAddRows = false;
            dgvdata.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            #region 设置列宽
            dgvdata.Columns[0].FillWeight = 5;
            dgvdata.Columns[1].FillWeight = 10;
            dgvdata.Columns[2].FillWeight = 10;
            dgvdata.Columns[3].FillWeight = 8;
            dgvdata.Columns[4].FillWeight = 8;
            dgvdata.Columns[5].FillWeight = 8;
            dgvdata.Columns[6].FillWeight = 15;
            dgvdata.Columns[7].FillWeight = 8;
            dgvdata.Columns[8].FillWeight = 8;
            #endregion

            for (int i = 0; i < this.dgvdata.Columns.Count; i++)
            {
                dgvdata.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                listSelected.Add(0);
                listSelectedText.Add("无");
            }

            //添加Combox控件
            this.Controls.Add(cb_selectItems);//添加combox
            cb_selectItems.SelectedIndexChanged += new EventHandler(cb_selectItems_SelectedIndexChanged);//添加下拉框事件
            cb_selectItems.Visible = false; //combox控件设置为不可见
            cb_selectItems.BringToFront();
        }

        //清空选项
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listSelected.Count; i++)
            {
                listSelected[i] = 0;
                listSelectedText[i] = "无";
            }

            modelData.RowFilter = "";
        }

        //鼠标双击表格型号内容跳转到对应的产品说明书pdf
        private void dgvdata_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                string cellValue = dgvdata.CurrentCell.Value.ToString();//鼠标双击获取的当前型号值
                DataRowView row = modelData.Cast<DataRowView>().FirstOrDefault(r => r[1].ToString() == cellValue);

                if (row != null)
                {
                    string documentName = row[9].ToString(); // 获取第10列的文档名称
                    string documentPath = Application.StartupPath + "\\pic\\" + documentName;
                    if (File.Exists(documentPath))
                    {
                        Process myProcess = new Process();
                        myProcess.StartInfo.FileName = documentPath;
                        myProcess.StartInfo.Verb = "Open";
                        myProcess.StartInfo.CreateNoWindow = true;
                        myProcess.Start();
                    }
                    else
                    {
                        MessageBox.Show("该型号产品暂无相关产品手册");
                    }
                }
            }
            catch
            {
                MessageBox.Show("该型号产品暂无相关产品手册");
            }
        }

        #region 表下拉框处理

        /// <summary>
        /// 点击dgv列标题，panel显示，并根据列标题的不同位置，对应显示到相应的位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvdata_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int dLeft, dTop;
            if (e.ColumnIndex == 0)
                return;
            //记录操作表的列号
            index = e.ColumnIndex;
            //获取dgv列标题位置相对坐标
            Rectangle range = dgvdata.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            //计算pl_dgv_extend位置坐标
            dLeft = range.Left + dgvdata.Left;
            dTop = range.Top + dgvdata.Top + range.Height;
            //设置pl_dgv_extend位置，超出框体宽度时，和dgv右边对齐
            if (dLeft + cb_selectItems.Width > this.Width)
            {
                cb_selectItems.SetBounds(dgvdata.Width - cb_selectItems.Width, dTop, cb_selectItems.Width, cb_selectItems.Height);
            }
            else
            {
                cb_selectItems.SetBounds(dLeft, dTop, cb_selectItems.Width, cb_selectItems.Height);
            }
            //设置cb_selectItems下拉菜单内容
            cb_selectItems.Items.Clear();
            cb_selectItems.Items.Add("无");
            for (int i = 0; i < dgvdata.Rows.Count; i++)
            {
                bool isfind = false;
                for (int j = 0; j < cb_selectItems.Items.Count; j++)
                {
                    if (cb_selectItems.Items[j].ToString() == dgvdata.Rows[i].Cells[e.ColumnIndex].Value.ToString())
                    {
                        isfind = true;
                        j = cb_selectItems.Items.Count;//break
                    }
                }
                if (!isfind)
                {
                    cb_selectItems.Items.Add(dgvdata.Rows[i].Cells[e.ColumnIndex].Value.ToString());
                }
            }
            cb_selectItems.Text = dgvdata.Columns[index].Name;
            cb_selectItems.Visible = true;
        }

        /// <summary>
        /// combox发生变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_selectItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            //筛选条件添加到listbox
            if (cb_selectItems.Text.Trim() == "无")
            {
                listSelectedText[index] = "无";
            }
            else
            {
                listSelectedText[index] = dgvdata.Columns[index].Name + " = '" + cb_selectItems.Text.Trim() + "'";
            }

            listSelected[index] = cb_selectItems.SelectedIndex;//将选择结果赋值
            selectData();
        }

        //根据条件筛选信息
        private void selectData()
        {
            string search_line = "";
            for (int i = 0; i < listSelectedText.Count; i++)
            {
                if (listSelectedText[i] != "无")
                {
                    search_line += listSelectedText[i] + " and ";
                }
            }
            search_line = search_line.TrimEnd(" and ".ToCharArray());

            modelData.RowFilter = search_line;
            cb_selectItems.Visible = false;
        }
        #endregion

        #region 方法--txt导入dgv
        /// <summary>
        /// 将Txt中数据读入DataTable中
        /// </summary>
        /// <param name="strFileName">文件名称</param>
        /// <param name="isHead">是否包含表头</param>
        /// <param name="strSplit">分隔符</param>
        /// <param name="strErrorMessage">错误信息</param>
        /// <returns>DataTable</returns>
        public static DataTable TxtToDataTable(string strFileName, char strSplit, ref string strErrorMessage)
        {
            DataTable dtReturn = new DataTable();

            try
            {
                string[] strFileTexts = File.ReadAllLines(strFileName, System.Text.Encoding.Default);// System.Text.Encoding.UTF8

                if (strFileTexts.Length == 0) // 如果没有数据
                {
                    strErrorMessage = "文件中没有数据！";
                    return null;
                }

                string[] strLineTexts = strFileTexts[0].Split(strSplit);
                if (strLineTexts.Length == 0)
                {
                    strErrorMessage = "文件中数据格式不正确！";
                    return null;
                }


                for (int i = 0; i < strLineTexts.Length; i++)
                {
                    if (i == 1)
                    {
                        dtReturn.Columns.Add(strLineTexts[i] + (MyDevice.languageType == 0 ? "(双击可打开说明书)" : "(Double-click to open the manual)"));
                    }
                    else
                    {
                        dtReturn.Columns.Add(strLineTexts[i]);
                    }
                }

                for (int i = 1; i < strFileTexts.Length; i++)
                {
                    strLineTexts = strFileTexts[i].Split(strSplit);
                    DataRow dr = dtReturn.NewRow();
                    for (int j = 0; j < strLineTexts.Length; j++)
                    {
                        dr[j] = strLineTexts[j].ToString();
                    }
                    dtReturn.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                strErrorMessage = "读入数据出错！" + ex.Message;

                return null;
            }

            return dtReturn;
        }
        #endregion

    }
}
