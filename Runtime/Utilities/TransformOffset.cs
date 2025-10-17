using System;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct TransformOffset
    {
        public TransformOffset(Vector3 sizeOffset, Vector3 positionOffset, Vector3 rotationOffset)
        {
            this.positionOffset = positionOffset;
            this.rotationOffset = rotationOffset;
            this.sizeOffset = sizeOffset;
        }

        public Vector3 sizeOffset;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }
}