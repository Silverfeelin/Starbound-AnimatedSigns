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
        public string Light { get; set; } = string.Empty;
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
        public string BorderInner { get; set; } = "00000000";
        public string BorderOuter { get; set; } = "00000000";

        public SignTemplate() { }
    }
}
