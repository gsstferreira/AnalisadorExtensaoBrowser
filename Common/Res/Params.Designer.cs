﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Common.Res {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Params {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Params() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Common.Res.Params", typeof(Params).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to crx2,crx3.
        /// </summary>
        public static string AcceptedFormat {
            get {
                return ResourceManager.GetString("AcceptedFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 126.0.6478.260.
        /// </summary>
        public static string ChromeProdVersion {
            get {
                return ResourceManager.GetString("ChromeProdVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://clients2.google.com/service/update2/crx?response=redirect&amp;prodversion=[PRODVER]&amp;acceptformat=[FORMAT]&amp;x=id%3D[ID]%26uc.
        /// </summary>
        public static string DownloadURL {
            get {
                return ResourceManager.GetString("DownloadURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://safebrowsing.googleapis.com/v4/threatMatches:find?key=.
        /// </summary>
        public static string GSBLookupURL {
            get {
                return ResourceManager.GetString("GSBLookupURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://www.npmjs.com/package/.
        /// </summary>
        public static string NpmPackageUrl {
            get {
                return ResourceManager.GetString("NpmPackageUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://registry.npmjs.com/[NAME].
        /// </summary>
        public static string NPMRegistryGetURL {
            get {
                return ResourceManager.GetString("NPMRegistryGetURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://registry.npmjs.com/-/v1/search?text=[NAME]&amp;size=5.
        /// </summary>
        public static string NPMRegistryQueryURL {
            get {
                return ResourceManager.GetString("NPMRegistryQueryURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://security.snyk.io/package/npm/[pack]/[version].
        /// </summary>
        public static string SnykDbUrl {
            get {
                return ResourceManager.GetString("SnykDbUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://chromewebstore.google.com/detail/.
        /// </summary>
        public static string ViewURL {
            get {
                return ResourceManager.GetString("ViewURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://www.virustotal.com/api/v3/files.
        /// </summary>
        public static string VirusTotalFileURL {
            get {
                return ResourceManager.GetString("VirusTotalFileURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://www.virustotal.com/api/v3/files/upload_url.
        /// </summary>
        public static string VirusTotalUrlRequest {
            get {
                return ResourceManager.GetString("VirusTotalUrlRequest", resourceCulture);
            }
        }
    }
}
