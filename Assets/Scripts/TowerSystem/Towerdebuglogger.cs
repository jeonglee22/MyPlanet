using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerDebugLogger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode debugKey = KeyCode.F1;
    [SerializeField] private bool enableDebug = true;

    [Header("References")]
    [SerializeField] private TowerInstallControl towerInstallControl;

    private void Update()
    {
        if (!enableDebug) return;
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            LogAllTowersDebugInfo();
        }
    }

    public void LogAllTowersDebugInfo()
    {
        if (towerInstallControl == null)
        {
            Debug.LogError("[TowerDebugLogger] TowerInstallControl이 설정되지 않았습니다!");
            return;
        }

        StringBuilder fullLog = new StringBuilder();

        fullLog.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        fullLog.AppendLine("║           타워 디버그 정보 (TOWER DEBUG INFO)                  ║");
        fullLog.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        fullLog.AppendLine();

        int totalSlots = towerInstallControl.TowerCount;
        int usedSlots = 0;
        int emptySlots = 0;
        int attackTowers = 0;
        int amplifierTowers = 0;

        fullLog.AppendLine($"총 슬롯 개수: {totalSlots}");
        fullLog.AppendLine($"현재 설치된 타워: {towerInstallControl.CurrentTowerCount} / {towerInstallControl.MaxTowerCount}");
        fullLog.AppendLine();
        fullLog.AppendLine("════════════════════════════════════════════════════════════════");
        fullLog.AppendLine();

        // 각 슬롯별로 정보 출력
        for (int i = 0; i < totalSlots; i++)
        {
            fullLog.AppendLine($"┌─────────────────────────────────────────────────────────────┐");
            fullLog.AppendLine($"│ 슬롯 {i,2}                                                      │");
            fullLog.AppendLine($"└─────────────────────────────────────────────────────────────┘");

            // 빈 슬롯 체크
            if (!towerInstallControl.IsUsedSlot(i))
            {
                fullLog.AppendLine("  [빈 슬롯]");
                fullLog.AppendLine();
                emptySlots++;
                continue;
            }

            usedSlots++;

            // ========== 수정된 부분 시작 ==========
            // Planet을 통해 실제 타워 컴포넌트 가져오기
            var attackTower = towerInstallControl.GetAttackTower(i);
            var amplifierTower = towerInstallControl.GetAmplifierTower(i);

            if (attackTower != null)
            {
                attackTowers++;
                fullLog.AppendLine("  타입: 공격 타워 (ATTACK TOWER)");
                fullLog.AppendLine();
                fullLog.AppendLine(attackTower.GetDebugInfo());
                fullLog.AppendLine();
                continue;
            }

            if (amplifierTower != null)
            {
                amplifierTowers++;
                fullLog.AppendLine("  타입: 증폭 타워 (AMPLIFIER TOWER)");
                fullLog.AppendLine();
                fullLog.AppendLine(amplifierTower.GetDebugInfo());
                fullLog.AppendLine();
                continue;
            }
            // ========== 수정된 부분 끝 ==========

            // 둘 다 아닌 경우
            fullLog.AppendLine("  [오류: TowerAttack 또는 TowerAmplifier 컴포넌트를 찾을 수 없습니다]");
            fullLog.AppendLine();
        }

        // 요약 정보
        fullLog.AppendLine("════════════════════════════════════════════════════════════════");
        fullLog.AppendLine();
        fullLog.AppendLine("📊 요약 (SUMMARY)");
        fullLog.AppendLine($"  총 슬롯:        {totalSlots}개");
        fullLog.AppendLine($"  사용 중:        {usedSlots}개");
        fullLog.AppendLine($"  빈 슬롯:        {emptySlots}개");
        fullLog.AppendLine($"  공격 타워:      {attackTowers}개");
        fullLog.AppendLine($"  증폭 타워:      {amplifierTowers}개");
        fullLog.AppendLine();
        fullLog.AppendLine("════════════════════════════════════════════════════════════════");

        // 콘솔에 출력
        Debug.Log(fullLog.ToString());
    }

    /// <summary>
    /// 특정 슬롯의 타워 정보만 출력합니다.
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스</param>
    public void LogSingleTowerDebugInfo(int slotIndex)
    {
        if (towerInstallControl == null)
        {
            Debug.LogError("[TowerDebugLogger] TowerInstallControl이 설정되지 않았습니다!");
            return;
        }

        if (slotIndex < 0 || slotIndex >= towerInstallControl.TowerCount)
        {
            Debug.LogError($"[TowerDebugLogger] 잘못된 슬롯 인덱스: {slotIndex}");
            return;
        }

        StringBuilder log = new StringBuilder();

        log.AppendLine($"═══ 슬롯 {slotIndex} 디버그 정보 ═══");
        log.AppendLine();

        // 빈 슬롯 체크
        if (!towerInstallControl.IsUsedSlot(slotIndex))
        {
            log.AppendLine("[빈 슬롯]");
            Debug.Log(log.ToString());
            return;
        }

        var towers = towerInstallControl.Towers;
        GameObject towerObj = towers[slotIndex];

        if (towerObj == null)
        {
            log.AppendLine("[오류: GameObject가 null입니다]");
            Debug.Log(log.ToString());
            return;
        }

        // 공격 타워 확인
        var attackTower = towerObj.GetComponent<TowerAttack>();
        if (attackTower != null)
        {
            log.AppendLine("타입: 공격 타워 (ATTACK TOWER)");
            log.AppendLine();
            log.AppendLine(attackTower.GetDebugInfo());
            Debug.Log(log.ToString());
            return;
        }

        // 증폭 타워 확인
        var amplifierTower = towerObj.GetComponent<TowerAmplifier>();
        if (amplifierTower != null)
        {
            log.AppendLine("타입: 증폭 타워 (AMPLIFIER TOWER)");
            log.AppendLine();
            log.AppendLine(amplifierTower.GetDebugInfo());
            Debug.Log(log.ToString());
            return;
        }

        log.AppendLine("[오류: TowerAttack 또는 TowerAmplifier 컴포넌트를 찾을 수 없습니다]");
        Debug.Log(log.ToString());
    }
}