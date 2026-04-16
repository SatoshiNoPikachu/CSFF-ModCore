using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
public static class Extensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }

    extension<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        public TValue? GetValueOrDefault(TKey key)
        {
            return dict.TryGetValue(key, out var value) ? value : default;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (dict.ContainsKey(key)) return false;

            dict[key] = value;
            return true;
        }
    }

    extension(File)
    {
        public static async Task<string> ReadAllTextAsync(string path)
        {
            using var reader = new StreamReader(path, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}