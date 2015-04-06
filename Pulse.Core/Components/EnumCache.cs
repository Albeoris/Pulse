using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public static class EnumCache<T> where T : struct
    {
        public static readonly T[] Values = Enum.GetValues(TypeCache<T>.Type).Cast<T>().ToArray();
        public static readonly string[] Names = Enum.GetNames(TypeCache<T>.Type);

        public static int Count
        {
            get { return Values.Length; }
        }

        public static bool IsDefined(T value)
        {
            return Values.Contains(value);
        }

        public static T? TryParse(string name, StringComparison nameComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            for (int i = 0; i < Count; i++)
            {
                if (String.Equals(Names[i], name, nameComparison))
                    return Values[i];
            }

            return null;
        }

        public static T Parse(string name, T defaulValue, StringComparison nameComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return TryParse(name, nameComparison) ?? defaulValue;
        }

        public static T Parse(string name, StringComparison nameComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            T? result = TryParse(name, nameComparison);
            if (result == null)
                throw Exceptions.CreateException("Тег '{0}' не определен для перечисления '{1}'.", name, TypeCache<T>.Type);
            return result.Value;
        }
    }
}
