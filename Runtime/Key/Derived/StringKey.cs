using System;

namespace StatController.Runtime
{
    [Serializable]
    public struct StringKey : IStatKey<string>
    {
        private const int _EQUAL_VALUE = 0;
        
        private object _boxedKeyObject;
        
        private string _key;
        
        
        object IStatKey.boxedKeyObject
        {
            get => _boxedKeyObject;
            set => _boxedKeyObject = value;
        }

        string IStatKey<string>.key
        {
            get => _key;
            set => _key = value;
        }
        
        
        public bool Equals(IStatKey other)
        {
            if (other is not StringKey)
            {
                return false;
            } 
            
            string stringKey = ((StringKey)other)._key;
            int result = string.Compare(stringKey, this._key);
            
            return result == _EQUAL_VALUE;
        }
    }
}