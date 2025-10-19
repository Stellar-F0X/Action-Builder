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
}