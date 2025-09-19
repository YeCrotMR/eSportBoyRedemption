using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggleController : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Toggle muteToggle;

    private const string FULLSCREEN_PREF_KEY = "IsFullscreen";
    private const string MUTE_PREF_KEY = "IsMuted";

    public void Start()
    {
        // ========== 全屏设置 ==========
        bool savedFullscreen = PlayerPrefs.GetInt(FULLSCREEN_PREF_KEY, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.isOn = savedFullscreen;
        ApplyFullscreenSetting(savedFullscreen);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

        // ========== 静音设置 ==========
        bool savedMuted = PlayerPrefs.GetInt(MUTE_PREF_KEY, 0) == 1;
        muteToggle.isOn = savedMuted;
        ApplyMuteSetting(savedMuted);
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
    }

    public void OnFullscreenToggleChanged(bool isFullscreen)
    {
        ApplyFullscreenSetting(isFullscreen);
        PlayerPrefs.SetInt(FULLSCREEN_PREF_KEY, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        //Debug.Log("卧槽，全屏了");
    }

    public void OnMuteToggleChanged(bool isMuted)
    {
        ApplyMuteSetting(isMuted);
        PlayerPrefs.SetInt(MUTE_PREF_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("卧槽，没声音了");
    }

    public void ApplyFullscreenSetting(bool isFullscreen)
    {
        if (isFullscreen)
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, FullScreenMode.FullScreenWindow);
            //Debug.Log("卧槽，真全屏了");
        }
        else
        {
            Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
        }
    }

    public void ApplyMuteSetting(bool isMuted)
{
    if (isMuted)
    {
        AudioListener.pause = true; // 暂停全局音频
    }
    else
    {
        AudioListener.pause = false;
    }
}

}
