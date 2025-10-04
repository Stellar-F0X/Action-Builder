using System;
using StatController.Runtime;
using UnityEngine;

namespace StatController.Sample
{
    [Serializable]
    public struct EnumKey : IStatKey<EStat>
    {
        [SerializeField]
        private EStat _key;

        
        EStat IStatKey<EStat>.key
        {
            get { return _key; }
            set { _key = value; }
        }

        public object boxedKeyObject
        {
            get { return _key; }
        }

        
        public IStatKey Clone()
        {
            EnumKey newKey = new EnumKey();
            newKey._key = this._key;
            return newKey;
        }
        

        public bool Equals(IStatKey other)
        {
            if (other is not EnumKey enumKey)
            {
                return false;
            }
            else
            {
                return this._key == enumKey._key;
            }
        }
    }
}