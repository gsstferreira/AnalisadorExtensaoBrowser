﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.WebClasses.VirusTotal
{
    public class VTResponseStatistics
    {
        [JsonPropertyName("malicious")]
        public int NumMalicious { get; set; }
        [JsonPropertyName("suspicious")]
        public int NumSuspicious { get; set; }
        [JsonPropertyName("undetected")]
        public int NumUndetectd { get; set; }
        [JsonPropertyName("harmless")]
        public int NumHarmless { get; set; }
        [JsonPropertyName("timeout")]
        public int NumTimeout { get; set; }
        [JsonPropertyName("confirmed-timeout")]
        public int NumTimeoutConfirmed { get; set; }
        [JsonPropertyName("failure")]
        public int NumFailure { get; set; }
        [JsonPropertyName("type-unsupported")]
        public int NumUnsupported { get; set; }
        public VTResponseStatistics() { }
    }
}
