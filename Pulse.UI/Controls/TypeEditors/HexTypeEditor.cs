using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using Be.Windows.Forms;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pulse.UI
{
    public class HexTypeEditor : TypeEditor<UiHexControl>
    {
        protected override UiHexControl CreateEditor()
        {
            return (UiHexControl)new UiHexControl();
        }

        protected override void SetControlProperties()
        {
        }

        protected override void SetValueDependencyProperty()
        {
            this.ValueProperty = UiHexControl.ValueProperty;
        }
    }

    public sealed class UiHexControl : WindowsFormsHost
    {
        private readonly HexBox _hexBox;
        private readonly object _lock = new object();
        private bool _isInternalCall;

        public UiHexControl()
        {
            _hexBox = new HexBox {VScrollBarVisible = true};
            Height = 70;
            Child = _hexBox;
        }

        public byte[] Value
        {
            get { return (byte[])GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(byte[]), typeof(UiHexControl), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UiHexControl self = (UiHexControl)d;
            byte[] value = e.NewValue as byte[];

            lock (self._lock)
            {
                if (self._isInternalCall)
                    return;

                FixedByteProvider byteProvider = null;
                if (value == null)
                {
                    self._hexBox.Height = 0;
                }
                else
                {
                    byteProvider = new FixedByteProvider((byte[])e.NewValue);
                    self._hexBox.Height = 70;
                }

                self._hexBox.ByteProvider = byteProvider;
                if (byteProvider != null)
                    byteProvider.Changed += (s, a) => OnValueEdited(self, byteProvider);
            }
        }

        private static void OnValueEdited(UiHexControl self, FixedByteProvider byteProvider)
        {
            Monitor.Enter(self._lock);
            try
            {
                self._isInternalCall = true;
                self.Value = byteProvider.Bytes.ToArray();
                self._isInternalCall = false;
            }
            finally
            {
                Monitor.Exit(self._lock);
            }
        }
    }

    /// <summary>
    /// Byte provider for a small amount of data.
    /// 
    /// </summary>
    public class FixedByteProvider : IByteProvider
    {
        /// <summary>
        /// Contains information about changes.
        /// 
        /// </summary>
        private bool _hasChanges;

        /// <summary>
        /// Contains a byte collection.
        /// 
        /// </summary>
        private List<byte> _bytes;

        /// <summary>
        /// Gets the byte collection.
        /// 
        /// </summary>
        public List<byte> Bytes
        {
            get { return this._bytes; }
        }

        /// <summary>
        /// Gets the length of the bytes in the byte collection.
        /// 
        /// </summary>
        public long Length
        {
            get { return (long)this._bytes.Count; }
        }

        /// <summary>
        /// Occurs, when the write buffer contains new changes.
        /// 
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Occurs, when InsertBytes or DeleteBytes method is called.
        /// 
        /// </summary>
        public event EventHandler LengthChanged;

        /// <summary>
        /// Initializes a new instance of the DynamicByteProvider class.
        /// 
        /// </summary>
        /// <param name="data"/>
        public FixedByteProvider(byte[] data)
            : this(new List<byte>((IEnumerable<byte>)data))
        {
        }

        /// <summary>
        /// Initializes a new instance of the DynamicByteProvider class.
        /// 
        /// </summary>
        /// <param name="bytes"/>
        public FixedByteProvider(List<byte> bytes)
        {
            this._bytes = bytes;
        }

        /// <summary>
        /// Raises the Changed event.
        /// 
        /// </summary>
        private void OnChanged(EventArgs e)
        {
            _hasChanges = true;
            Changed?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the LengthChanged event.
        /// 
        /// </summary>
        private void OnLengthChanged(EventArgs e)
        {
            LengthChanged?.Invoke(this, e);
        }

        /// <summary>
        /// True, when changes are done.
        /// 
        /// </summary>
        public bool HasChanges()
        {
            return this._hasChanges;
        }

        /// <summary>
        /// Applies changes.
        /// 
        /// </summary>
        public void ApplyChanges()
        {
            this._hasChanges = false;
        }

        /// <summary>
        /// Reads a byte from the byte collection.
        /// 
        /// </summary>
        /// <param name="index">the index of the byte to read</param>
        /// <returns>
        /// the byte
        /// </returns>
        public byte ReadByte(long index)
        {
            return this._bytes[(int)index];
        }

        /// <summary>
        /// Write a byte into the byte collection.
        /// 
        /// </summary>
        /// <param name="index">the index of the byte to write.</param><param name="value">the byte</param>
        public void WriteByte(long index, byte value)
        {
            this._bytes[(int)index] = value;
            this.OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes bytes from the byte collection.
        /// 
        /// </summary>
        /// <param name="index">the start index of the bytes to delete.</param><param name="length">the length of bytes to delete.</param>
        public void DeleteBytes(long index, long length)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Inserts byte into the byte collection.
        /// 
        /// </summary>
        /// <param name="index">the start index of the bytes in the byte collection</param><param name="bs">the byte array to insert</param>
        public void InsertBytes(long index, byte[] bs)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns true
        /// 
        /// </summary>
        public bool SupportsWriteByte()
        {
            return true;
        }

        /// <summary>
        /// Returns true
        /// 
        /// </summary>
        public bool SupportsInsertBytes()
        {
            return false;
        }

        /// <summary>
        /// Returns true
        /// 
        /// </summary>
        public bool SupportsDeleteBytes()
        {
            return false;
        }
    }
}