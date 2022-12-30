using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // 人数分の診断用紙
        private readonly List<Report> Reports = new List<Report>();

        // 現在の状態フラグ（兼ボタンテキスト）
        private string State { get => button1.Text; set => button1.Text = value; }

        // ユーザー向けメッセージ
        private string Message { set => label1.Text = value; }

        // 参加者名TextBox
        private readonly TextBox[] Participants;

        // 入力中の参加者インデックス
        private int index;

        // 特徴選択肢
        private readonly List<string> features = @"
頭が良い
発想力がある
段取り力がある
向上心がある
行動力がある
表情が豊か
話し上手
聞き上手
親切
リーダーシップ
空気が読める
情報通
根性がある
責任感がある
プライドが高い
自信家
頑固
真面目
慎重".Split(new string[] { "\n", "\r\n", }, StringSplitOptions.RemoveEmptyEntries).ToList();


        public Form1()
        {
            InitializeComponent();

            Participants = new TextBox[] { textBox1, textBox2, textBox3, textBox4, };

            flowLayoutPanel1.Controls.Clear(); // 仮で入れたUserControl1を削除
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            switch (State)
            {
                case "スタート":
                    // 人数分の診断用紙作り直し
                    Reports.Clear();
                    foreach (var textBox in Participants)
                        Reports.Add(new Report() { Name = textBox.Text, });

                    Next();
                    break;

                case "次の人へ":
                    Next();
                    break;

                case "結果を見る":
                    foreach (var report in Reports)
                    {
                        report.Analyse(features); // 診断
                        flowLayoutPanel1.Controls.Add(new UserControl1(report)); // 診断結果追加
                    }

                    Message = "診断結果";
                    State = "もう一度";
                    index = -1;
                    break;

                case "もう一度":
                    flowLayoutPanel1.Controls.Clear(); // 診断結果クリア

                    Message = "メンバーの名前を入力してください";
                    State = "スタート";
                    index++;
                    break;
            }
        }

        private void Next()
        {
            // 全員の名前
            var names = Reports.Select(x => x.Name).ToArray();

            // 全員の名前, 自分のインデックス, 特徴選択肢
            var form2 = new Form2(names, index, features.ToArray());
            form2.ShowDialog();

            // 全員分の特徴
            var results = form2.GetReports();
            for (var i = 0; i < results.Count; i++)
            {
                // 自分の分はMyselfに追加
                if (i == index) Reports[i].Myself.AddRange(results[i]);
                // 他人の分はOthersに追加
                else Reports[i].Others.AddRange(results[i]);
            }

            Message = "";
            State = "次の人へ";

            index++;

            // もし最後の人なら結果へ
            if (index == Participants.Length) State = "結果を見る";
        }
    }
}
