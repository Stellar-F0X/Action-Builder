using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct TransformOffset
    {
        public TransformOffset(Vector3 size, Vector3 positionOffset, Vector3 rotationOffset)
        {
            this.positionOffset = positionOffset;
            this.rotationOffset = rotationOffset;
            this.size = size;
        }

        public Vector3 size;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }
    
    
    [Serializable]
    public struct MinMax
    {
        public MinMax(float min, float max)
        {
            this._min = min;
            this._max = max;
        }

        private float _min;
        private float _max;

        
        public float min
        {
            get { return _min;}
            set { _min = value; }
        }
        
        public float max
        {
            get { return _max;}
            set { _max = value; }
        }

        
        public override string ToString()
        {
            return $"{_min} {_min}";
        }
    }
}