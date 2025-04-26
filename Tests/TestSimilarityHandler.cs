using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Common.Handlers
{
    // Código retirado do pacote String.Similarity - https://github.com/glienard/StringSimilarity.NET
    // Reimplementação realizada para remover métodos não utilizados e aprimorar a eficiência de memória no
    // caso de uso da análise de arquivos javascript.
    //
    // A bilioteca original realiza a construção e preenchimento de um IDictionary baseado no conteúdo do arquivo .js
    // toda vez que a similaridade é calculada. Como o arquivo .js da extensão é conhecido e não passa por alterações,
    // basta que o IDictionary seja criado uma única vez e mantido durante o ciclo de vida da análise.

    //Calcula a similaridade entre textos - https://en.wikipedia.org/wiki/Cosine_similarity
    public partial class TestSimilarityHandler
    {
        [GeneratedRegex("\\s+")]
        private static partial Regex RegexSimilarity();

        private const int DEFAULT_K = 5;
        private static readonly Regex SPACE_REG = RegexSimilarity();

        public static double GetSimilarityInt(string a, string b)
        {
            if (a.Length < DEFAULT_K || b.Length < DEFAULT_K)
            {
                return 0.0;
            }
            else if (a.Equals(b))
            {
                return 1.0;
            }
            else
            {
                var profileA = GetProfileInt(a);
                var profileB = GetProfileInt(b);
                double similarity = DotProductInt(profileA, profileB) / (NormInt(profileA) * NormInt(profileB));
                return similarity;
            }
        }

        public static double GetSimilarityReut(string a, string b, Dictionary<int,int> reuseDic1, Dictionary<int, int> reuseDic2)
        {
            if (a.Length < DEFAULT_K || b.Length < DEFAULT_K)
            {
                return 0.0;
            }
            else if (a.Equals(b))
            {
                return 1.0;
            }
            else
            {
                GetProfileReut(a, reuseDic1);
                GetProfileReut(b, reuseDic2);
                return DotProductReut(reuseDic1, reuseDic2) / (NormReut(reuseDic1) * NormReut(reuseDic2));
            }
        }
        public static double GetSimilarityShort(string a, string b, Dictionary<int, ushort> reuseDic1, Dictionary<int, ushort> reuseDic2)
        {
            if (a.Length < DEFAULT_K || b.Length < DEFAULT_K)
            {
                return 0.0;
            }
            else if (a.Equals(b))
            {
                return 1.0;
            }
            else
            {
                GetProfileShort(a, reuseDic1);
                GetProfileShort(b, reuseDic2);
                return DotProductShort(reuseDic1, reuseDic2) / (NormShort(reuseDic1) * NormShort(reuseDic2));
            }
        }
        private static Dictionary<int, int> GetProfileInt(string s)
        {
            var dictionary = new Dictionary<int, int>();

            var text = SPACE_REG.Replace(s, " ").AsSpan();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                int key = CalculateKey(text.Slice(i, DEFAULT_K));

                if (dictionary.TryGetValue(key, out var value))
                {
                    dictionary[key] = value + 1;
                }
                else
                {
                    dictionary[key] = 1;
                }
            }

            return dictionary;
        }
        private static void GetProfileReut(string s, Dictionary<int, int> reuseDictionary)
        {
            var text = SPACE_REG.Replace(s, " ").AsSpan();
            reuseDictionary.Clear();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                int key = CalculateKey(text.Slice(i, DEFAULT_K));

                if (reuseDictionary.TryGetValue(key, out var value))
                {
                    reuseDictionary[key] = value + 1;
                }
                else
                {
                    reuseDictionary[key] = 1;
                }
            }
        }
        private static void GetProfileShort(string s, Dictionary<int, ushort> reuseDictionary)
        {
            var text = SPACE_REG.Replace(s, " ").AsSpan();
            reuseDictionary.Clear();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                int key = CalculateKey(text.Slice(i, DEFAULT_K));

                if (reuseDictionary.TryGetValue(key, out var value))
                {
                    reuseDictionary[key] = (ushort)(value + 1);
                }
                else
                {
                    reuseDictionary[key] = 1;
                }
            }
        }
        private static double NormInt(IDictionary<int, int> profile)
        {
            int num = 0;

            foreach (var item in profile)
            {
                num += item.Value * item.Value;
            }

            return Math.Sqrt(1.0 * num);
        }
        private static double NormReut(IDictionary<int, int> profile)
        {
            int num = 0;

            foreach (var item in profile)
            {
                num += item.Value * item.Value;
            }

            return Math.Sqrt(1.0 * num);
        }
        private static double NormShort(IDictionary<int, ushort> profile)
        {
            int num = 0;

            foreach (var item in profile)
            {
                num += item.Value * item.Value;
            }

            return Math.Sqrt(1.0 * num);
        }
        private static double DotProductInt(Dictionary<int, int> profile1, Dictionary<int, int> profile2)
        {
            Dictionary<int, int> dictionary = profile2;
            Dictionary<int, int> dictionary2 = profile1;

            if (profile1.Count < profile2.Count)
            {
                dictionary = profile1;
                dictionary2 = profile2;
            }

            int num = 0;
            foreach (var item in dictionary)
            {
                if (dictionary2.TryGetValue(item.Key, out var value))
                {
                    num += item.Value * value;
                }
            }
            return 1.0 * num;
        }
        private static double DotProductReut(Dictionary<int, int> profile1, Dictionary<int, int> profile2)
        {
            Dictionary<int, int> dictionary = profile2;
            Dictionary<int, int> dictionary2 = profile1;

            if (profile1.Count < profile2.Count)
            {
                dictionary = profile1;
                dictionary2 = profile2;
            }

            int num = 0;
            foreach (var item in dictionary)
            {
                if (dictionary2.TryGetValue(item.Key, out var value))
                {
                    num += item.Value * value;
                }
            }
            return 1.0 * num;
        }
        private static double DotProductShort(Dictionary<int, ushort> profile1, Dictionary<int, ushort> profile2)
        {
            Dictionary<int, ushort> dictionary = profile2;
            Dictionary<int, ushort> dictionary2 = profile1;

            if (profile1.Count < profile2.Count)
            {
                dictionary = profile1;
                dictionary2 = profile2;
            }

            int num = 0;
            foreach (var item in dictionary)
            {
                if (dictionary2.TryGetValue(item.Key, out var value))
                {
                    num += item.Value * value;
                }
            }
            return 1.0 * num;
        }
        private static int CalculateKey(ReadOnlySpan<char> span)
        {
            HashCode h = new();

            for (int i = 0; i < DEFAULT_K; i++)
            {
                h.Add(span[i]);
            }
            return h.ToHashCode();
        }

        public static void TestSimilarities(string s1, string s2)
        {
            var qtdIter = 100000;

            var watchInt = new Stopwatch();
            watchInt.Start();
            for (int i = 0; i < qtdIter; i++)
            {
                GetSimilarityInt(s1, s2);
            }
            watchInt.Stop();

            double avgInt = 1000.0 * watchInt.ElapsedMilliseconds / qtdIter;

            Console.WriteLine("new Init total time: {0}ms ; avg: {1:0.00}mis", watchInt.ElapsedMilliseconds, avgInt);

            var watchReut = new Stopwatch();
            watchReut.Start();
            Dictionary<int, int> dic1 = [];
            Dictionary<int, int> dic2 = [];

            for (int i = 0; i < qtdIter; i++)
            {
                GetSimilarityReut(s1, s2, dic1, dic2);
            }
            watchReut.Stop();

            double avgReut = 1000.0 * watchReut.ElapsedMilliseconds / qtdIter;

            Console.WriteLine("Reutilization total time: {0}ms ; avg: {1:0.00}mis", watchReut.ElapsedMilliseconds, avgReut);

            var watchShort = new Stopwatch();
            watchShort.Start();
            Dictionary<int, ushort> Sdic1 = [];
            Dictionary<int, ushort> Sdic2 = [];

            for (int i = 0; i < qtdIter; i++)
            {
                GetSimilarityShort(s1, s2, Sdic1, Sdic2);
            }
            watchShort.Stop();

            double avgShort = 1000.0 * watchShort.ElapsedMilliseconds / qtdIter;

            Console.WriteLine("Short Reut. total time: {0}ms ; avg: {1:0.00}mis", watchShort.ElapsedMilliseconds, avgShort);


        }

        public static void GetProfileManualHash(string s, Dictionary<int, int> reuseDictionary)
        {
            var text = SPACE_REG.Replace(s, " ").AsSpan();
            reuseDictionary.Clear();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                int key = CalculateKey(text.Slice(i, DEFAULT_K));

                if (reuseDictionary.TryGetValue(key, out var value))
                {
                    reuseDictionary[key] = value + 1;
                }
                else
                {
                    reuseDictionary[key] = 1;
                }
            }
        }
        public static void GetProfileHashCode(string s, Dictionary<int, int> reuseDictionary)
        {
            var text = SPACE_REG.Replace(s, " ").AsMemory();
            reuseDictionary.Clear();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                int key = text.Slice(i, DEFAULT_K).GetHashCode();

                if (reuseDictionary.TryGetValue(key, out var value))
                {
                    reuseDictionary[key] = value + 1;
                }
                else
                {
                    reuseDictionary[key] = 1;
                }
            }
        }
        public static void GetProfileKnut(string s, Dictionary<ulong, int> reuseDictionary)
        {
            var text = SPACE_REG.Replace(s, " ").AsSpan();
            reuseDictionary.Clear();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                ulong key = CalculateKeyKnut(text.Slice(i, DEFAULT_K));

                if (reuseDictionary.TryGetValue(key, out var value))
                {
                    reuseDictionary[key] = value + 1;
                }
                else
                {
                    reuseDictionary[key] = 1;
                }
            }
        }
        private static ulong CalculateKeyKnut(ReadOnlySpan<char> span)
        {
            ulong hash = 3074457345618258799ul;

            for (int i = 0; i < span.Length; i++) 
            {
                hash += span[i];
                hash *= 3074457345618258799ul;
            }

            return hash;
        }

    }
}
