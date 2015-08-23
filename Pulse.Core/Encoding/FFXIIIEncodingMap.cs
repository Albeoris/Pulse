using System.Collections.Generic;

namespace Pulse.Core
{
    public static class FFXIIIEncodingMap
    {
        private static readonly Dictionary<int, int> IndexToValueDic;
        private static readonly Dictionary<int, int> ValueToIndexDic;

        static FFXIIIEncodingMap()
        {
            ValueToIndexDic = InitializeValueToIndex();
            IndexToValueDic = InitializeIndexToValue();
        }

        public static int ValueToIndex(int value)
        {
            if (value < 0x80)
                return value;

            return ValueToIndex(value >> 8, value & 0xFF);
        }

        public static int ValueToIndex(int hight, int low)
        {
            return ValueToIndexDic[(hight << 8) | low];
        }

        public static void IndexToValue(int index, out int hight, out int low)
        {
            if (index < 0x80)
            {
                hight = 0;
                low = index;
                return;
            }

            int value = IndexToValueDic[index];
            hight = (value & 0xFF00) >> 8;
            low = value & 0x00FF;
        }

        private static Dictionary<int, int> InitializeIndexToValue()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>(ValueToIndexDic.Count);
            foreach (KeyValuePair<int, int>  pair in ValueToIndexDic)
            {
                if (pair.Value >= 0xC0 || pair.Value <= 0xDF)
                {
                    if (!dic.ContainsKey(pair.Value))
                        dic.Add(pair.Value, pair.Key);
                }
                else
                {
                    dic.Add(pair.Value, pair.Key);
                }
            }
            return dic;
        }

        private static Dictionary<int, int> InitializeValueToIndex()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>
            {
                {0x851C, 0x5C}, // _it
                {0x859F, 0xC0},

                {0x8140, 0x100},
                {0x8141, 0x101},
                {0x8142, 0x102},
                {0x8145, 0x105},
                {0x8146, 0x106},
                {0x8148, 0x108},
                {0x8149, 0x109},
                {0x8151, 0x111},
                {0x815B, 0x11B},
                {0x815C, 0x11C},
                {0x815E, 0x11E},
                {0x8160, 0x120},
                {0x8163, 0x123},
                {0x8169, 0x129},
                {0x816A, 0x12A},
                {0x8173, 0x133},
                {0x8174, 0x134},
                {0x8175, 0x135},
                {0x8176, 0x136},
                {0x8177, 0x137}, // 13-2
                {0x8178, 0x138}, // 13-2
                {0x8179, 0x139},
                {0x817A, 0x13A},
                {0x817B, 0x13B},
                {0x817C, 0x13C},
                {0x817E, 0x13E},
                {0x8181, 0x140},
                {0x8183, 0x142},
                {0x8184, 0x143},
                {0x8185, 0x144},
                {0x8187, 0x146}, // 13-2
                {0x8193, 0x152},
                {0x8195, 0x154},
                {0x8199, 0x158}, // 13-3
                {0x819A, 0x159},
                {0x819B, 0x15A},
                {0x81A0, 0x15F},
                {0x81A2, 0x161},
                {0x81A6, 0x165},
                {0x81A8, 0x167},
                {0x81A9, 0x168},
                {0x81AA, 0x169},
                {0x81AB, 0x16A},
                {0x81F4, 0x1B3}
            };

            // High ANSI
            for (int i = 0x41; i <= 0x9D; i++)
                dic.Add(0x8500 + i, 0x40 + i);
            for (int i = 0xA0; i <= 0xDE; i++)
                dic.Add(0x8500 + i, 0x21 + i);

            return dic;
        }
    }
}