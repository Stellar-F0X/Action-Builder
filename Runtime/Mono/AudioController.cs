using System.Collections.Generic;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    public class AudioController : MonoBehaviour
    {
        private readonly List<AudioSource> _availableAudioSources = new List<AudioSource>();

        
        private readonly List<AudioSource> _usedAudioSources = new List<AudioSource>();
        
        
        [Header("Audio Sources Pool"), SerializeField]
        private int _initialPoolSize = 5;

        
        [SerializeField]
        private int _maxPoolSize = 20;




        private void Awake()
        {
            this.InitializePool();
        }

        
        /// <summary>
        /// 오디오 소스 풀 초기화
        /// </summary>
        private void InitializePool()
        {
            _availableAudioSources.Clear();
            
            _usedAudioSources.Clear();

            
            for (int i = 0; i < _initialPoolSize; i++)
            {
                AudioSource audioSource = this.CreateNewAudioSource();
                
                _availableAudioSources.Add(audioSource);
            }
        }
        

        /// <summary>
        /// 새로운 AudioSource 생성
        /// </summary>
        private AudioSource CreateNewAudioSource()
        {
            GameObject audioObject = new GameObject("PooledAudioSource");
            audioObject.transform.SetParent(this.transform);

            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            return audioSource;
        }

        
        /// <summary>
        /// 사용 가능한 AudioSource 하나 가져오기
        /// </summary>
        public AudioSource GetAudioSourceFromPool()
        {
            AudioSource audioSource = null;

            if (_availableAudioSources.Count > 0)
            {
                audioSource = _availableAudioSources[0];
                _availableAudioSources.RemoveAt(0);
            }
            else if (_usedAudioSources.Count + _availableAudioSources.Count < _maxPoolSize)
            {
                audioSource = this.CreateNewAudioSource();
            }
            else
            {
                Debug.LogWarning("[AudioController] Audio source pool is full. Reusing oldest source.");
                audioSource = _usedAudioSources[0];
                _usedAudioSources.RemoveAt(0);
                audioSource.Stop();
            }

            if (audioSource != null)
            {
                _usedAudioSources.Add(audioSource);
            }

            return audioSource;
        }
        

        /// <summary>
        /// 여러 개의 AudioSource 가져오기
        /// </summary>
        public AudioSource[] GetAudioSources(int count)
        {
            if (count <= 0)
            {
                return new AudioSource[0];
            }

            AudioSource[] audioSources = new AudioSource[count];

            for (int i = 0; i < count; i++)
            {
                audioSources[i] = this.GetAudioSourceFromPool();
            }

            return audioSources;
        }
        

        /// <summary>
        /// AudioSource 사용 완료 후 풀에 반환
        /// </summary>
        public void ReturnAudioSource(AudioSource audioSource)
        {
            if (audioSource == null)
            {
                return;
            }

            if (_usedAudioSources.Contains(audioSource) == false)
            {
                return;
            }

            audioSource.Stop();
            audioSource.clip = null;
            audioSource.volume = 1f;

            _usedAudioSources.Remove(audioSource);
            _availableAudioSources.Add(audioSource);
        }
        

        /// <summary>
        /// 여러 AudioSource들을 풀에 반환
        /// </summary>
        public void ReturnToPool(AudioSource[] audioSources)
        {
            if (audioSources == null)
            {
                return;
            }

            for (int i = 0; i < audioSources.Length; i++)
            {
                this.ReturnAudioSource(audioSources[i]);
            }
        }

        
        /// <summary>
        /// 모든 사용 중인 AudioSource 정지 및 반환
        /// </summary>
        public void StopAllAndReturn()
        {
            for (int i = _usedAudioSources.Count - 1; i >= 0; i--)
            {
                AudioSource audioSource = _usedAudioSources[i];
                this.ReturnAudioSource(audioSource);
            }
        }


        private void OnDestroy()
        {
            this.StopAllAndReturn();
        }
    }
}