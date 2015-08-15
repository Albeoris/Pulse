using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Pulse.Core
{
    public static class Exceptions
    {
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception CreateException(string message, params object[] args)
        {
            return new Exception(String.Format(message, args));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception CreateArgumentException(string paramName, string message, params object[] args)
        {
            return new ArgumentException(String.Format(message, args), paramName);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CheckArgumentNull<T>(T arg, string name) where T : class
        {
            if (ReferenceEquals(arg, null))
                throw new ArgumentNullException(name);

            return arg;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CheckArgumentNullOrEmprty(string arg, string name)
        {
            if (ReferenceEquals(arg, null))
                throw new ArgumentNullException(name);
            if (arg == string.Empty)
                throw new ArgumentEmptyException(name);

            return arg;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CheckArgumentNullOrEmprty<T>(T arg, string name) where T : IList
        {
            if (ReferenceEquals(arg, null))
                throw new ArgumentNullException(name);
            if (arg.Count == 0)
                throw new ArgumentEmptyException(name);

            return arg;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CheckFileNotFoundException(string fullName)
        {
            CheckArgumentNullOrEmprty(fullName, "fullName");
            if (!File.Exists(fullName))
                throw new FileNotFoundException(fullName);

            return fullName;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CheckDirectoryNotFoundException(string fullName)
        {
            CheckArgumentNullOrEmprty(fullName, "fullName");
            if (!Directory.Exists(fullName))
                throw new DirectoryNotFoundException(fullName);

            return fullName;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlElement CheckXmlElement(XmlElement node, string nodeName)
        {
            CheckArgumentNull(node, "node");
            if (node.Name != nodeName)
                throw new XmlException($"Неверное имя узла: '{node.Name}'. Ожидается: '{nodeName}'.");

            return node;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CheckArgumentOutOfRangeException<T>(T value, string name, T minValue, T maxValue) where T : IComparable<T>
        {
            if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
                throw new ArgumentOutOfRangeException(name, value, $"Значение аргумента ({name} = {value}) выходит за пределы допустимого диапазона: ({minValue}~{maxValue}).");
            return value;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CheckReadableStream<T>(T stream, string name) where T : Stream
        {
            CheckArgumentNull(stream, name);

            if (!stream.CanRead)
            {
                if (!stream.CanWrite)
                    throw new ObjectDisposedException("stream", "Stream closed.");
                throw new NotSupportedException("Unreadable stream.");
            }

            return stream;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CheckWritableStream<T>(T stream, string name) where T : Stream
        {
            CheckArgumentNull(stream, name);

            if (!stream.CanWrite)
            {
                if (!stream.CanRead)
                    throw new ObjectDisposedException("output", "Stream closed.");
                throw new NotSupportedException("Unwritable stream.");
            }

            return stream;
        }
    }
}