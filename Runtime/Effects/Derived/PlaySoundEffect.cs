using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ActionBuilder.Runtime
{
    [Serializable]
    public class PlaySoundEffect : EffectBase
    {
        [Header("Audio Clips")]
        public AudioClip[] audioClips;

        [Header("Sound Configuration")]
        public SoundData soundData;

        [Header("Play Settings")]
        public AudioPlayType playType = AudioPlayType.RandomSingle;

        [Header("Play Timing")]
        public SoundPlayMode playMode = SoundPlayMode.PlayOnApply;

        [Header("Control")]
        public bool stopOnRelease = true;

        public bool stopOnActionEnd ;
        
        private bool _isPlaying;
        
        private AudioController _audioController;
        
        private AudioSource[] _currentAudioSources;

        
        
        public override void OnApply()
        {
            if ((playMode & SoundPlayMode.PlayOnApply) == 0)
            {
                return;
            }

            this.PlaySound();
        }

        
        public override void OnActionStart()
        {
            if ((playMode & SoundPlayMode.PlayOnActionStart) == 0)
            {
                return;
            }

            this.PlaySound();
        }

        
        public override void OnActionEnd()
        {
            if ((playMode & SoundPlayMode.PlayOnActionEnd) != 0)
            {
                this.PlaySound();
                return;
            }

            if (stopOnActionEnd && stopOnRelease && _isPlaying)
            {
                this.StopSound();
            }
            else if (stopOnActionEnd == false && _isPlaying)
            {
                this.DetachFromEffect();
            }
        }
        

        public override void OnRelease()
        {
            if (stopOnRelease == false || _isPlaying == false)
            {
                return;
            }

            this.StopSound();
        }

        
        /// <summary>
        /// AudioController 설정
        /// </summary>
        private void SetupAudioController()
        {
            if (_audioController != null)
            {
                return;
            }

            if (gameObject == null)
            {
                return;
            }

            _audioController = gameObject.GetComponentInChildren<AudioController>();

            if (_audioController == null)
            {
                GameObject audioControllerObject = new GameObject("AudioController");
                audioControllerObject.transform.SetParent(gameObject.transform);
                _audioController = audioControllerObject.AddComponent<AudioController>();
            }
        }

        
        /// <summary>
        /// 사운드 재생
        /// </summary>
        public void PlaySound()
        {
            if (audioClips == null || audioClips.Length == 0)
            {
                Debug.LogWarning($"[PlaySoundEffect] No audio clips assigned to {effectName}");
                return;
            }

            this.SetupAudioController();

            if (_audioController == null)
            {
                Debug.LogError($"[PlaySoundEffect] Failed to setup AudioController for {effectName}");
                return;
            }

            // 기존에 재생 중이던 오디오 정리
            this.ReturnCurrentAudioSources();

            if (playType == AudioPlayType.RandomSingle)
            {
                this.PlayRandomSingle();
            }
            else
            {
                this.PlayAll();
            }

            _isPlaying = true;
        }

        
        /// <summary>
        /// 랜덤으로 하나의 클립만 재생
        /// </summary>
        private void PlayRandomSingle()
        {
            AudioClip clipToPlay = this.GetRandomClip();

            if (clipToPlay == null)
            {
                return;
            }

            AudioSource audioSource = _audioController.GetAudioSourceFromPool();

            if (audioSource == null)
            {
                return;
            }

            _currentAudioSources = new AudioSource[] { audioSource };

            audioSource.clip = clipToPlay;
            audioSource.volume = soundData.GetRandomVolume();
            audioSource.Play();
        }

        
        /// <summary>
        /// 모든 클립을 동시에 재생
        /// </summary>
        private void PlayAll()
        {
            AudioClip[] validClips = this.GetValidClips();

            if (validClips.Length == 0)
            {
                return;
            }

            AudioSource[] audioSources = _audioController.GetAudioSources(validClips.Length);

            if (audioSources == null || audioSources.Length == 0)
            {
                return;
            }

            _currentAudioSources = audioSources;

            for (int i = 0; i < validClips.Length && i < audioSources.Length; i++)
            {
                AudioSource audioSource = audioSources[i];

                if (audioSource != null)
                {
                    audioSource.clip = validClips[i];
                    audioSource.volume = soundData.GetRandomVolume();
                    audioSource.Play();
                }
            }
        }

        
        /// <summary>
        /// 사운드 정지
        /// </summary>
        public void StopSound()
        {
            if (_isPlaying == false)
            {
                return;
            }

            this.ReturnCurrentAudioSources();
            _isPlaying = false;
        }

        
        /// <summary>
        /// Effect에서 분리하여 독립적으로 재생 완료까지 관리
        /// </summary>
        private void DetachFromEffect()
        {
            if (_audioController == null || _currentAudioSources == null || _isPlaying == false)
            {
                return;
            }

            // AudioObserver 컴포넌트 추가하여 독립적으로 관리
            GameObject observerObject = new GameObject("AudioObserver");
            observerObject.transform.SetParent(_audioController.transform);

            AudioObserver observer = observerObject.AddComponent<AudioObserver>();
            observer.Initialize(this, _audioController, _currentAudioSources);

            // 현재 Effect에서는 더 이상 관리하지 않음
            _currentAudioSources = null;
            _isPlaying = false;
        }
        

        /// <summary>
        /// 현재 사용 중인 AudioSource들을 풀에 반환
        /// </summary>
        private void ReturnCurrentAudioSources()
        {
            if (_audioController == null || _currentAudioSources == null)
            {
                return;
            }

            _audioController.ReturnToPool(_currentAudioSources);
            _currentAudioSources = null;
        }

        
        /// <summary>
        /// 랜덤 AudioClip 반환
        /// </summary>
        private AudioClip GetRandomClip()
        {
            if (audioClips == null || audioClips.Length == 0)
            {
                return null;
            }

            return audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
        }
        

        /// <summary>
        /// 유효한 오디오 클립 목록 반환
        /// </summary>
        private AudioClip[] GetValidClips()
        {
            if (audioClips == null)
            {
                return new AudioClip[0];
            }

            return audioClips.Where(clip => clip != null).ToArray();
        }

        
        /// <summary>
        /// 독립 실행 중인 오디오가 종료되었을 때 호출됩니다
        /// </summary>
        internal void OnDetachedAudioFinished(AudioSource[] audioSources)
        {
            // 독립 실행이 완료되었을 때 처리할 로직이 있다면 여기에 추가
            Debug.Log($"[PlaySoundEffect] Detached audio finished for {effectName}");
        }

        
        public override void OnReset()
        {
            this.StopSound();
        }

        
        public override void OnValidateEffect()
        {
            base.OnValidateEffect();

            SoundData data = soundData;

            // 볼륨 검증
            if (soundData.volume < 0f)
            {
                data.volume = 0f;
            }
            else if (soundData.volume > 1f)
            {
                data.volume = 1f;
            }

            // 볼륨 변화량 검증
            if (soundData.volumeVariation < 0f)
            {
                data.volumeVariation = 0f;
            }
            else if (soundData.volumeVariation > 1f)
            {
                data.volumeVariation = 1f;
            }

            soundData = data;

            // 최소 하나의 플레이 모드는 설정되어야 함
            if (playMode != SoundPlayMode.None)
            {
                return;
            }

            playMode = SoundPlayMode.PlayOnApply;
            Debug.LogWarning($"[PlaySoundEffect] {effectName}: Play mode cannot be None. Setting to 'PlayOnApply'.");
        }
    }

    
    /// <summary>
    /// 독립 실행 오디오를 감시하는 MonoBehaviour 컴포넌트
    /// </summary>
    internal class AudioObserver : MonoBehaviour
    {
        private PlaySoundEffect _owner;
        private AudioController _audioController;
        private AudioSource[] _audioSources;
        private bool _hasFinished;

        /// <summary>
        /// 오디오 감시자를 초기화합니다
        /// </summary>
        public void Initialize(PlaySoundEffect owner, AudioController audioController, AudioSource[] audioSources)
        {
            _owner = owner;
            _audioController = audioController;
            _audioSources = audioSources;
            _hasFinished = false;

            this.StartCoroutine(this.WatchAudio());
        }
        

        /// <summary>
        /// 오디오가 끝날 때까지 감시합니다
        /// </summary>
        private IEnumerator WatchAudio()
        {
            while (this.IsAnyAudioPlaying())
            {
                yield return null;
            }

            this.OnAudioFinished();
        }

        
        /// <summary>
        /// 재생 중인 오디오가 있는지 확인
        /// </summary>
        private bool IsAnyAudioPlaying()
        {
            if (_audioSources == null)
            {
                return false;
            }

            for (int i = 0; i < _audioSources.Length; i++)
            {
                AudioSource audioSource = _audioSources[i];

                if (audioSource != null && audioSource.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }
        

        /// <summary>
        /// 오디오 재생 완료 처리
        /// </summary>
        private void OnAudioFinished()
        {
            if (_hasFinished)
            {
                return;
            }

            _hasFinished = true;

            // AudioController에 AudioSource들 반환
            if (_audioController != null && _audioSources != null)
            {
                _audioController.ReturnToPool(_audioSources);
            }

            // 소유자에게 완료 알림
            _owner?.OnDetachedAudioFinished(_audioSources);

            // 자기 자신 정리
            Destroy(this.gameObject);
        }
        
        
        private void OnDestroy()
        {
            if (_hasFinished || _audioController == null || _audioSources == null)
            {
                return;
            }

            _hasFinished = true;
            _audioController.ReturnToPool(_audioSources);
            _owner?.OnDetachedAudioFinished(_audioSources);
        }
    }
}