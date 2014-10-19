namespace Pulse.Text
{
    public enum FFXIIITextTagCode
    {
        // Без параметров
        End = 0x00,
        Question = 0x01,
        Italic = 0x02,
        Many = 0x03,
        Article = 0x04,
        ArticleMany = 0x05,


        // С параметром (байт)
        Icon = 0xF0,
        Var = 0xF7,

        // С параметром (именованый)
        Text = 0x40,
        Key = 0xF1
    }
}