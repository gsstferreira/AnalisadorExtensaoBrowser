using Common.Classes;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Common.Services
{
    // Código retirado do pacote String.Similarity - https://github.com/glienard/StringSimilarity.NET
    // Reimplementação realizada para remover métodos não utilizados e aprimorar a eficiência de memória no
    // caso de uso da análise de arquivos javascript.
    //
    // A bilioteca original realiza a construção e preenchimento de um IDictionary baseado no conteúdo do arquivo .js
    // toda vez que a similaridade é calculada. Como o arquivo .js da extensão é conhecido e não passa por alterações,
    // basta que o IDictionary seja criado uma única vez e mantido durante o ciclo de vida da análise.

    //Calcula a similaridade entre textos - https://en.wikipedia.org/wiki/Cosine_similarity
    public partial class CosineSimilarityService
    {
        [GeneratedRegex("\\s+")]
        private static partial Regex RegexSimilarity();

        private const int DEFAULT_K = 5;
        private static readonly Regex SPACE_REG = RegexSimilarity();

        public static void SetCosineProfile(JSFile jsFile)
        {
            jsFile.Profile = GetProfile(jsFile.Content);
            jsFile.Norm = Norm(jsFile.Profile);
        }
        public static double GetSimilarity(JSFile jsFile, string comparison)
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
                var profileComparison = GetProfile(comparison);
                double similarity = DotProduct(jsFile.Profile, profileComparison) / (jsFile.Norm * Norm(profileComparison));
                profileComparison.Clear();
                return similarity;
            }
        }
        private static Dictionary<string, short> GetProfile(string s)
        {
            var dictionary = new Dictionary<string, short>();
            string text = SPACE_REG.Replace(s, " ");
            for (int i = 0; i < text.Length - DEFAULT_K + 1; i++)
            {
                string key = text.Substring(i, DEFAULT_K);
                if (dictionary.TryGetValue(key, out var value))
                {
                    dictionary[key] = (short)(value + 1);
                }
                else
                {
                    dictionary[key] = 1;
                }
            }

            return dictionary;
        }
        private static double Norm(IDictionary<string, short> profile)
        {
            int num = 0;

            foreach (var item in profile)
            {
                num += item.Value * item.Value;
            }

            return Math.Sqrt(1.0 * num);
        }
        private static double DotProduct(IDictionary<string, short> profile1, IDictionary<string, short> profile2)
        {
            IDictionary<string, short> dictionary = profile2;
            IDictionary<string, short> dictionary2 = profile1;
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
    }
}
