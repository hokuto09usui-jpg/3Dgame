
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("フェード設定")]
    [SerializeField] private CanvasGroup fadeGroup;

    [SerializeField, Min(0f)]
    private float fadeDuration = 0.5f;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        // 二重生成を防止
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーン移動後も保持する
        DontDestroyOnLoad(gameObject);

        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = false;
    }

    private IEnumerator Start()
    {
        // ゲーム起動時のフェードイン
        isTransitioning = true;

        yield return Fade(0f);

        fadeGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    public void LoadScene(string sceneName)
    {
        // ボタンの連打による二重遷移を防止
        if (isTransitioning)
            return;

        // シーンが登録されているか確認
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("シーンが見つかりません：" + sceneName);
            return;
        }

        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        // 遷移中のUI操作を禁止
        fadeGroup.blocksRaycasts = true;

        // フェードアウト
        yield return Fade(1f);

        // ポーズ中でも正常に移動できるようにする
        Time.timeScale = 1f;

        // シーン読み込み
        yield return SceneManager.LoadSceneAsync(sceneName);

        // フェードイン
        yield return Fade(0f);

        fadeGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);

            fadeGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                t
            );

            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
