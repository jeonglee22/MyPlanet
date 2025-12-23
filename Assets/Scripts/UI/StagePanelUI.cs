using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StagePanelUI : MonoBehaviour
{
    [SerializeField] private Image stageImage;
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private int stageId;
    private string lockedBossName = "LockedBossName";

    public void Initialize(StageData stageData, bool isUnLocked)
    {
        stageId = stageData.Stage_Id;

        if (isUnLocked)
        {
            stageNameText.text = DataTableManager.LobbyStringTable.GetString(stageData.StageName);
            bossNameText.text = DataTableManager.LobbyStringTable.GetString(stageData.BossName);
            stageImage.sprite = LoadManager.GetLoadedGameTexture(stageData.StageImage);
        }
        else
        {
            stageNameText.text = DataTableManager.LobbyStringTable.GetString(stageData.LockedStageName);
            bossNameText.text = DataTableManager.LobbyStringTable.GetString(lockedBossName);
            stageImage.sprite = LoadManager.GetLoadedGameTexture(stageData.LockedStageImage);
        }
    }
}