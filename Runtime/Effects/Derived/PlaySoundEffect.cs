using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class PlaySoundEffect : EffectBase
    {
        [Header("Audio Settings")]
        public AudioPlayType playType = AudioPlayType.RandomSingle;
        public AudioClip[] audioClips;

        private AudioController _audioController;
        private AudioSource[] _currentAudioSources;
    }
}