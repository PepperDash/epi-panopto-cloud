using System;
namespace PreEmptive.Dotfuscator.ObfuscationAttributes
{
    public sealed class ObfuscationAttribute : Attribute
    {
        private bool exclude = true;
        private bool strip = true;
        private bool applyToMembers = true;
        [Obfuscation(Feature = "renaming", Exclude = true)]
        private string feature = "all";

        public ObfuscationAttribute()
        { }

        public bool ApplyToMembers
        {
            get { return applyToMembers; }
            set { applyToMembers = true; }
        }

        public bool Exclude
        {
            get { return exclude; }
            set { exclude = value; }
        }

        public string Feature
        {
            get { return feature; }
            set { feature = value; }
        }

        public bool StripAfterObfuscation
        {
            get { return strip; }
            set { strip = value; }
        }
    }
}
