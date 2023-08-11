namespace Febucci.UI.Core.Parsing
{
    public class TMPTagParser : TagParserBase
    {
        readonly bool richTagsEnabled;
        public override bool shouldPasteTag => richTagsEnabled;

        //PS no "noparse" since it's already checked
        static readonly string[] lookup = new[]
        {
            "<align=", "</align>", 
            "<allcaps>", "</allcaps>",
            "<alpha=", "</alpha>",
            "<b>", "</b>", 
            "<color=", "</color>", 
            "<cspace=", "</cspace>",
            "<font=", "</font>", 
            "<font-weight=", "</font-weight>", 
            "<gradient=", "</gradient>", 
            "<i>", "</i>", 
            "<indent=", "</indent>", 
            "<line-height=", "</line-height>",
            "<line-indent=", "</line-indent>",
            "<link=", "</link>",
            "<lowercase>", "</lowercase>",
            "<margin=", "</margin>", "<margin-left>", "<margin-right>",
            "<mark=", "</mark>", 
            "<mspace=", "</mspace>", 
            "<nobr>", "</nobr>",
            "<page>",
            "<pos=",
            "<rotate=", "</rotate>", 
            "<s>", "</s>", 
            "<size=", 
            "<smallcaps>", "</smallcaps>",
            "<space=", 
            "<sprite", "<sprite ", 
            "<style=", "</style>", 
            "<sub>", "</sub>", 
            "<sup>", "</sup>", 
            "<u>", "</u>",
            "<uppercase>", "</uppercase>", 
            "<voffset=", "</voffset>",
            "<width=", "</width>" 
        };

        public TMPTagParser(bool richTagsEnabled, char openingBracket, char closingBracket, char closingTagSymbol)
            : base(openingBracket, closingBracket, closingTagSymbol)
        {
            this.richTagsEnabled = richTagsEnabled;
        }

        public override bool TryProcessingTag(string textInsideBrackets, int tagLength, int realTextIndex, int internalOrder)
        {
            if (!richTagsEnabled) return false;
            
            string fullTag = startSymbol + textInsideBrackets + endSymbol; //TODO improve
            
            foreach (var tag in lookup)
            {
                if (fullTag.StartsWith(tag, true, System.Globalization.CultureInfo.InvariantCulture))
                    return true;
            }
            
            return false;
        }
    }
}