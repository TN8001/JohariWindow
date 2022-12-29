// [ジョハリの窓Webアプリ（β） ～みんなで自己分析・他己分析～ | 適性検査「ポテクト」](https://potect-a.com/johari-app/)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp1;


public partial class Report : ObservableObject
{
    public string? Name { get; init; }
    [ObservableProperty] private List<string>? _Open;
    [ObservableProperty] private List<string>? _Hidden;
    [ObservableProperty] private List<string>? _Blind;
    [ObservableProperty] private List<string>? _Unknown;

    internal readonly List<string> Myself = new();
    internal readonly List<string> Others = new();
    internal string? ColumnName;

    internal void Analyse(List<string> features)
    {
        Open = Myself.Intersect(Others).ToList();
        Hidden = Myself.Except(Others).ToList();
        //Blind = Others.Except(Myself).ToList();
        Blind = Others.Where(x => !Myself.Contains(x))
                      .GroupBy(x => x)
                      .Select(x => 1 < x.Count() ? $"{x.Key} ×{x.Count()}" : x.Key)
                      .ToList();
        Unknown = features.Except(Myself).Except(Others).ToList();
    }
}


[INotifyPropertyChanged]
public partial class MainWindow : Window
{
    [ObservableProperty] private DataTable _AdjectivesList = new();
    [ObservableProperty] private string _State = "スタート";
    [ObservableProperty] private string _Message = "メンバーの名前を入力してください";
    public DataTable Participants { get; } = new();
    public ObservableCollection<Report> Reports { get; } = new();

    private int index;
    private readonly List<string> features = """
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
        慎重
        """.Split(Environment.NewLine).ToList();


    public MainWindow()
    {
        InitializeComponent();

        Participants.Columns.Add("参加者", typeof(string));
        Participants.RowChanged += (_, _) => ClickCommand.NotifyCanExecuteChanged();
        Participants.RowDeleted += (_, _) => ClickCommand.NotifyCanExecuteChanged();
    }


    private bool CanClick() => 4 <= Participants.Rows.Count;
    [RelayCommand(CanExecute = nameof(CanClick))]
    private void OnClick()
    {
        switch (State)
        {
            case "スタート":
                Reports.Clear();
                for (int i = 0; i < Participants.Rows.Count; i++)
                {
                    var row = Participants.Rows[i];
                    Reports.Add(new() { Name = (string)row[0], ColumnName = $"Columns{i}" });
                }

                Next();
                break;

            case "次の人へ":
                Extract();
                Next();
                break;

            case "結果を見る":
                Extract();

                foreach (var r in Reports) r.Analyse(features);

                Message = "診断結果";
                State = "もう一度";
                index = -1;
                break;

            case "もう一度":
                Message = "メンバーの名前を入力してください";
                State = "スタート";
                index++;
                break;
        }
    }

    private void Next()
    {
        var name = (string)Participants.Rows[index][0];

        var dt = new DataTable(name);
        dt.Columns.Add("feature", typeof(string));

        for (int i = 0; i < Participants.Rows.Count; i++)
        {
            var row = Participants.Rows[i];
            var col = dt.Columns.Add((string)row[0], typeof(bool));
            col.Caption = (string)row[0];
            col.ColumnName = $"Columns{i}";
            col.DefaultValue = false;
        }

        foreach (var f in features) dt.Rows.Add(f);

        AdjectivesList = dt;


        Message = $"{name} さんの番です\nメンバーに当てはまると思う特徴をチェックしてください";
        State = "次の人へ";
        index++;
        if (index == Participants.Rows.Count) State = "結果を見る";
    }

    private void Extract()
    {
        var name = AdjectivesList.TableName;
        foreach (DataColumn col in AdjectivesList.Columns)
        {
            if (col.ColumnName == "feature") continue;

            var target = Reports.Single(x => x.ColumnName == col.ColumnName);
            foreach (DataRow row in AdjectivesList.Rows)
            {
                if (!(bool)row[col.ColumnName]) continue;

                if (name == target.Name) target.Myself.Add((string)row[0]);
                else target.Others.Add((string)row[0]);
            }
        }
    }

    // [C# WPF DataGrid AutoGeneratingColumn PackIcon Content - Stack Overflow](https://stackoverflow.com/questions/74252029/c-sharp-wpf-datagrid-autogeneratingcolumn-packicon-content)
    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyType == typeof(bool))
        {
            var xaml = $$"""
              <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <CheckBox HorizontalAlignment="Center" IsChecked="{Binding [{{e.PropertyName}}], UpdateSourceTrigger=PropertyChanged}">
                  <CheckBox.LayoutTransform>
                    <ScaleTransform ScaleX="1.5" ScaleY="1.5" />
                  </CheckBox.LayoutTransform>
                </CheckBox>
              </DataTemplate>
              """;

            // [【WPF】DataGridのヘッダに _/.[]() を含むと正しく表示されない件の対策 | 趣味や仕事に役立つ初心者DIYプログラミング入門](https://resanaplaza.com/2021/04/17/%E3%80%90wpf%E3%80%91datagrid%E3%81%AE%E3%83%98%E3%83%83%E3%83%80%E3%81%AB-_-%E3%82%92%E5%90%AB%E3%82%80%E3%81%A8%E6%AD%A3%E3%81%97%E3%81%8F%E8%A1%A8%E7%A4%BA%E3%81%95%E3%82%8C%E3%81%AA%E3%81%84/)
            if (sender is DataGrid { ItemsSource: DataView { Table: DataTable table } })
            {
                var index = table.Columns.IndexOf(e.PropertyName);
                e.Column = new DataGridTemplateColumn
                {
                    CellTemplate = (DataTemplate)XamlReader.Parse(xaml),
                    Header = table.Columns[index].Caption,
                    Width = new(1, DataGridLengthUnitType.Star),
                };
            }
            else throw new InvalidOperationException();
        }
        else
        {
            e.Column.IsReadOnly = true;
            e.Column.Header = "";
            ((DataGridTextColumn)e.Column).Foreground = Brushes.Black;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // [c# - how to set focus to particular cell of WPF toolkit datagrid - Stack Overflow](https://stackoverflow.com/questions/3421597/how-to-set-focus-to-particular-cell-of-wpf-toolkit-datagrid/59059075#59059075)
        participantsDataGrid.Focus();
        (Keyboard.FocusedElement as UIElement)?.MoveFocus(new(FocusNavigationDirection.Next));
    }
}
