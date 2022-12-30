using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1() => InitializeComponent();

        /// <summary></summary>
        /// <param name="report">ジョハリの窓 診断用紙</param>
        public UserControl1(Report report) : this()
        {
            // 名前をセット
            groupBox1.Text = groupBox1.Text.Replace("○○", report.Name);

            // それぞれの窓にLabelを追加
            flowLayoutPanel1.Controls.AddRange(report.Open.Select(x => new Label { Text = x, }).ToArray());
            flowLayoutPanel2.Controls.AddRange(report.Blind.Select(x => new Label { Text = x, }).ToArray());
            flowLayoutPanel3.Controls.AddRange(report.Hidden.Select(x => new Label { Text = x, }).ToArray());
            flowLayoutPanel4.Controls.AddRange(report.Unknown.Select(x => new Label { Text = x, }).ToArray());
        }
    }
}
