namespace ExtendedSystemObjects.Helper
{
    public unsafe struct EntryEnumerator
    {
        private readonly Entry* _entries;
        private readonly int _capacity;
        private int _index;

        public EntryEnumerator(Entry* entries, int capacity)
        {
            _entries = entries;
            _capacity = capacity;
            _index = -1;
        }

        public bool MoveNext()
        {
            while (++_index < _capacity)
            {
                if (_entries[_index].Used == SharedResources.Occupied)
                {
                    return true;
                }
            }

            return false;
        }

        public (int Key, int Value) Current => (_entries[_index].Key, _entries[_index].Value);
    }
}
