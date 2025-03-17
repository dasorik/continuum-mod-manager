using Continuum.Core.Interfaces;

namespace Continuum.Core.Models
{
    public abstract class BaseConfiguration : IVersionLoadableData
    {
        public abstract string Type { get; }
        public abstract string ID { get; }
        public abstract string CacheFolder { get; set; }

        public string Version;
        public ModContributor Author;
        public ModContributor[] Contributors;
        public UpdateKeys UpdateKeys;
        public string DisplayName;
        public string DisplayImage;
        public string DisplayBackground;
        public string Description;
    }
}
