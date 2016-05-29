using System;
using System.Collections.Generic;

namespace VroomJs
{
    public class KeepAliveDictionaryStore : IKeepAliveStore
    {
        Dictionary<int, object> _store = new Dictionary<int, object>();
        int _store_index = 1;

        public int MaxSlots
        {
            get { return int.MaxValue; }
        }

        public int AllocatedSlots
        {
            get { return _store.Count; }
        }

        public int UsedSlots
        {
            get { return _store.Count; }
        }

        public int Add(object obj)
        {
            _store.Add(_store_index, obj);
            return _store_index++;
        }

        public object Get(int slot)
        {
            object obj;
            if (_store.TryGetValue(slot, out obj))
                return obj;
            return null;
        }

        public void Remove(int slot)
        {
            var obj = Get(slot);
            if (obj != null)
            {
                var disposable = obj as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                _store.Remove(slot);
            }
        }

        public void Clear()
        {
            _store.Clear();
        }
    }
}
