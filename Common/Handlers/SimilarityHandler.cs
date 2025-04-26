using Common.Classes;
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

    // As funções "string.substring()" foram substituídas por "Span.Slice()" para aprimorar uso de memória e tempo de execução.
    // Para acomodar o uso de Spans, trocou-se o dicionário <string,int> por <ulong,int>, com as chaves sendo o cálculo do
    // hash (Knuth Hash Function) dos caracteres (chars) presentes em cada fatia do Span

    //Calcula a similaridade entre textos - https://en.wikipedia.org/wiki/Cosine_similarity
    public partial class SimilarityHandler
    {
        [GeneratedRegex("\\s+")]
        private static partial Regex RegexSimilarity();

        public const int DEFAULT_K = 5;
        private static readonly Regex SPACE_REG = RegexSimilarity();

        public static void SetCosineProfile(JSFile jsFile)
        {
            GetProfile(jsFile.Content, jsFile.Profile);
            jsFile.Norm = Norm(jsFile.Profile);
        }
        public static double GetSimilarity(JSFile jsFile, string comparison, Dictionary<ulong, int> compDictionary)
        {
            if (jsFile.Lenght < DEFAULT_K || comparison.Length < DEFAULT_K)
            {
                return 0.0;
            }
            else if (jsFile.Content.Equals(comparison))
            {
                return 1.0;
            }
            else
            {
                GetProfile(comparison, compDictionary);
                double similarity = DotProduct(jsFile.Profile, compDictionary) / (jsFile.Norm * Norm(compDictionary));
                return similarity;
            }
        }
        private static void GetProfile(string s, Dictionary<ulong, int> dictionary)
        {
            var text = SPACE_REG.Replace(s, " ").AsSpan();
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                ulong key = CalculateKeyKnut(text.Slice(i, DEFAULT_K));

                if (dictionary.TryGetValue(key, out var value))
                {
                    dictionary[key] = value + 1;
                }
                else
                {
                    dictionary[key] = 1;
                }
            }
        }
        private static double Norm(IDictionary<ulong, int> profile)
        {
            long num = 0;

            foreach (var item in profile)
            {
                num += item.Value * item.Value;
            }

            return Math.Sqrt(1.0 * num);
        }
        private static long DotProduct(Dictionary<ulong, int> profile1, Dictionary<ulong, int> profile2)
        {
            Dictionary<ulong, int> dictionary = profile2;
            Dictionary<ulong, int> dictionary2 = profile1;

            if (profile1.Count < profile2.Count)
            {
                dictionary = profile1;
                dictionary2 = profile2;
            }

            long num = 0;
            foreach (var item in dictionary)
            {
                if (dictionary2.TryGetValue(item.Key, out var value))
                {
                    num += item.Value * value;
                }
            }
            return num;
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
        //private static ulong CalculateKey(ReadOnlySpan<char> span)
        //{
        //    HashCode hash = new HashCode();

        //    for (int i = 0; i < span.Length; i++)
        //    {
        //        hash.Add(span[i]);
        //    }

        //    return (ulong) hash.ToHashCode();
        //}
    }
}
