using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Pulse.Core
{
    public static class TypeCache<T>
    {
        public static readonly Type Type = typeof(T);
        public static readonly int UnsafeSize = GetSize();

        private static int GetSize()
        {
            DynamicMethod dynamicMethod = new DynamicMethod("SizeOf", typeof(int), Type.EmptyTypes);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Sizeof, Type);
            generator.Emit(OpCodes.Ret);

            return ((Func<int>)dynamicMethod.CreateDelegate(typeof(Func<int>)))();
        }
    }
}