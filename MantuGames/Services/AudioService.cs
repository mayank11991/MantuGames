using Plugin.Maui.Audio;

namespace MantuGames.Services;

public class AudioService
{
    public static AudioService Instance { get; private set; }

    private readonly IAudioManager _audio;
    private readonly Dictionary<string, IAudioPlayer?> _cache = new();
    private IAudioPlayer _bgPlayer;
    private bool _musicEnabled = true;
    private bool _sfxEnabled = true;

    public AudioService(IAudioManager audio)
    {
        _audio = audio;
        Instance = this;
        _musicEnabled = Preferences.Get("music_enabled", true);
        _sfxEnabled = Preferences.Get("sfx_enabled", true);
    }

    public async Task PreloadAsync()
    {
        string[] sounds = ["tap", "win", "lose", "correct", "wrong", "pop", "star",
                           "move", "bump", "slide", "clear", "swoosh", "countdown", "wobble", "fall", "thud","whoosh"];
        foreach (var name in sounds)
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync($"{name}.wav");
                _cache[name] = _audio.CreatePlayer(stream);
            }
            catch { _cache[name] = null; }
        }

        try
        {
            var bgStream = await FileSystem.OpenAppPackageFileAsync("bg_new.mp3");
            _bgPlayer = _audio.CreatePlayer(bgStream);
            _bgPlayer.Loop = true;
            _bgPlayer.Volume = 0.5; // background track — keep it decently low
            _bgPlayer.PlaybackEnded += OnBgPlaybackEnded;
        }
        catch
        {
            // Fall back to the legacy track if the new one is missing
            try
            {
                var bgStream = await FileSystem.OpenAppPackageFileAsync("bgmusic.wav");
                _bgPlayer = _audio.CreatePlayer(bgStream);
                _bgPlayer.Loop = true;
                _bgPlayer.PlaybackEnded += OnBgPlaybackEnded;
            }
            catch { }
        }
    }

    private void OnBgPlaybackEnded(object sender, EventArgs e)
    {
        if (_musicEnabled && _bgPlayer != null)
        {
            try
            {
                _bgPlayer.Seek(0);
                _bgPlayer.Play();
            }
            catch { }
        }
    }

    public void Play(string name)
    {
        if (!_sfxEnabled) return;
        try
        {
            if (_cache.TryGetValue(name, out var player) && player != null)
            {
                player.Seek(0);
                player.Play();
            }
        }
        catch { }
    }

    public bool SfxEnabled
    {
        get => _sfxEnabled;
        set
        {
            _sfxEnabled = value;
            Preferences.Set("sfx_enabled", value);
        }
    }

    public bool MusicEnabled
    {
        get => _musicEnabled;
        set
        {
            _musicEnabled = value;
            Preferences.Set("music_enabled", value);
            if (!value)
                StopMusic();
            else
                StartMusic();
        }
    }

    public void StartMusic()
    {
        if (!_musicEnabled || _bgPlayer == null) return;
        try
        {
            if (!_bgPlayer.IsPlaying)
                _bgPlayer.Play();
        }
        catch { }
    }

    public void StopMusic()
    {
        try
        {
            if (_bgPlayer != null && _bgPlayer.IsPlaying)
            {
                _bgPlayer.Stop();
                _bgPlayer.Seek(0);
            }
        }
        catch { }
    }

    public void StopAll()
    {
        StopMusic();
        foreach (var kv in _cache)
        {
            try { kv.Value?.Stop(); }
            catch { }
        }
    }
}
