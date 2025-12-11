using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Cysharp.Threading.Tasks;

public abstract class DataTable
{
    public static readonly string FormatPath = "{0}";

    public abstract UniTask LoadAsync(string filename); //비동기
    
    public static async UniTask<List<T>> LoadCSVAsync<T>(string csvText)
    {
        using (var reader = new StringReader(csvText))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = new List<T>();
            await foreach (var record in csvReader.GetRecordsAsync<T>())
            {
                records.Add(record);
            }

            return records.ToList();
        }
    }
}

public class IntArrayConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
            return new int[0];

        return text.Split(',')
                   .Select(x => int.Parse(x.Trim()))
                   .ToArray();
    }
}
