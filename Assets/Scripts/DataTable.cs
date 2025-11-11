using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class DataTable
{
    public static readonly string FormatPath = "SeoTest/Scripts/DataTables/{0}";

    public abstract void Load(string filename); //동기
    public abstract UniTask LoadAsync(string filename); //비동기

    public static List<T> LoadCSV<T>(string csvText)
    {
        using (var reader = new StringReader(csvText))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csvReader.GetRecords<T>();
            return records.ToList();
        }
    }
    
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
