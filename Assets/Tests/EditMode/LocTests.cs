using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

/// <summary>Yerelleştirme tablosu: her anahtar iki dilde dolu ve biçim yer tutucuları eşleşiyor.</summary>
public class LocTests
{
    [Test]
    public void TumAnahtarlar_IkiDildeTutarli()
    {
        var field = typeof(Loc).GetField("Table", BindingFlags.NonPublic | BindingFlags.Static);
        var table = (Dictionary<string, string[]>)field.GetValue(null);
        Assert.Greater(table.Count, 30);
        var ph = new Regex(@"\{(\d+)(:[^}]*)?\}");
        foreach (var kv in table)
        {
            Assert.AreEqual(2, kv.Value.Length, kv.Key);
            Assert.IsFalse(string.IsNullOrWhiteSpace(kv.Value[0]), kv.Key + " TR boş");
            Assert.IsFalse(string.IsNullOrWhiteSpace(kv.Value[1]), kv.Key + " EN boş");
            var tr = new List<string>(); foreach (Match m in ph.Matches(kv.Value[0])) tr.Add(m.Groups[1].Value);
            var en = new List<string>(); foreach (Match m in ph.Matches(kv.Value[1])) en.Add(m.Groups[1].Value);
            tr.Sort(); en.Sort();
            CollectionAssert.AreEqual(tr, en, kv.Key + " yer tutucuları farklı");
        }
    }
}
