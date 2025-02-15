using UnityEditor;
using UnityEngine;

public class RoundTransformPositions : EditorWindow
{
    private GameObject targetObject;

    [MenuItem("Tools/Round Transform Positions")]
    public static void ShowWindow()
    {
        GetWindow<RoundTransformPositions>("Round Transform Positions");
    }

    private void OnGUI()
    {
        GUILayout.Label("Round Transform Positions", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Round Positions"))
        {
            if (targetObject != null)
            {
                RoundChildTransforms(targetObject);
            }
            else
            {
                Debug.LogWarning("Target Object is not set.");
            }
        }
    }

    private void RoundChildTransforms(GameObject parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child != parent.transform)
            {
                Undo.RecordObject(child, "Round Transform Position");
                child.position = new Vector3(
                    Mathf.Round(child.position.x),
                    Mathf.Round(child.position.y),
                    Mathf.Round(child.position.z)
                );
                EditorUtility.SetDirty(child);
            }
        }

        Debug.Log("Rounded positions of all child transforms.");
    }
}
