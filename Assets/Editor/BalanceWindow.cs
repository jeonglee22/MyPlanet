using UnityEditor;
using UnityEngine;

public class BalanceWindow : EditorWindow
{
    private SerializedObject so;
    private SerializedProperty damageProp;
    private SerializedProperty attackSpeedProp;
    private SerializedProperty rangeProp;

    private BalanceTester targetTester;

    [MenuItem("MyTools/Balance Window")]
    public static void ShowWindow()
    {
        GetWindow<BalanceWindow>("Balance Tuner");
    }

    private void OnSelectionChange()
    {
        // 선택된 오브젝트 바뀔 때마다 타겟 갱신
        TrySetTargetFromSelection();
        Repaint();
    }

    private void OnFocus()
    {
        // 창에 포커스 왔을 때도 갱신
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
        if (tester != null)
        {
            targetTester = tester;
            so = new SerializedObject(targetTester);
            damageProp = so.FindProperty("damage");
            attackSpeedProp = so.FindProperty("attackSpeed");
            rangeProp = so.FindProperty("range");
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

        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 중이므로 값 변경 즉시 게임에 반영됩니다.", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("에디터 모드에서는 그냥 일반 Inspector랑 동일하게 동작해요.", MessageType.None);
        }
    }
}
