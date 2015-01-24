namespace Pulse.Core
{
    public enum FFXIIITextTagCode : byte
    {
        // === 1 байт ===
        End = 0x00,
        Escape = 0x01, // OLD: Question (replace it!)
        Question = 0x01,
        Italic = 0x02,
        Many = 0x03, // OLD: Separator (replace it!)
        Separator = 0x03,
        Article = 0x04,
        ArticleMany = 0x05,

        // --- 1 + 1 байт ---
        //Var81 = 0x81, // Old
        //Var82 = 0x82, // Spanish chars
        //Var83 = 0x83, // Other chars
        //Var84 = 0x84, // Other chars
        //Var85 = 0x85, // Old
        //Var91 = 0x91, // Other chars
        VarF4 = 0xF4,
        VarF6 = 0xF6,
        VarF7 = 0xF7,

        // --- 1 + 1 именованный ---
        Text = 0x40,
        Icon = 0xF0,
        Key = 0xF1,
        Color = 0xF9
    }
}