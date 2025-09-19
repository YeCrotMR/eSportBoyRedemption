using UnityEngine;

public class GameQuitter : MonoBehaviour
{
    // 退出游戏
    public void QuitGame()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        // 如果在编辑器中，停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 如果是打包后运行，退出应用
        Application.Quit();
#endif
    }
}
