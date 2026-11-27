
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get;
        private set;
    }

    [Header("ステージ設定")]
    [SerializeField] private StageData defaultStage;

    [Header("ゲーム開始演出")]
    [SerializeField] private float tutorialDuration = 5f;
    [SerializeField] private float readyDuration = 1.5f;
    [SerializeField] private float goDuration = 0.5f;
    [SerializeField] private float finishDuration = 1.5f;

    [Header("プレイヤー初期設定")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float maxGuard = 100f;

    [Header("UI")]
    [SerializeField] private GameHUD hud;

    // 現在のゲーム状態
    public GameState State { get; private set; }

    // 選択中のステージ
    public StageData CurrentStage { get; private set; }

    // ゲーム中の数値
    public int Score { get; private set; }
    public int CurrentHP { get; private set; }
    public float CurrentGuard { get; private set; }
    public float RemainingTime { get; private set; }
    public float ElapsedTime { get; private set; }

    public int MaxHP => maxHP;
    public float MaxGuard => maxGuard;

    public bool IsPlaying =>
        State == GameState.Playing;

    public bool IsCleared { get; private set; }

    // 他のスクリプトへの通知
    public event Action<GameState> OnStateChanged;
    public event Action OnStatusChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ステージ選択画面からデータを取得
        CurrentStage = GameSession.SelectedStage;

        // Gameシーンから直接実行した場合
        if (CurrentStage == null)
        {
            CurrentStage = defaultStage;
        }

        if (CurrentStage == null || hud == null)
        {
            Debug.LogError(
                "GameManagerの設定が不足しています"
            );

            enabled = false;
            return;
        }

        InitializeGame();

        StartCoroutine(StartGameRoutine());
    }

    private void InitializeGame()
    {
        Score = 0;

        CurrentHP = maxHP;
        CurrentGuard = maxGuard;

        RemainingTime = CurrentStage.timeLimit;
        ElapsedTime = 0f;

        IsCleared = false;

        ChangeState(GameState.Tutorial);

        hud.Initialize(this);

        NotifyStatusChanged();
    }

    private IEnumerator StartGameRoutine()
    {
        hud.SetHUDVisible(false);
        hud.SetResultVisible(false);
        hud.SetAnnouncement("");
        hud.SetTutorialVisible(true);

        // シーンのフェード終了を待つ
        while (SceneTransition.Instance != null &&
               SceneTransition.Instance.IsTransitioning)
        {
            yield return null;
        }

        // チュートリアル表示
        yield return new WaitForSecondsRealtime(
            tutorialDuration
        );

        hud.SetTutorialVisible(false);
        hud.SetHUDVisible(true);

        ChangeState(GameState.Ready);

        // READY表示
        hud.SetAnnouncement("READY");

        yield return new WaitForSecondsRealtime(
            readyDuration
        );

        // GO表示
        hud.SetAnnouncement("GO!");

        yield return new WaitForSecondsRealtime(
            goDuration
        );

        hud.SetAnnouncement("");

        ChangeState(GameState.Playing);
    }

    private void Update()
    {
        // プレイ中以外は時間を進めない
        if (!IsPlaying)
            return;

        ElapsedTime += Time.deltaTime;

        // 通常ステージの制限時間
        if (!CurrentStage.isEndless)
        {
            RemainingTime -= Time.deltaTime;

            RemainingTime = Mathf.Max(
                0f, RemainingTime
            );

            if (RemainingTime <= 0f)
            {
                bool cleared =
                    Score >= CurrentStage.targetScore;

                FinishGame(cleared);
            }
        }

        hud.UpdateTimer(this);
    }

    // ポイント獲得
    public void AddScore(int amount)
    {
        if (!IsPlaying || amount <= 0)
            return;

        Score += amount;

        NotifyStatusChanged();
    }

    // プレイヤーがダメージを受ける
    public void TakeDamage(int damage)
    {
        if (!IsPlaying || damage <= 0)
            return;

        CurrentHP = Mathf.Max(
            0, CurrentHP - damage
        );

        NotifyStatusChanged();

        if (CurrentHP <= 0)
        {
            FinishGame(false);
        }
    }

    // HP回復
    public void Heal(int amount)
    {
        if (!IsPlaying || amount <= 0)
            return;

        CurrentHP = Mathf.Min(
            maxHP, CurrentHP + amount
        );

        NotifyStatusChanged();
    }

    // ガードゲージ消費
    public void ConsumeGuard(float amount)
    {
        if (!IsPlaying || amount <= 0f)
            return;

        CurrentGuard = Mathf.Max(
            0f, CurrentGuard - amount
        );

        NotifyStatusChanged();
    }

    // ガードゲージ回復
    public void RecoverGuard(float amount)
    {
        if (!IsPlaying || amount <= 0f)
            return;

        CurrentGuard = Mathf.Min(
            maxGuard, CurrentGuard + amount
        );

        NotifyStatusChanged();
    }

    // ゲーム終了
    public void FinishGame(bool cleared)
    {
        if (!IsPlaying)
            return;

        IsCleared = cleared;

        ChangeState(GameState.Finished);

        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        // ゲーム世界を停止
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        hud.SetAnnouncement("FINISH!");

        yield return new WaitForSecondsRealtime(
            finishDuration
        );

        hud.SetAnnouncement("");

        ChangeState(GameState.Result);

        hud.ShowResult(this);
    }

    private void ChangeState(GameState newState)
    {
        State = newState;

        OnStateChanged?.Invoke(State);
    }

    private void NotifyStatusChanged()
    {
        OnStatusChanged?.Invoke();
    }

    // ステージ選択へ戻る
    public void BackToStageSelect()
    {
        Time.timeScale = 1f;

        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(
                "StageSelect"
            );
        }
        else
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    // ===== 動作確認用 =====

    public void DebugAddScore()
    {
        AddScore(100);
    }

    public void DebugTakeDamage()
    {
        TakeDamage(20);
    }

    public void DebugFinish()
    {
        if (!IsPlaying)
            return;

        bool cleared =
            !CurrentStage.isEndless &&
            Score >= CurrentStage.targetScore;

        FinishGame(cleared);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
