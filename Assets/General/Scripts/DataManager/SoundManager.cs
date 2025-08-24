using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManger : Singleton<SoundManger>
{
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public class SoundManager : Singleton<SoundManager>
    {
        AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount];
        Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

        void Start()
        {
            for (int i = 0; i < (int)Sound.MaxCount; i++)
            {
                GameObject go = new GameObject($"{(Sound)i} AudioSource");
                go.transform.parent = transform;
                _audioSources[i] = go.AddComponent<AudioSource>();
            }

            // BGM은 루프 기본 설정
            _audioSources[(int)Sound.Bgm].loop = true;
        }

        public void PlayBgm(string clipName) //같은 오디오소스에서 관리되니까 새 bgm가 들어오면 자동으로 끊기고 새 bgm으로 교체됨. 
        {
            AudioClip clip = GetOrAddClip(clipName);
            if (clip == null) return;

            var source = _audioSources[(int)Sound.Bgm];
            source.clip = clip;
            source.Play();
        }

        public void PlaySfx(string clipName)
        {
            AudioClip clip = GetOrAddClip(clipName);
            if (clip == null) return;

            var source = _audioSources[(int)Sound.Effect];
            source.PlayOneShot(clip); //효과음은 한 번만 실행하세요.
        }

        AudioClip GetOrAddClip(string clipName)
        {
            if (_audioClips.TryGetValue(clipName, out AudioClip clip))
                return clip;

            clip = Resources.Load<AudioClip>($"Sounds/{clipName}");
            if (clip != null)
                _audioClips.Add(clipName, clip);

            return clip;
        }
    }

}
