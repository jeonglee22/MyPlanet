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
        Test,
    }

    private Page currentPage = Page.Base;


    private SerializedObject so;

    private SerializedProperty towerAttackIdProp;
    private SerializedProperty damageProp;
    private SerializedProperty attackSpeedProp;
    private SerializedProperty rangeProp;

    private SerializedProperty accuracyProp;
    private SerializedProperty groupingProp;
    private SerializedProperty projectileNumProp;
    private SerializedProperty projectileIDProp;
    private SerializedProperty randomAbilityGroupIDProp;

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
            rangeProp = so.FindProperty("range");
        }
        else if (towerTester != null)
        {
            towerAttackTester = towerTester;
            so = new SerializedObject(towerAttackTester);

            towerAttackIdProp = so.FindProperty("towerAttackId");
            damageProp = so.FindProperty("damage");
            attackSpeedProp = so.FindProperty("attackSpeed");
            rangeProp = so.FindProperty("attackRange");
            accuracyProp = so.FindProperty("accuracy");
            groupingProp = so.FindProperty("grouping");
            projectileNumProp = so.FindProperty("projectileNum");
            projectileIDProp = so.FindProperty("projectile_ID");
            randomAbilityGroupIDProp = so.FindProperty("randomAbilityGroup_ID");
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
            case Page.Test:
                DrawTestPage();
                break;
        }
    }

    private void DrawTestPage()
    {

        if (so == null || targetTester == null)
        {
            EditorGUILayout.HelpBox(
                "씬에서 BalanceTester가 붙어있는 오브젝트를 선택하면 조절할 수 있어요.",
                MessageType.Info
            );
            if (GUILayout.Button("선택된 오브젝트에서 찾기"))
            {
                TrySetTargetFromSelection();
            }
            return;
        }

        EditorGUILayout.ObjectField("Target", targetTester, typeof(BalanceTester), true);
        EditorGUILayout.Space();

        so.Update();

        EditorGUILayout.PropertyField(damageProp);
        EditorGUILayout.PropertyField(attackSpeedProp);
        EditorGUILayout.PropertyField(rangeProp);

        so.ApplyModifiedProperties();

        EditorGUILayout.Space();
    }

    private void DrawPlanetPage()
    {
    }

    private void DrawTowerPage()
    {
        if (so == null || towerAttackTester == null)
        {
            EditorGUILayout.HelpBox(
                "씬에서 BalanceTester가 붙어있는 오브젝트를 선택하면 조절할 수 있어요.",
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

        int[] towerIds = { 1001001, 1000001, 1001002, 1002002, 1000002, 1002001 };
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

        EditorGUILayout.PropertyField(towerAttackIdProp);
        EditorGUILayout.PropertyField(damageProp);
        EditorGUILayout.PropertyField(attackSpeedProp);
        EditorGUILayout.PropertyField(rangeProp);
        EditorGUILayout.PropertyField(accuracyProp);
        EditorGUILayout.PropertyField(groupingProp);
        EditorGUILayout.PropertyField(projectileNumProp);
        EditorGUILayout.PropertyField(projectileIDProp);
        EditorGUILayout.PropertyField(randomAbilityGroupIDProp);

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

        if (GUILayout.Toggle(currentPage == Page.Test, "테스트", EditorStyles.toolbarButton))
            currentPage = Page.Test;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
