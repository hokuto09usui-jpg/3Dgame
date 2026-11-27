
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewStageData",
    menuName = "SlimeRush/Stage Data"
)]
public class StageData : ScriptableObject
{
    [Header("基本情報")]
    public int stageId = 1;
    public string stageName;

    [TextArea(2, 5)]
    public string description;

    public Sprite stageImage;

    [Header("クリア条件")]
    public bool isEndless;

    [Min(1f)]
    public float timeLimit = 120f;

    [Min(0)]
    public int targetScore = 1000;

    [Header("敵の出現設定")]
    [Range(0f, 1f)]
    public float blueSlimeRate = 0f;

    [Min(0.1f)]
    public float spawnInterval = 1f;

    [Min(1)]
    public int maxEnemies = 20;

    [Header("敵のHP増加設定")]
    public bool increaseEnemyHP;

    [Min(1f)]
    public float hpIncreaseInterval = 20f;

    [Min(0f)]
    public float hpIncreaseRate = 0.1f;
}
