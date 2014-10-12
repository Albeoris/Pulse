namespace Pulse.Core
{
    public enum FFXFileSignatures
    {
        Ftcx = 0x58435446,
        Map = 0x3150414D,
        Bgm = 0x204D4742,
        Elf = 0x464C457F,
        Ebp = 0x31305645,
        Scn = 0x00000008 // Не сигнатура, а количество записей в заголовке, возможны ложные срабатывания
    }
}