using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public struct SoundData
    {
        [Header("Volume Settings"), Range(0f, 1f)]
        public float volume;

        
        [Range(0f, 1f)]
        public float volumeVariation;

        
        /// <summary>  랜덤화된 볼륨 반환 </summary>
        public readonly float GetRandomVolume()
        {
            if (volumeVariation <= 0.001f)
            {
                return volume;
            }
            else
            {
                return Mathf.Clamp01(volume + Random.Range(-volumeVariation, volumeVariation));
            }
        }
    }
}