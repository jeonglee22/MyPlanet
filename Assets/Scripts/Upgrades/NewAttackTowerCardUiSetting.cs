using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewAttackTowerCardUiSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerExplainText;
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI specialAbilityText;
    [SerializeField] private GameObject specialAbilityObjectBackground;
    [SerializeField] private GameObject newTowerTextObject;

    [SerializeField] private GameObject randomAbilityObject; // 기존 변수 유지 (지금 코드에선 사용 안 해도 OK)

    [SerializeField] private RectTransform contentRoot;

    // ✅ 기존에 인스펙터에 이미 연결돼 있는 리스트 그대로 사용
    [SerializeField] private List<GameObject> abilityPanels;     // "한 줄 능력" 패널들
    [SerializeField] private List<GameObject> selfAbilityPanels; // "자기 능력" 라벨/배경 등 (있다면)

    private void OnEnable()
    {
        if (specialAbilityObjectBackground != null) specialAbilityObjectBackground.SetActive(true);
        if (specialAbilityText != null) specialAbilityText.gameObject.SetActive(true);
    }

    public void SettingNewTowerCard(int towerId, int ability)
    {
        HideAllPanels();

        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        if (towerData == null) return;

        var towerExplainData = DataTableManager.TowerExplainTable.Get(towerData.TowerText_ID);
        if (towerExplainData != null)
        {
            SetTowerName(towerExplainData.TowerName);
            SetTowerExplain(towerExplainData.TowerDescribe);
        }

        var attackTowerAssetName = towerData.AttackTowerAssetCut;
        var attackTowerAsset = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        if (attackTowerAsset != null && towerImage != null)
            towerImage.sprite = attackTowerAsset;

        // ---- 표시할 "줄" 구성 ----
        // 0) 투사체 고유 능력(있으면 1줄)
        int startIndex = 0;

        var projectileData = DataTableManager.ProjectileTable.Get(towerData.Projectile_ID);
        int projectileEffectId = projectileData != null ? projectileData.ProjectileProperties1_ID : 0;
        float projectileEffectValue = projectileData != null ? projectileData.ProjectileProperties1Value : 0f;

        bool hasProjectileEffect = projectileEffectId != 0;

        // 1) 랜덤 능력(abilityId 1개) 안의 SpecialEffect 1~3 줄
        var abilityData = DataTableManager.RandomAbilityTable.Get(ability);
        if (abilityData == null) return;

        int effectCount = 1; // SpecialEffect_ID는 무조건 있다고 가정(없을 수 있으면 체크 추가)
        if (abilityData.SpecialEffect2_ID.HasValue && abilityData.SpecialEffect2_ID.Value != 0) effectCount++;
        if (abilityData.SpecialEffect3_ID.HasValue && abilityData.SpecialEffect3_ID.Value != 0) effectCount++;

        // 필요한 패널 수 계산: (투사체 1줄이면 +1) + 랜덤능력 효과 줄 수
        int neededPanels = (hasProjectileEffect ? 1 : 0) + effectCount;

        EnsureAbilityPanelCount(neededPanels);

        // ---- 실제 UI 채우기 ----

        // 투사체 고유 능력 1줄
        if (hasProjectileEffect)
        {
            // selfAbilityPanels[0]은 "Self Ability" 라벨/배경 같은 용도였던 걸 유지
            if (selfAbilityPanels != null && selfAbilityPanels.Count > 0 && selfAbilityPanels[0] != null)
                selfAbilityPanels[0].SetActive(true);

            var panel = abilityPanels[0];
            if (panel != null)
            {
                panel.SetActive(true);
                FillPanel(panel, projectileEffectId, projectileEffectValue);
            }
            startIndex = 1;
        }

        // 랜덤 능력 효과 1
        {
            var panel = abilityPanels[startIndex];
            panel.SetActive(true);
            FillPanel(panel, abilityData.SpecialEffect_ID, abilityData.SpecialEffectValue);
            startIndex++;
        }

        // 랜덤 능력 효과 2
        if (abilityData.SpecialEffect2_ID.HasValue && abilityData.SpecialEffect2_ID.Value != 0)
        {
            var panel = abilityPanels[startIndex];
            panel.SetActive(true);
            FillPanel(panel, abilityData.SpecialEffect2_ID.Value, abilityData.SpecialEffect2Value ?? 0f);
            startIndex++;
        }

        // 랜덤 능력 효과 3
        if (abilityData.SpecialEffect3_ID.HasValue && abilityData.SpecialEffect3_ID.Value != 0)
        {
            var panel = abilityPanels[startIndex];
            panel.SetActive(true);
            FillPanel(panel, abilityData.SpecialEffect3_ID.Value, abilityData.SpecialEffect3Value ?? 0f);
            startIndex++;
        }
    }

    private void HideAllPanels()
    {
        if (selfAbilityPanels != null)
        {
            foreach (var p in selfAbilityPanels)
                if (p != null) p.SetActive(false);
        }

        if (abilityPanels != null)
        {
            foreach (var p in abilityPanels)
                if (p != null) p.SetActive(false);
        }
    }

    // ✅ 인스펙터 수정 없이, 부족하면 "기존 0번 패널"을 복제해서 리스트를 늘림(한 번만 늘어나고 계속 재사용)
    private void EnsureAbilityPanelCount(int needed)
    {
        if (needed <= 0) return;

        if (abilityPanels == null || abilityPanels.Count == 0 || abilityPanels[0] == null)
        {
            Debug.LogWarning("[NewAttackTowerCardUiSetting] abilityPanels[0] (template) is missing.");
            return;
        }

        var template = abilityPanels[0];
        var parent = template.transform.parent;

        while (abilityPanels.Count < needed)
        {
            var clone = Instantiate(template, parent);
            clone.SetActive(false);
            abilityPanels.Add(clone);
        }
    }

    private void FillPanel(GameObject panel, int effectId, float value)
    {
        if (panel == null) return;

        // inactive 포함해서 가져오기 (SetActive(true) 전에 세팅해도 안전)
        var texts = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
        var image = panel.GetComponentInChildren<Image>(true);

        var effectData = DataTableManager.SpecialEffectTable.Get(effectId);
        if (effectData == null) return;

        if (image != null)
            image.sprite = LoadManager.GetLoadedGameTexture(effectData.SpecialEffectIcon);

        if (texts != null && texts.Length >= 2)
        {
            texts[0].text = effectData.SpecialEffectName;

            bool isPercent = effectData.SpecialEffectValueType == 1;
            texts[1].text = isPercent ? $"{value:0.##}%" : $"{value:0.##}";
        }
    }

    private void SetTowerName(string towerName)
    {
        if (towerNameText != null) towerNameText.text = towerName;
    }

    private void SetTowerExplain(string explain)
    {
        if (towerExplainText != null) towerExplainText.text = explain;
    }
}
