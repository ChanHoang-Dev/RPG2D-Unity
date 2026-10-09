using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ListAnimatorStates : EditorWindow
{
    private AnimatorController controller;

    [MenuItem("Tools/List Animator States")]
    public static void ShowWindow()
    {
        GetWindow<ListAnimatorStates>("List Animator States");
    }

    private void OnGUI()
    {
        GUILayout.Label("Kéo Animator Controller gốc (VD: Knight_White) vào đây:", EditorStyles.boldLabel);
        controller = (AnimatorController)EditorGUILayout.ObjectField(controller, typeof(AnimatorController), false);

        if (GUILayout.Button("In tên state ra Console") && controller != null)
        {
            Debug.Log($"=== Danh sách state trong '{controller.name}' ===");
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                var layer = controller.layers[layerIndex];
                Debug.Log($"--- Layer {layerIndex}: '{layer.name}' ---");
                PrintStates(layer.stateMachine, layerIndex, "");
            }
        }
    }

    private void PrintStates(AnimatorStateMachine sm, int layerIndex, string prefix)
    {
        foreach (var childState in sm.states)
        {
            Debug.Log($"Layer {layerIndex} | State: \"{prefix}{childState.state.name}\"");
        }
        foreach (var childSM in sm.stateMachines)
        {
            // Sub-state machine: tên gọi Play() phải có dạng "TenSubSM.TenState"
            PrintStates(childSM.stateMachine, layerIndex, prefix + childSM.stateMachine.name + ".");
        }
    }
}