using System;
using UnityEditor;
using UnityEngine;

public class BalanceWindow : EditorWindow
{
    enum Page
    {
        Base,
        Planet,
        Tower,
        Enemy,
        RandomAbility,
    }

    private Page currentPage = Page.Base;

    [System.Flags]
    public enum RandomAbilityType
    {
    None      = 0,
    Explosion = 1 << 0,
    Homing    = 1 << 1,
    Split     = 1 << 2,
    Slow      = 1 << 3,
    Chain     = 1 << 4,
    Pierce    = 1 << 5,
    }

    RandomAbilityType abilityMask;

    private SerializedProperty explosionRadius;
    private SerializedProperty homing;
    private SerializedProperty splitCount;
    private SerializedProperty slowPercent;
    private SerializedProperty chainCount;
    private SerializedProperty pierceCount;

    private SerializedObject so;

    private SerializedProperty towerAttackIdProp;
    private SerializedProperty damageProp;
    private SerializedProperty attackSpeedProp;
    private SerializedProperty targetRangeProp;

    private SerializedProperty accuracyProp;
    private SerializedProperty groupingProp;
    private SerializedProperty projectileNumProp;
    private SerializedProperty targetNumProp;
    private SerializedProperty hitSizeProp;
    private SerializedProperty ratePenetrationProp;
    private SerializedProperty fixedPenetrationProp;
    private SerializedProperty projectileSpeedProp;
    private SerializedProperty durationProp;

    private BalanceTester targetTester;
    private TowerAttackTester towerAttackTester;

    [MenuItem("MyTools/Balance Window")]
    public static void ShowWindow()
    {
        GetWindow<BalanceWindow>("Balance Tuner");
    }

    private void OnSelectionChange()
    {
        TrySetTargetFromSelection();
        Repaint();
    }

    private void OnFocus()
    {
        TrySetTargetFromSelection();
    }

    private void TrySetTargetFromSelection()
    {
        if (Selection.activeGameObject == null)
        {
            so = null;
            targetTester = null;
            return;
        }

        var tester = Selection.activeGameObject.GetComponent<BalanceTester>();
        var towerTester = Selection.activeGameObject.GetComponent<TowerAttackTester>();
        if (tester != null)
        {
            targetTester = tester;
            so = new SerializedObject(targetTester);
            damageProp = so.FindProperty("damage");
            attackSpeedProp = so.FindProperty("attackSpeed");
        }
        else if (towerTester != null)
        {
            towerAttackTester = towerTester;
            so = new SerializedObject(towerAttackTester);

            towerAttackIdProp = so.FindProperty("towerAttackId");
            targetRangeProp = so.FindProperty("targetRange");
            damageProp = so.FindProperty("damage");
            attackSpeedProp = so.FindProperty("attackSpeed");
            accuracyProp = so.FindProperty("accuracy");
            groupingProp = so.FindProperty("grouping");
            projectileNumProp = so.FindProperty("projectileNum");
            targetNumProp = so.FindProperty("targetNum");
            hitSizeProp = so.FindProperty("hitSize");
            ratePenetrationProp = so.FindProperty("ratePenetration");
            fixedPenetrationProp = so.FindProperty("fixedPenetration");
            projectileSpeedProp = so.FindProperty("projectileSpeed");
            durationProp = so.FindProperty("duration");

            pierceCount = so.FindProperty("pierceCount");
            chainCount = so.FindProperty("chainCount");
            slowPercent = so.FindProperty("slowPercent");
            splitCount = so.FindProperty("splitCount");
            homing = so.FindProperty("homing");
            explosionRadius = so.FindProperty("explosionRadius");
        }
        else
        {
            so = null;
            targetTester = null;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Balance Tuner", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawTopButtons();
        GUILayout.Space(10);

        switch (currentPage)
        {
            case Page.Base:
                DrawBasePage();
                break;
            case Page.Enemy:
                DrawEnemyPage();
                break;
            case Page.Tower:
                DrawTowerPage();
                break;
            case Page.Planet:
                DrawPlanetPage();
                break;
            case Page.RandomAbility:
                DrawRandomAbilityPage();
                break;
        }
    }

    private void DrawRandomAbilityPage()
    {
        EditorGUILayout.LabelField("랜덤 능력 설정", EditorStyles.boldLabel);
        GUILayout.Space(4);

        // 1) 어떤 랜덤 능력들을 쓸지 묶어서 선택
        abilityMask = (RandomAbilityType)EditorGUILayout.EnumFlagsField(
            new GUIContent("활성화 능력들"),
            abilityMask
        );

        GUILayout.Space(8);

        if (Selection.activeGameObject == null)
        {
            return;
        }

        var towerTester = Selection.activeGameObject.GetComponent<TowerAttackTester>();
        if (towerTester == null)
            return;

        so.Update();

        // 2) 능력별 카드/행으로 파라미터 표시
        using (new EditorGUILayout.VerticalScope("box"))
        {
            DrawExplosionAbility();
            DrawHomingAbility();
            DrawSplitAbility();
            DrawSlowAbility();
            DrawChainAbility();
            DrawPierceAbility();
        }

        so.ApplyModifiedProperties();
    }

    private void DrawPierceAbility()
    {
        if (!abilityMask.HasFlag(RandomAbilityType.Pierce))
            return;

        EditorGUILayout.LabelField("관통", EditorStyles.boldLabel);

        pierceCount.intValue = EditorGUILayout.IntSlider("관통 수", pierceCount.intValue, 0, 100);

        EditorGUILayout.HelpBox("0이면 관통 없음", MessageType.Info);
        GUILayout.Space(4);
    }

    private void DrawChainAbility()
    {
        if (!abilityMask.HasFlag(RandomAbilityType.Chain))
            return;

        EditorGUILayout.LabelField("연쇄", EditorStyles.boldLabel);

        chainCount.intValue = EditorGUILayout.IntSlider("연쇄 수", chainCount.intValue, 0, 100);

        EditorGUILayout.HelpBox("0이면 연쇄 없음", MessageType.Info);
        GUILayout.Space(4);
    }

    private void DrawSlowAbility()
    {
        if (!abilityMask.HasFlag(RandomAbilityType.Slow))
            return;

        EditorGUILayout.LabelField("둔화", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(slowPercent);

        EditorGUILayout.HelpBox("0%이면 둔화 없음", MessageType.Info);
        GUILayout.Space(4);
    }

    private void DrawSplitAbility()
    {
        if (!abilityMask.HasFlag(RandomAbilityType.Split))
            return;

        EditorGUILayout.LabelField("분열", EditorStyles.boldLabel);

        splitCount.intValue = EditorGUILayout.IntSlider("분열 횟수", splitCount.intValue, 0, 100);

        EditorGUILayout.HelpBox("0이면 분열 없음, 그 이상은 분열 횟수", MessageType.Info);
        GUILayout.Space(4);
    }

    private void DrawHomingAbility()
    {
        if (!abilityMask.HasFlag(RandomAbilityType.Homing))
            return;

        EditorGUILayout.LabelField("유도", EditorStyles.boldLabel);

        homing.intValue = EditorGUILayout.IntSlider("유도 여부 (0/1)", homing.intValue, 0, 1);

        EditorGUILayout.HelpBox("0 : 유도 없음, 1 : 유도 있음", MessageType.Info);
        GUILayout.Space(4);
    }

    private void DrawExplosionAbility()
    {
        if (!abilityMask.HasFlag(RandomAbilityType.Explosion))
            return;

        EditorGUILayout.LabelField("폭발", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(explosionRadius);

        EditorGUILayout.HelpBox("0이면 폭발 효과 없음", MessageType.Info);
        GUILayout.Space(4);
    }

    private void DrawPlanetPage()
    {
    }

    private void DrawTowerPage()
    {
        if (so == null || towerAttackTester == null)
        {
            EditorGUILayout.HelpBox(
                "씬에서 TowerAttackTester가 붙어있는 오브젝트를 선택하면 조절할 수 있어요.",
                MessageType.Info
            );
            if (GUILayout.Button("선택된 오브젝트에서 찾기"))
            {
                TrySetTargetFromSelection();
            }
            return;
        }

        EditorGUILayout.ObjectField("Tower", towerAttackTester, typeof(TowerAttackTester), true);
        EditorGUILayout.Space();

        so.Update();

        int[] towerIds = { 0, 1, 2, 3, 4, 5 };
        string[] towerNames = { "케틀링", "권총", "레이저", "미사일", "샷건", "스나이퍼" };

        EditorGUI.BeginChangeCheck();

        int newId = EditorGUILayout.IntPopup(
            "TowerAttack ID",
            towerAttackIdProp.intValue,
            towerNames,
            towerIds
        );

        if (EditorGUI.EndChangeCheck())
        {
            towerAttackIdProp.intValue = newId;
        }

        int[] rangeIds = { 0, 1, 2 };
        string[] rangeNames = { "근거리", "중거리", "원거리" };

        EditorGUI.BeginChangeCheck();

        int rangeId = EditorGUILayout.IntPopup(
            "TowerRange ID",
            targetRangeProp.intValue,
            rangeNames,
            rangeIds
        );

        if (EditorGUI.EndChangeCheck())
        {
            targetRangeProp.intValue = rangeId;
        }

        EditorGUILayout.PropertyField(towerAttackIdProp);
        EditorGUILayout.PropertyField(damageProp);
        EditorGUILayout.PropertyField(attackSpeedProp);
        EditorGUILayout.PropertyField(accuracyProp);
        EditorGUILayout.PropertyField(groupingProp);
        EditorGUILayout.PropertyField(projectileNumProp);
        EditorGUILayout.PropertyField(targetNumProp);
        EditorGUILayout.PropertyField(hitSizeProp);
        EditorGUILayout.PropertyField(ratePenetrationProp);
        EditorGUILayout.PropertyField(fixedPenetrationProp);
        EditorGUILayout.PropertyField(projectileSpeedProp);
        EditorGUILayout.PropertyField(durationProp);

        so.ApplyModifiedProperties();
    }

    private void DrawEnemyPage()
    {
    }

    private void DrawBasePage()
    {
    }

    private void DrawTopButtons()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Toggle(currentPage == Page.Base, "개요", EditorStyles.toolbarButton))
            currentPage = Page.Base;

        if (GUILayout.Toggle(currentPage == Page.Enemy, "적 설정", EditorStyles.toolbarButton))
            currentPage = Page.Enemy;

        if (GUILayout.Toggle(currentPage == Page.Tower, "타워 설정", EditorStyles.toolbarButton))
            currentPage = Page.Tower;

        if (GUILayout.Toggle(currentPage == Page.Planet, "행성 설정", EditorStyles.toolbarButton))
            currentPage = Page.Planet;

        if (GUILayout.Toggle(currentPage == Page.RandomAbility, "능력", EditorStyles.toolbarButton))
            currentPage = Page.RandomAbility;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
