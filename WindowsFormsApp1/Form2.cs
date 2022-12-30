using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private int index;

        public Form2() => InitializeComponent();

        /// <summary></summary>
        /// <param name="names">全員の名前</param>
        /// <param name="index">自分のインデックス</param>
        /// <param name="features">特徴選択肢</param>
        public Form2(string[] names, int index, string[] features) : this()
        {
            this.index = index;

            // 自分の名前をセット
            label1.Text = label1.Text.Replace("○○", names[index]);

            // 左端は常に自分。自分以外を順にセット
            var others = names.ToList();
            others.RemoveAt(index);
            groupBox2.Text = others[0];
            groupBox3.Text = others[1];
            groupBox4.Text = others[2];

            // 特徴選択肢をセット
            listBox1.Items.Clear(); // デザイナに仮で入れている選択肢をクリア
            listBox1.Items.AddRange(features);
            listBox2.Items.AddRange(features);
            listBox3.Items.AddRange(features);
            listBox4.Items.AddRange(features);
        }

        /// <summary>入力結果を取得</summary>
        /// <returns>個人の特徴（string[]）を人数分入れたリスト（namesで渡された順）</returns>
        public List<string[]> GetReports()
        {
            var results = new List<string[]>
            {
                listBox1.SelectedItems.Cast<string>().ToArray(),
                listBox2.SelectedItems.Cast<string>().ToArray(),
                listBox3.SelectedItems.Cast<string>().ToArray(),
                listBox4.SelectedItems.Cast<string>().ToArray(),
            };

            // 自分が先頭になっているので、元の順に入れ替え
            var s = results[0]; // 自分をバックアップ
            results.RemoveAt(0); // 自分を削除
            results.Insert(index, s); // 自分を元のインデックスに挿入

            return results;
        }

        // 入力せずに閉じられると面倒なため、OK以外で閉じられなくする
        // [フォームの「閉じる」ボタンを無効にする - .NET Tips (VB.NET,C#...)](https://dobon.net/vb/dotnet/form/disabledclosebutton.html)
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x112;
            const long SC_CLOSE = 0xF060L;
            if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt64() & 0xFFF0L) == SC_CLOSE) return;
            base.WndProc(ref m);
        }
    }
}
