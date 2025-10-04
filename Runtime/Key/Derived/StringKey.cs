using System;
using UnityEngine;

namespace StatController.Runtime
{
    [Serializable]
    public struct StringKey : IStatKey<string>
    {
        private const int _EQUAL_VALUE = 0;
        
        private object _boxedKeyObject;
        
        public string key;
        
        
        object IStatKey.boxedKeyObject
        {
            get => _boxedKeyObject;
            set => _boxedKeyObject = value;
        }

        string IStatKey<string>.key
        {
            get => key;
            set => key = value;
        }
        
        
        public bool Equals(IStatKey other)
        {
            if (other is not StringKey)
            {
                return false;
            } 
            
            string stringKey = ((StringKey)other).key;
            int result = string.Compare(stringKey, this.key);
            
            return result == _EQUAL_VALUE;
        }
    }
}