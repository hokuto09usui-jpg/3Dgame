
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("パネル")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("ステージ情報")]
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text timerText;

    [Header("スコア")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Slider scoreSlider;

    [Header("プレイヤー情報")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider guardSlider;

    [Header("ゲーム演出")]
    [SerializeField] private TMP_Text announcementText;

    [Header("仮リザルト")]
    [SerializeField] private TMP_Text resultText;

    private GameManager gameManager;

    public void Initialize(GameManager manager)
    {
        if (gameManager != null)
        {
            gameManager.OnStatusChanged -= Refresh;
        }

        gameManager = manager;

        StageData stage = manager.CurrentStage;

        stageNameText.text = stage.stageName;

        hpSlider.maxValue = manager.MaxHP;
        guardSlider.maxValue = manager.MaxGuard;

        // エンドレスでは目標スコアがない
        scoreSlider.gameObject.SetActive(
            !stage.isEndless
        );

        scoreSlider.maxValue = Mathf.Max(
            1, stage.targetScore
        );

        gameManager.OnStatusChanged += Refresh;

        Refresh();
        UpdateTimer(manager);
    }

    private void Refresh()
    {
        if (gameManager == null)
            return;

        StageData stage = gameManager.CurrentStage;

        // スコア表示
        if (stage.isEndless)
        {
            scoreText.text =
                "SCORE : " +
                gameManager.Score.ToString("N0");
        }
        else
        {
            scoreText.text =
                gameManager.Score.ToString("N0") +
                " / " +
                stage.targetScore.ToString("N0");

            scoreSlider.value = gameManager.Score;
        }

        // HP表示
        hpText.text =
            "HP " +
            gameManager.CurrentHP +
            " / " +
            gameManager.MaxHP;

        hpSlider.value = gameManager.CurrentHP;

        // ガードゲージ表示
        guardSlider.value = gameManager.CurrentGuard;
    }

    public void UpdateTimer(GameManager manager)
    {
        bool endless = manager.CurrentStage.isEndless;

        int seconds = endless
            ? Mathf.FloorToInt(manager.ElapsedTime)
            : Mathf.CeilToInt(manager.RemainingTime);

        seconds = Mathf.Max(0, seconds);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text =
            $"{minutes:00}:{remainingSeconds:00}";

        // 残り10秒で文字を赤くする
        timerText.color =
            !endless && seconds <= 10
            ? Color.red
            : Color.white;
    }

    public void SetHUDVisible(bool visible)
    {
        hudPanel.SetActive(visible);
    }

    public void SetTutorialVisible(bool visible)
    {
        tutorialPanel.SetActive(visible);
    }

    public void SetResultVisible(bool visible)
    {
        resultPanel.SetActive(visible);
    }

    public void SetAnnouncement(string message)
    {
        announcementText.text = message;

        announcementText.gameObject.SetActive(
            !string.IsNullOrEmpty(message)
        );
    }

    public void ShowResult(GameManager manager)
    {
        string result;

        if (manager.CurrentStage.isEndless)
        {
            result = "ENDLESS FINISH!";
        }
        else
        {
            result = manager.IsCleared
                ? "CLEAR!"
                : "FAILED...";
        }

        resultText.text =
            result +
            "\n\nSCORE\n" +
            manager.Score.ToString("N0") +
            " pt";

        SetResultVisible(true);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnStatusChanged -= Refresh;
        }
    }
}
