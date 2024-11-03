using System.Drawing;
using System.Windows.Forms;

//Lumi 20241021

namespace Base.UI.MyControl
{
    public partial class CustomMessageBox : Form
    {
        public bool DontShowAgain { get; private set; }
        public string MessageText
        {
            get => messageLabel.Text;
            set => messageLabel.Text = value;
        }

        private CheckBox checkBox;
        private Button yesButton;
        private Button noButton;
        private Label messageLabel;

        public CustomMessageBox()
        {
            Text = "确认";
            Width = 450;
            Height = 170;

            FormBorderStyle = FormBorderStyle.FixedDialog; // 设置为不可拖动大小
            MaximizeBox = false; // 禁用最大化按钮
            StartPosition = FormStartPosition.CenterParent; // 居中显示

            messageLabel = new Label
            {
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(messageLabel);

            checkBox = new CheckBox
            {
                Text = "不再提示",
                AutoSize = true,
                Location = new Point(20, 60)
            };
            Controls.Add(checkBox);

            yesButton = new Button
            {
                Text = "是",
                DialogResult = DialogResult.Yes,
                Location = new Point(140, 96)
            };
            Controls.Add(yesButton);

            noButton = new Button
            {
                Text = "否",
                DialogResult = DialogResult.No,
                Location = new Point(230, 96)
            };
            Controls.Add(noButton);

            AcceptButton = yesButton;
            CancelButton = noButton;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            DontShowAgain = checkBox.Checked;
        }
    }
}
