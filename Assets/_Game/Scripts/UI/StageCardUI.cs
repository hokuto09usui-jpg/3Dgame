
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageCardUI : MonoBehaviour
{
    [Header("カードUI")]
    [SerializeField] private Image background;
    [SerializeField] private Image stageImage;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button button;

    [Header("カードカラー")]
    [SerializeField] private Color selectedColor =
        new Color(1f, 0.85f, 0.3f);

    [SerializeField] private Color normalColor =
        Color.white;

    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Setup(
        StageData data,
        int index,
        Action<int> onClick
    )
    {
        stageNameText.text = data.stageName;

        stageImage.sprite = data.stageImage;
        stageImage.enabled = data.stageImage != null;

        int bestScore = PlayerPrefs.GetInt(
            "BestScore_" + data.stageId, 0
        );

        bestScoreText.text =
            "BEST : " + bestScore.ToString("N0") + " pt";

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            () => onClick?.Invoke(index)
        );
    }

    public void SetVisual(
        bool selected,
        float alpha
    )
    {
        background.color = selected
            ? selectedColor
            : normalColor;

        canvasGroup.alpha = alpha;
    }
}
