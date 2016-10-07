using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedSigns
{
    public class SignTemplate
    {
        public int FPS { get; set; } = 4;
        private string _light = string.Empty;
        public string Light
        {
            get
            {
                return _light;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _light = null;
                else
                    _light = value.Replace("#", "");
            }
        }
        public bool Wired { get; set; } = false;
        public int StartIndex { get; set; } = 1;
        public string Category { get; set; } = "decorative";
        public string ShortDescription { get; set; } = "Sign";
        public string Rarity { get; set; } = "Essential";
        public string DefaultDescription { get; set; } = string.Empty;
        public string ApexDescription { get; set; } = string.Empty;
        public string AvianDescripion { get; set; } = string.Empty;
        public string FloranDescription { get; set; } = string.Empty;
        public string GlitchDescription { get; set; } = string.Empty;
        public string HumanDescription { get; set; } = string.Empty;
        public string HylotlDescription { get; set; } = string.Empty;
        public string NovakidDescription { get; set; } = string.Empty;
        public bool TransparentBack { get; set; } = true;
        public string Back { get; set; } = "blank";
        private string _borderInner = "00000000";
        public string BorderInner
        {
            get
            {
                return _borderInner;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _borderInner = "00000000";
                else
                    _borderInner = value.Replace("#", "");
            }
        }
        private string _borderOuter = "00000000";
        public string BorderOuter
        {
            get
            {
                return _borderOuter;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _borderOuter = "00000000";
                else
                    _borderOuter = value.Replace("#", "");
            }
        }
        public bool UseSubfolder { get; set; } = false;
        public string ExportPath { get; set; } = null;

        public SignTemplate() { }
    }
}
