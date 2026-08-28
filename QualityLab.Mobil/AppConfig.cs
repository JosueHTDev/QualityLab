namespace QualityLab.Mobil
{
    public static class AppConfig
    {
#if ANDROID
        public const string BaseUrl = "http://10.0.2.2:5080/";
#else
        public const string BaseUrl = "http://localhost:5080/";
#endif
        public const string ClientAppName = "QualityLab.Mobil";
    }
}