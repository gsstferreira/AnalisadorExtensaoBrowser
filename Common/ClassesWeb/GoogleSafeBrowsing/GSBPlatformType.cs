﻿using System.Text.Json.Serialization;

namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    [JsonConverter(typeof(JsonStringEnumConverter<GSBPlatformType>))]
    public enum GSBPlatformType
    {
        PLATFORM_TYPE_UNSPECIFIED = 0,
        WINDOWS = 1,
        LINUX = 2,
        ANDROID = 3,
        OSX = 4,
        IOS = 5,
        ANY_PLATFORM = 6,
        ALL_PLATFORMS = 7,
        CHROME = 8
    }
}
