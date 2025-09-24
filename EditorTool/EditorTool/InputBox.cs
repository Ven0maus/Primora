using System.Drawing;
using System.Windows.Forms;

namespace EditorTool
{
    public static class InputBox
    {
        public static string Show(string prompt, string title = "Input", string defaultValue = "")
        {
            using (Form form = new Form())
            using (Label label = new Label())
            using (TextBox textBox = new TextBox())
            using (Button buttonOk = new Button())
            using (Button buttonCancel = new Button())
            {
                form.Text = title;
                label.Text = prompt;
                textBox.Text = defaultValue;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.AutoSize = true;
                label.MaximumSize = new Size(400, 0); // wrap long text
                textBox.Width = 300;

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 3,
                    Padding = new Padding(10),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };

                layout.Controls.Add(label, 0, 0);
                layout.SetColumnSpan(label, 2);

                layout.Controls.Add(textBox, 0, 1);
                layout.SetColumnSpan(textBox, 2);

                layout.Controls.Add(buttonOk, 0, 2);
                layout.Controls.Add(buttonCancel, 1, 2);

                form.Controls.Add(layout);

                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                form.AutoSize = true;
                form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }
    }

}
