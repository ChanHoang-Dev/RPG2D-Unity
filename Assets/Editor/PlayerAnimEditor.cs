#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(PlayerCombat))]
public class PlayerAnimEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Kéo AnimationClip vào ô 'Editor Clip Ref' của từng đòn rồi bấm nút bên dưới " +
            "để tự động điền Lock Duration = độ dài clip thật. Chỉ chạy trong Editor, " +
            "không ảnh hưởng lúc build/runtime.",
            MessageType.Info);

        if (GUILayout.Button("Đồng bộ Lock Duration từ Editor Clip Ref"))
        {
            PlayerCombat combat = (PlayerCombat)target;
            Undo.RecordObject(combat, "Sync Attack Lock Durations");

            foreach (var atk in combat.attacks)
            {
                if (atk.editorClipRef != null)
                {
                    atk.lockDuration = atk.editorClipRef.length;
                }
            }

            EditorUtility.SetDirty(combat);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif