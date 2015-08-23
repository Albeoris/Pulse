using System;
using System.Collections.Generic;
using Pulse.Core;

namespace Pulse.UI
{
    public class InfoProviderGroup<T> : List<IInfoProvider<T>>, IInfoProvider<T> where T : class
    {
        public string Title { get; private set; }
        public string Description { get; private set; }

        private readonly object _lock = new object();
        private T _current;

        public event Action<T> InfoProvided;
        public event Action InfoLost;

        public InfoProviderGroup(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public T Provide()
        {
            lock (_lock)
            {
                if (_current != null)
                    return _current;

                List<Exception> exceptions = new List<Exception>();
                foreach (IInfoProvider<T> provider in this)
                {
                    try
                    {
                        SetValue(provider.Provide());
                        return _current;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
                throw new AggregateException(exceptions);
            }
        }

        public T Refresh(IInfoProvider<T> provider)
        {
            lock (_lock)
            {
                ClearValue();
                T result = provider.Provide();

                if (Count == 0)
                {
                    Add(provider);
                }
                else if (this[0] != provider)
                {
                    Remove(provider);
                    Insert(0, provider);
                }

                return SetValue(result);
            }
        }

        public T Refresh()
        {
            lock (_lock)
            {
                ClearValue();
                return Provide();
            }
        }

        private void ClearValue()
        {
            _current = null;
            InfoLost.NullSafeInvoke();
        }

        public T SetValue(T value)
        {
            _current = value;
            InfoProvided.NullSafeInvoke(value);
            return value;
        }
    }
}