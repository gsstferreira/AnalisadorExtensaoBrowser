using Common.Classes;
using Common.ClassesJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(NpmPackageJson))]
    public partial class NpmPackageSG : JsonSerializerContext  {}
}
