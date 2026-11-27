
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class StageSelectManager : MonoBehaviour
{
    [Header("ステージデータ")]
    [SerializeField] private StageData[] stages;

    [Header("カード生成")]
    [SerializeField] private StageCardUI cardPrefab;
    [SerializeField] private RectTransform cardContainer;

    [Header("スクロール設定")]
    [SerializeField] private float cardSpacing = 400f;
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float normalScale = 0.85f;
    [SerializeField] private float normalAlpha = 0.6f;

    [Header("ステージ情報UI")]
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text targetScoreText;
    [SerializeField] private TMP_Text enemyRateText;

    [Header("ボタン")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button startButton;

    [Header("チュートリアル")]
    [SerializeField] private GameObject tutorialPanel;

    private readonly List<StageCardUI> cards = new();

    private int selectedIndex;
    private bool initialized;
    private bool isMoving;

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        tutorialPanel.SetActive(false);

        if (stages == null || stages.Length == 0)
        {
            Debug.LogError("ステージデータがありません");
            return;
        }

        selectedIndex = Mathf.Clamp(
            GameSession.SelectedStageIndex,
            0,
            stages.Length - 1
        );

        CreateCards();

        initialized = true;

        UpdateCards(true);
        UpdateStageInfo();
        UpdateButtons();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (SceneTransition.Instance != null &&
            SceneTransition.Instance.IsTransitioning)
            return;

        if (tutorialPanel.activeSelf)
        {
            if (EscapePressed())
                CloseTutorial();

            return;
        }

        HandleInput();
        UpdateCards(false);
        UpdateButtons();
    }

    // ステージカードを自動生成
    private void CreateCards()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            StageCardUI card = Instantiate(
                cardPrefab,
                cardContainer
            );

            card.Setup(stages[i], i, SelectStage);

            cards.Add(card);
        }
    }

    // キーボード・マウス操作
    private void HandleInput()
    {
        bool left = false;
        bool right = false;
        float scroll = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            left =
                Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame;

            right =
                Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame;
        }

        if (Mouse.current != null)
            scroll = Mouse.current.scroll.ReadValue().y;

#elif ENABLE_LEGACY_INPUT_MANAGER
        left =
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.A);

        right =
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.D);

        scroll = Input.mouseScrollDelta.y;
#endif

        if (left || scroll > 0f)
            MoveLeft();
        else if (right || scroll < 0f)
            MoveRight();
    }

    private bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame;

#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);

#else
        return false;
#endif
    }

    public void MoveLeft()
    {
        SelectStage(selectedIndex - 1);
    }

    public void MoveRight()
    {
        SelectStage(selectedIndex + 1);
    }

    public void SelectStage(int index)
    {
        if (!initialized || tutorialPanel.activeSelf)
            return;

        if (index < 0 || index >= stages.Length)
            return;

        if (selectedIndex == index)
            return;

        selectedIndex = index;
        isMoving = true;

        // 移動中は情報とスタートボタンを隠す
        SetStageInfoVisible(false);

        UpdateButtons();
    }

    // カードの位置と大きさを更新
    private void UpdateCards(bool immediate)
    {
        bool allArrived = true;

        float t = immediate
            ? 1f
            : 1f - Mathf.Exp(
                -scrollSpeed * Time.unscaledDeltaTime
            );

        for (int i = 0; i < cards.Count; i++)
        {
            StageCardUI card = cards[i];

            bool selected = i == selectedIndex;

            float targetX =
                (i - selectedIndex) * cardSpacing;

            float targetScale = selected
                ? selectedScale
                : normalScale;

            Vector2 targetPosition =
                new Vector2(targetX, 0f);

            Vector3 targetScaleVector =
                Vector3.one * targetScale;

            RectTransform rect = card.RectTransform;

            rect.anchoredPosition = Vector2.Lerp(
                rect.anchoredPosition,
                targetPosition,
                t
            );

            rect.localScale = Vector3.Lerp(
                rect.localScale,
                targetScaleVector,
                t
            );

            card.SetVisual(
                selected,
                selected ? 1f : normalAlpha
            );

            if (Vector2.Distance(
                rect.anchoredPosition,
                targetPosition
            ) > 1f)
            {
                allArrived = false;
            }

            if (Vector3.Distance(
                rect.localScale,
                targetScaleVector
            ) > 0.01f)
            {
                allArrived = false;
            }
        }

        // 中央のカードを手前に表示
        cards[selectedIndex]
            .transform.SetAsLastSibling();

        if (isMoving && allArrived)
        {
            isMoving = false;

            UpdateStageInfo();
            SetStageInfoVisible(true);
        }
    }

    private void UpdateStageInfo()
    {
        StageData stage = stages[selectedIndex];

        stageNameText.text = stage.stageName;
        descriptionText.text = stage.description;

        timeText.text = stage.isEndless
            ? "制限時間：無制限"
            : "制限時間：" + stage.timeLimit + " 秒";

        targetScoreText.text = stage.isEndless
            ? "クリア条件：なし"
            : "クリア条件：" +
              stage.targetScore.ToString("N0") + " pt";

        int blueRate = Mathf.RoundToInt(
            stage.blueSlimeRate * 100f
        );

        int redRate = 100 - blueRate;

        enemyRateText.text =
            "アカスラ " + redRate + "%  /  " +
            "アオスラ " + blueRate + "%";
    }

    private void SetStageInfoVisible(bool visible)
    {
        stageNameText.enabled = visible;
        descriptionText.enabled = visible;
        timeText.enabled = visible;
        targetScoreText.enabled = visible;
        enemyRateText.enabled = visible;

        startButton.gameObject.SetActive(visible);
    }

    private void UpdateButtons()
    {
        leftButton.interactable =
            selectedIndex > 0;

        rightButton.interactable =
            selectedIndex < stages.Length - 1;

        startButton.interactable = !isMoving;
    }

    // ゲーム開始
    public void StartGame()
    {
        if (!initialized || isMoving)
            return;

        if (tutorialPanel.activeSelf)
            return;

        GameSession.SelectStage(
            stages[selectedIndex],
            selectedIndex
        );

        LoadScene("Game");
    }

    // タイトルへ戻る
    public void BackToTitle()
    {
        LoadScene("Title");
    }

    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }

    private void LoadScene(string sceneName)
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
