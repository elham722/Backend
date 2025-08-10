namespace Backend.Infrastructure.LocalStorage
{
    public class LocalStorageOptions
    {
        public bool AutoLoad { get; set; } = true;
        public bool AutoSave { get; set; } = true;
        public string Filename { get; set; } = "DigitekShop.LocalStorage";
        public string Directory { get; set; } = "LocalStorage";
    }
} 