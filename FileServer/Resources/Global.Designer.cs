namespace FileServer.Resources
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Global
    {

        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Global()
        {
        }
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FileServer.Resources.Global", typeof(Global).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }
        #region App
        public static string FileFormatIsIncorrect
        {
            get
            {
                return ResourceManager.GetString("FileFormatIsIncorrect", resourceCulture);
            }
        }
        public static string FileSizeIsBig
        {
            get
            {
                return ResourceManager.GetString("FileSizeIsBig", resourceCulture);
            }
        }
        public static string AppNotFound
        {
            get
            {
                return ResourceManager.GetString("AppNotFound", resourceCulture);
            }
        }
        public static string AppApiKeyNotFound
        {
            get
            {
                return ResourceManager.GetString("AppApiKeyNotFound", resourceCulture);
            }
        }
        public static string DocumentNotFound
        {
            get
            {
                return ResourceManager.GetString("DocumentNotFound", resourceCulture);
            }
        }
        public static string ErrorInProccess
        {
            get
            {
                return ResourceManager.GetString("ErrorInProccess", resourceCulture);
            }
        }
        public static string InputFormatIsIncorrect
        {
            get
            {
                return ResourceManager.GetString("InputFormatIsIncorrect", resourceCulture);
            }
        }
        public static string ResizeParametersIsBigerThanOriginal
        {
            get
            {
                return ResourceManager.GetString("ResizeParametersIsBigerThanOriginal", resourceCulture);
            }
        }
        public static string TagHadWatermark
        {
            get
            {
                return ResourceManager.GetString("TagHadWatermark", resourceCulture);
            }
        }
        public static string WatermarkHasNotType
        {
            get
            {
                return ResourceManager.GetString("WatermarkHasNotType", resourceCulture);
            }
        }
        public static string ImageHasNotWatermark
        {
            get
            {
                return ResourceManager.GetString("ImageHasNotWatermark", resourceCulture);
            }
        }
        public static string AppApiKeyIsInvalid
        {
            get
            {
                return ResourceManager.GetString("AppApiKeyIsInvalid", resourceCulture);
            }
        }
        #endregion
    }
}
