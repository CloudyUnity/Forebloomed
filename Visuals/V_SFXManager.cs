using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_SFXManager : MonoBehaviour
{
    [SerializeField] List<SFX> _allSFX = new List<SFX>();
    [SerializeField] AudioSource _sourceMain;
    [SerializeField] AudioSource _sourceAlt;
    [SerializeField] AudioSource _musicPlayer;
    [SerializeField] AudioSource _musicNight;
    [SerializeField] AudioSource _musicPaused;
    [SerializeField] AudioSource _ambience;

    [SerializeField] AudioClip _bossIntro;
    [SerializeField] AudioClip _bossOutro;
    [SerializeField] float _bar;
    AudioSource _currentSource;

    float _goldPitchCounter;
    float _goldPitchDropOff;

    bool _canChangeMusic = true;
    bool _pausedLastFrame;

    bool _outroDone;

    Dictionary<string, float> _lastTimePlayed = new Dictionary<string, float>();

    bool _startedSong;
    bool _lockVolumes;

    KeyCode _skip;

    static bool _triedToUnlockMusical;

    [SerializeField] float _loopSongAfterTime;

    private void OnEnable()
    {
        A_EventManager.OnPlaySFX += PlaySFX;
        A_EventManager.OnLowerMusic += LowerMusicVolume;
        A_EventManager.OnBossDie += BossOutro;
    }
    private void OnDisable()
    {
        A_EventManager.OnPlaySFX -= PlaySFX;
        A_EventManager.OnLowerMusic -= LowerMusicVolume;
        A_EventManager.OnBossDie -= BossOutro;
    }

    private void Start()
    {
        _currentSource = _musicPlayer;
        _skip = A_InputManager.Instance != null ? A_InputManager.Instance.Key("Interact") : KeyCode.E;
    }

    private void Update()
    {
        if (Player.Instance != null && !_musicPlayer.isPlaying && _musicPlayer.enabled && !_startedSong)
            StartCoroutine(C_RaiseMusic());
        else if (A_LevelManager.Instance.CurrentLevel < 0 && !_musicPlayer.isPlaying && _musicPlayer.enabled && !_startedSong)
            StartCoroutine(C_RaiseMusic());

        _goldPitchDropOff += Time.deltaTime;
        if (_goldPitchDropOff > 1)
            _goldPitchCounter = 1;

        if (A_TimeManager.Instance != null && A_TimeManager.Instance.TimePercent > 1 && _musicPlayer.enabled)
        {
            _musicPlayer.enabled = false;
            _musicNight.Play();
            _currentSource = _musicNight;
        }

        if (_loopSongAfterTime != 0 && _musicPlayer.time > _loopSongAfterTime)
        {
            _musicPlayer.Stop();
            _musicPlayer.Play();
        }

        float volM = A_OptionsManager.Instance.Current.MusicVolume * 1.1f;
        float volS = A_OptionsManager.Instance.Current.SFXVolume * 0.4f;
        //Debug.Log(_lockVolumes + " " + _canChangeMusic + " " + volM + " " + volS);
        if (!_lockVolumes)
        {
            if (_canChangeMusic || A_LevelManager.Instance.CurrentLevel == 24)
            {
                _musicPaused.volume = volM;
                _musicPlayer.volume = A_LevelManager.Instance.CurrentLevel == 24 ? 0 : volM;
                _musicNight.volume = volM;
                if (Time.timeScale == 0 || (A_LevelManager.Instance.BossLevel && _musicPlayer.enabled))
                {
                    _ambience.volume = 0;
                }                    
                else
                    _ambience.volume = volS * 0.17f;                

                if (A_LevelManager.Instance.CurrentLevel != 24)
                {
                    _musicPlayer.pitch = 1;
                    _musicPaused.pitch = 1;
                    _musicNight.pitch = 1;
                    _ambience.pitch = 1;
                }
            }
            
            _sourceMain.volume = volS;
            _sourceAlt.volume = volS;
        }

        if (A_OptionsManager.Instance.Current.MusicVolume >= 1 && !_triedToUnlockMusical)
        {
            A_EventManager.InvokeUnlock("Musical");
            _triedToUnlockMusical = true;
        }

        if (V_HUDManager.Instance == null)
            return;

        if (V_HUDManager.Instance.IsPaused && !_pausedLastFrame)
        {
            _currentSource.Pause();
            _musicPaused.Play();
        }
        else if (!V_HUDManager.Instance.IsPaused && _pausedLastFrame)
        {
            if (!_outroDone)
                _currentSource.Play();
            _musicPaused.Stop();
        }


        if (_currentSource.isPlaying && _musicPaused.isPlaying)
        {
            if (V_HUDManager.Instance.IsPaused)
                _currentSource.Pause();
            else
                _musicPaused.Stop();
        }            

        _pausedLastFrame = V_HUDManager.Instance.IsPaused;

        if (Time.timeScale == 1 && !V_HUDManager.Instance.IsPaused && A_LevelManager.Instance != null && 
            A_LevelManager.Instance.StoppedTimeTotal > 200 && !_changingSpeed)
        {
            StartCoroutine(C_ChangeSpeed(A_LevelManager.Instance.StoppedTimeTotal / 1000));
            A_LevelManager.Instance.StoppedTimeTotal = 0;
        }

        if (!_changingSpeed && A_TimeManager.Instance != null && A_TimeManager.Instance.TimeMult != 0)
        {
            _musicPlayer.pitch = Mathf.Clamp(A_TimeManager.Instance.TimeMult, -3, 3);
        }
    }

    bool _changingSpeed;
    IEnumerator C_ChangeSpeed(float time)
    {
        if (A_TimeManager.Instance == null)
            yield break;

        _changingSpeed = true;
        float speed = 1.01f;
        float dur = time / (speed - 1);
        float elapsed = 0;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            _musicPlayer.pitch = speed * Mathf.Clamp(A_TimeManager.Instance.TimeMult, -3, 3);   
            yield return null;
        }

        _changingSpeed = false;
    }

    bool _introPlayed;
    void PlaySFX(string name)
    {
        //Debug.Log("SFX: " + name);

        if (string.IsNullOrEmpty(name))
            return;

        if (name == "BossIntro")
        {
            if (_introPlayed)
            {
                Debug.LogWarning("Intro played twice!!! WTF");
                return;
            }

            StartCoroutine(C_BossIntro());
            _introPlayed = true;
            return;
        }

        if (A_OptionsManager.Instance.Current.SFXVolume <= 0)
            return;

        if (_lastTimePlayed != null && _lastTimePlayed.ContainsKey(name) && Time.unscaledTime - _lastTimePlayed[name] < 0.05f)
            return;

        foreach (SFX sfx in _allSFX)
        {
            if (sfx.Name == name)
            {
                AudioClip clip = sfx.Clips[Random.Range(0, sfx.Clips.Length)];
                AudioSource source = GetSource(name);
                source.pitch = GetPitch(name);
                source.PlayOneShot(clip);
                //Debug.Log("Played " + name);

                if (_lastTimePlayed.ContainsKey(name))
                    _lastTimePlayed[name] = Time.unscaledTime;
                else
                    _lastTimePlayed.Add(name, Time.unscaledTime);

                return;
            }
        }

        Debug.Log("SFX " + name + " is not found");
    }

    float GetPitch(string name)
    {
        if (name.Is("Shoot", "Enemy Shoot", "Laser")) 
            return Random.Range(1f, 1.1f);

        if (name.Is("Scolop", "Speak"))
            return Random.Range(0.5f, 2f);

        if (name.Is("Hurt", "EnemyDie", "Dig", "Wall", "Poison", "Ice", "Brittle", "Heal", "Recharge", "Bot Die", "HoverMenu", "Step2", "Step1", "Step3")) 
            return Random.Range(0.9f, 1.1f);

        if (name == "Gold")
        {
            _goldPitchCounter += 0.2f;
            _goldPitchCounter = Mathf.Clamp(_goldPitchCounter, 1, 3);
            _goldPitchDropOff = 0;
            return _goldPitchCounter;
        }

        return 1;
    }

    AudioSource GetSource(string name)
    {
        if (name.Is("Day End", "Game Start", "Trans", "Detrans", "Exit Open", "Exit Enter"))
            return _sourceAlt;

        return _sourceMain;
    }

    public static readonly bool RAISE_DISABLED = true;
    IEnumerator C_RaiseMusic()
    {
        if (_startedSong)
            yield break;

        if (RAISE_DISABLED)
        {
            float volM = A_OptionsManager.Instance.Current.MusicVolume * 1.1f;
            float volS = A_OptionsManager.Instance.Current.SFXVolume * 0.4f;
            _musicPaused.volume = volM;
            _musicPlayer.volume = A_LevelManager.Instance.CurrentLevel == 24 ? 0 : volM;
            _musicNight.volume = volM;
            if (Time.timeScale == 0 || (A_LevelManager.Instance.BossLevel && _musicPlayer.enabled))
            {
                _ambience.volume = 0;
            }
            else
                _ambience.volume = volS * 0.17f;

            if (A_LevelManager.Instance.CurrentLevel != 24)
            {
                _musicPlayer.pitch = 1;
                _musicPaused.pitch = 1;
                _musicNight.pitch = 1;
                _ambience.pitch = 1;
            }
            _sourceMain.volume = volS;
            _sourceAlt.volume = volS;

            _musicPlayer.Play();
            _canChangeMusic = true;
            _startedSong = true;
            yield break;
        }            

        float elapsed = 0;
        float dur = 0.5f;

        _musicPlayer.Play();
        _startedSong = true;

        while (elapsed < dur)
        {
            _musicPlayer.volume = A_OptionsManager.Instance.Current.MusicVolume * 1.1f * Mathf.Lerp(0, 1, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canChangeMusic = true;
    }

    void LowerMusicVolume(float dur, bool reset) => StartCoroutine(C_LowerMusic(dur, reset));

    IEnumerator C_LowerMusic(float dur, bool resetAfter)
    {
        if (_lockVolumes)
            yield break;

        _canChangeMusic = false;

        float elapsed = 0;

        while (elapsed < dur)
        {
            _musicPlayer.volume = A_OptionsManager.Instance.Current.MusicVolume * 1.1f * Mathf.Lerp(1, 0, elapsed / dur);
            _musicNight.volume = A_OptionsManager.Instance.Current.MusicVolume * 1.1f * Mathf.Lerp(1, 0, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (resetAfter)
        {
            _canChangeMusic = true;
            yield break;
        }

        _lockVolumes = true;
        _musicPlayer.volume = 0;
        _musicNight.volume = 0;
    }

    IEnumerator C_BossIntro()
    {
        _ambience.volume = 0;
        AudioClip song = _musicPlayer.clip;
        _musicPlayer.clip = _bossIntro;
        _musicPlayer.loop = false;
        _musicPlayer.Play();
        while (!Input.GetKeyDown(_skip) && _musicPlayer.time < 8.5f && _musicPlayer.isPlaying)
        {
            yield return null;
        }
        _musicPlayer.Stop();
        _musicPlayer.loop = true;
        _musicPlayer.clip = song;
        _musicPlayer.Play();
    }

    void BossOutro(EBoss _) => StartCoroutine(C_BossOutro());

    IEnumerator C_BossOutro()
    {
        if (_bar == 0 || !_musicPlayer.loop || _outroDone)
            yield break;

        Debug.Log("Playing Outro");

        //const float bar = 1.69014084507f;
        float time = _musicPlayer.time;
        float remaining = _bar - (time % _bar);
        yield return new WaitForSeconds(remaining);
        _musicPlayer.Stop();
        _musicPlayer.clip = _bossOutro;
        _musicPlayer.loop = false;
        _musicPlayer.Play();
        _outroDone = true;

        float volS = A_OptionsManager.Instance.Current.SFXVolume * 0.4f;
        _ambience.volume = volS * 0.17f;

        yield return new WaitForSecondsRealtime(6f);
        _musicPlayer.enabled = false;
    }
}

[System.Serializable]
public struct SFX
{
    public string Name;
    public AudioClip[] Clips;
}
