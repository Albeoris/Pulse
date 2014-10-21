namespace Pulse.Text
{
    public enum FFXIIITextTagCode : byte
    {
        // === 1 байт ===
        End = 0x00,
        Question = 0x01,
        Italic = 0x02,
        Separator = 0x03,
        Article = 0x04,
        ArticleMany = 0x05,

        // --- 1 + 1 байт ---
        Var81 = 0x81,
        Var85 = 0x85,
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