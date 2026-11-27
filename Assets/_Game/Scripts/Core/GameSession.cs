
using UnityEngine;

public static class GameSession
{
    // 現在選択されているステージ
    public static StageData SelectedStage
    {
        get;
        private set;
    }

    // ステージ選択画面での選択位置
    public static int SelectedStageIndex
    {
        get;
        private set;
    }

    public static void SelectStage(
        StageData stage,
        int index
    )
    {
        SelectedStage = stage;
        SelectedStageIndex = index;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetSession()
    {
        SelectedStage = null;
        SelectedStageIndex = 0;
    }
}
