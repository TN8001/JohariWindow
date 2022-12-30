using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace WindowsFormsApp1
{
    ///<summary>ジョハリの窓 診断用紙</summary>
    public class Report
    {
        ///<summary>対象者の名前</summary>
        public string Name { get; set; }

        ///<summary>開放の窓（自分も他人も挙げた特徴）</summary>
        public List<string> Open { get; private set; }

        ///<summary>秘密の窓（自分だけが挙げた特徴）</summary>
        public List<string> Hidden { get; private set; }

        ///<summary>盲点の窓（他人だけが挙げた特徴）</summary>
        public List<string> Blind { get; private set; }

        ///<summary>未知の窓（自分も他人も挙げなかった特徴）</summary>
        public List<string> Unknown { get; private set; }

        ///<summary>自分が思う自分の特徴</summary>
        public List<string> Myself { get; } = new List<string>();

        ///<summary>他人が思う自分の特徴（重複あり）</summary>
        public List<string> Others { get; } = new List<string>();


        /// <summary>診断結果生成</summary>
        /// <param name="features">すべての特徴選択肢</param>
        public void Analyse(List<string> features)
        {
            // Myself・Othersどちらにも入っているもの（積集合）
            Open = Myself.Intersect(Others).ToList();

            // MyselfにあってOthersにないもの（差集合）
            Hidden = Myself.Except(Others).ToList();

            // OthersにあってMyselfにないもの（差集合）
            //Blind = Others.Except(Myself).ToList();

            // ↑では重複が取り除かれてしまう（×2とかが不要であれば↑で十分）
            Blind = Others.Where(x => !Myself.Contains(x)) // OthersのうちMyselfに含まれていないものを
                          .GroupBy(x => x) // グループ化し
                          .Select(x => 1 < x.Count() ? $"{x.Key} ×{x.Count()}" : x.Key) // 個数が２個以上なら票数を追加
                          .ToList();

            // featuresにあってMyself・Othersにないもの（差集合）
            Unknown = features.Except(Myself).Except(Others).ToList();
        }
    }
}