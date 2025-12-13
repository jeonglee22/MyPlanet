using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class CsvSaveUtil
{
    public static string ToCsv<T>(System.Collections.Generic.IEnumerable<T> rows)
    {
        using var sw = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
        csv.WriteRecords(rows);
        return sw.ToString();
    }

    public static async UniTask SaveTextAsync(string fullPath, string text)
    {
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(fullPath, text, new UTF8Encoding(true));
    }
}