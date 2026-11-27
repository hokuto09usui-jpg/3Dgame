

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private void Start()
    {
        // マウスカーソルを表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
    }

    // ゲームスタート
    public void OnStartButton()
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene("StageSelect");
        }
        else
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    // 設定画面
    public void OnSettingsButton()
    {
        // 設定機能は後から実装する
        Debug.Log("設定ボタンが押されました");
    }

    // ゲーム終了
    public void OnExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
