using System;
using Cysharp.Threading.Tasks;
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

    private SerializedProperty hpScaleProp;
    private SerializedProperty attackScaleProp;
    private SerializedProperty defenseScaleProp;
    private SerializedProperty penetrationScaleProp;
    private SerializedProperty speedScaleProp;
    private SerializedProperty sizeScaleProp;
    private SerializedProperty expScaleProp;
    private SerializedProperty healthProp;
    private SerializedProperty defenseProp;
    private SerializedProperty barriorProp;
    private SerializedProperty speedProp;
    private SerializedProperty attackProp;
    private SerializedProperty ratePenetrationEnemyProp;
    private SerializedProperty fixedPenetrationEnemyProp;
    private SerializedProperty expProp;
    private SerializedProperty enemyCountProp;
    private SerializedProperty waveIdProp;
    private SerializedProperty moveTypeIdProp;
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
    private SerializedObject soTower;
    private EnemyStatTester enemyStatTester;
    private SerializedObject soEnemy;
    private SerializedProperty enemyTypeId;
    private SerializedProperty spawnPointProp;
    private SerializedProperty enemyTypeProp;
    private SerializedProperty paternIdProp;

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
        // if (Selection.activeGameObject == null)
        // {
        //     so = null;
        //     targetTester = null;
        //     return;
        // }

        soEnemy = null;
        soTower = null;

        // var tester = Selection.activeGameObject.GetComponent<BalanceTester>();
        // var towerTester = Selection.activeGameObject.GetComponent<TowerAttackTester>();

        var towerTester = GameObject.FindWithTag(TagName.TowerAttackTester)?.GetComponent<TowerAttackTester>();
        var enemyTester = GameObject.FindWithTag(TagName.EnemyStatTester)?.GetComponent<EnemyStatTester>();
        Debug.Log(towerTester);
        if (towerTester != null)
        {
            towerAttackTester = towerTester;
            soTower = new SerializedObject(towerAttackTester);

            towerAttackIdProp = soTower.FindProperty("towerAttackId");
            targetRangeProp = soTower.FindProperty("targetRange");
            damageProp = soTower.FindProperty("damage");
            attackSpeedProp = soTower.FindProperty("attackSpeed");
            accuracyProp = soTower.FindProperty("accuracy");
            groupingProp = soTower.FindProperty("grouping");
            projectileNumProp = soTower.FindProperty("projectileNum");
            targetNumProp = soTower.FindProperty("targetNum");
            hitSizeProp = soTower.FindProperty("hitSize");
            ratePenetrationProp = soTower.FindProperty("ratePenetration");
            fixedPenetrationProp = soTower.FindProperty("fixedPenetration");
            projectileSpeedProp = soTower.FindProperty("projectileSpeed");
            durationProp = soTower.FindProperty("duration");

            pierceCount = soTower.FindProperty("pierceCount");
            chainCount = soTower.FindProperty("chainCount");
            slowPercent = soTower.FindProperty("slowPercent");
            splitCount = soTower.FindProperty("splitCount");
            homing = soTower.FindProperty("homing");
            explosionRadius = soTower.FindProperty("explosionRadius");
        }
        if (enemyTester != null)
        {
            enemyStatTester = enemyTester;
            soEnemy = new SerializedObject(enemyStatTester);

            enemyTypeId = soEnemy.FindProperty("enemyTypeId");

            hpScaleProp = soEnemy.FindProperty("hpScale");
            attackScaleProp = soEnemy.FindProperty("attackScale");
            defenseScaleProp = soEnemy.FindProperty("defenseScale");
            penetrationScaleProp = soEnemy.FindProperty("penetrationScale");
            speedScaleProp = soEnemy.FindProperty("speedScale");
            sizeScaleProp = soEnemy.FindProperty("sizeScale");
            expScaleProp = soEnemy.FindProperty("expScale");

            healthProp = soEnemy.FindProperty("health");
            defenseProp = soEnemy.FindProperty("defense");
            barriorProp = soEnemy.FindProperty("barrior");
            speedProp = soEnemy.FindProperty("speed");
            attackProp = soEnemy.FindProperty("attack");
            ratePenetrationEnemyProp = soEnemy.FindProperty("ratePenetration");
            fixedPenetrationEnemyProp = soEnemy.FindProperty("fixedPenetration");
            expProp = soEnemy.FindProperty("exp");
            enemyCountProp = soEnemy.FindProperty("enemyCount");
            waveIdProp = soEnemy.FindProperty("waveId");

            moveTypeIdProp = soEnemy.FindProperty("moveTypeId");
            spawnPointProp = soEnemy.FindProperty("spawnPoint");
            enemyTypeProp = soEnemy.FindProperty("enemyType");
            paternIdProp = soEnemy.FindProperty("patternId");
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

        // if (Selection.activeGameObject == null)
        // {
        //     return;
        // }

        if (soTower == null)
            return;

        soTower.Update();

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

        soTower.ApplyModifiedProperties();
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
        if (soTower == null || towerAttackTester == null)
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

        soTower.Update();

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

        soTower.ApplyModifiedProperties();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("데이터 임시 저장"))
        {
            towerAttackTester.SetProjectileData();
        }
        if (GUILayout.Button("데이터 테이블에 저장"))
        {
            towerAttackTester.SetProjectileData();
            DataTableManager.ProjectileTable.SaveOverridesAsync().Forget();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEnemyPage()
    {
        if (soEnemy == null || enemyStatTester == null)
        {
            EditorGUILayout.HelpBox(
                "씬에서 오브젝트를 선택하면 창이 켜집니다.",
                MessageType.Info
            );
            return;
        }

        EditorGUILayout.ObjectField("Enemy", enemyStatTester, typeof(EnemyStatTester), true);
        EditorGUILayout.Space();

        soEnemy.Update();

        int[] enemyIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 };
        string[] enemyNames = { "wave용", "느린 운석", "보통 운석", "빠른 운석", "느린 유도 운석", "보통 유도 운석", "빠른 유도 운석",
                                "중형 운석", "대형 운석", "[희귀] 운석 뭉치", "[중간] 운석 뭉치", "[중간] 정예 운석 뭉치",
                                "[최종] 운석 뭉치", "[최종] 정예 운석 뭉치", "[중간] 다프니스", "[최종] 토성", "[최종] 타이탄",
                                "[튜토리얼 희귀] 중형 운석", "[튜토리얼 최종] 대형 운석", "[튜토리얼 중간] 대형 운석",
                                "[튜토리얼 최종] 초대형 운석", "테스트 운석", "테스트 운석뭉치" };

        EditorGUI.BeginChangeCheck();

        int newId = EditorGUILayout.IntPopup(
            "EnemyType ID",
            enemyTypeId.intValue,
            enemyNames,
            enemyIds
        );

        if (EditorGUI.EndChangeCheck())
        {
            enemyTypeId.intValue = newId;
        }


        int[] waveIds = new int[enemyStatTester.WaveIds.Length];
        string[] waveNames = new string[enemyStatTester.WaveIds.Length];

        for (int i = 0; i < 68; i++)
        {
            waveIds[i] = i;
            if (i == 0)
                waveNames[i] = "기본 웨이브";
            else
                waveNames[i] = $"Wave {enemyStatTester.WaveIds[i]}";
        }

        EditorGUI.BeginChangeCheck();

        int newWaveId = EditorGUILayout.IntPopup(
            "WaveType ID",
            waveIdProp.intValue,
            waveNames,
            waveIds
        );

        if (EditorGUI.EndChangeCheck())
        {
            waveIdProp.intValue = newWaveId;
        }

        int[] moveTypeIds = { 0, 1, 2, 3, 4, 5 };
        string[] moveTypeNames = { "일반", "유도", "추적:끝까지 따라감", "고정형", "공전", "가로 이동" };

        EditorGUI.BeginChangeCheck();

        int newMoveTypeId = EditorGUILayout.IntPopup(
            "MoveType ID",
            moveTypeIdProp.intValue,
            moveTypeNames,
            moveTypeIds
        );

        if (EditorGUI.EndChangeCheck())
        {
            moveTypeIdProp.intValue = newMoveTypeId;
        }

        int[] patternIds = { 0,1,2,3,4,5 };
        string[] patternNames = { "없음", "다프니스", "타이탄", "토성", "운석 뭉치 돌진", "운석 뭉치 추적" };

        EditorGUI.BeginChangeCheck();

        int newPatternId = EditorGUILayout.IntPopup(
            "Pattern ID",
            paternIdProp.intValue,
            patternNames,
            patternIds
        );

        if (EditorGUI.EndChangeCheck())
        {
            paternIdProp.intValue = newPatternId;
        }

        EditorGUILayout.LabelField("스폰포인트", EditorStyles.boldLabel);
        spawnPointProp.intValue = EditorGUILayout.IntSlider("스폰포인트 0 ~ 4", spawnPointProp.intValue, 0, 4);
        
        EditorGUILayout.LabelField("적타입", EditorStyles.boldLabel);
        enemyTypeProp.intValue = EditorGUILayout.IntSlider("적타입 1 ~ 4", enemyTypeProp.intValue, 1, 4);

        // EditorGUILayout.PropertyField(enemyTypeId);
        EditorGUILayout.PropertyField(hpScaleProp);
        EditorGUILayout.PropertyField(attackScaleProp);
        EditorGUILayout.PropertyField(defenseScaleProp);
        EditorGUILayout.PropertyField(penetrationScaleProp);
        EditorGUILayout.PropertyField(speedScaleProp);
        EditorGUILayout.PropertyField(sizeScaleProp);
        EditorGUILayout.PropertyField(expScaleProp);

        EditorGUILayout.PropertyField(healthProp);
        EditorGUILayout.PropertyField(defenseProp);
        EditorGUILayout.PropertyField(barriorProp);
        EditorGUILayout.PropertyField(speedProp);
        EditorGUILayout.PropertyField(attackProp);
        EditorGUILayout.PropertyField(ratePenetrationEnemyProp);
        EditorGUILayout.PropertyField(fixedPenetrationEnemyProp);
        EditorGUILayout.PropertyField(expProp);
        EditorGUILayout.PropertyField(enemyCountProp);

        soEnemy.ApplyModifiedProperties();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("적 소환"))
        {
            enemyStatTester.EnemySpawn();
        }
        if (GUILayout.Button("웨이브 소환"))
        {
            enemyStatTester.StartWave();
        }
        if (GUILayout.Button("모든 적 제거"))
        {
            enemyStatTester.ClearAllEnemies();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("데이터 임시 저장"))
        {
            enemyStatTester.SetEnemyData();
        }
        if (GUILayout.Button("데이터 테이블에 저장"))
        {
            enemyStatTester.SetEnemyData();
            DataTableManager.EnemyTable.SaveOverridesAsync().Forget();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTopButtons()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

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
