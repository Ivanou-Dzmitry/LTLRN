#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestionT01))]
public class QuestionT01Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Refresh Database Info"))
        {
            // Force reload
            var drawer = typeof(DatabaseReferenceDrawer);
            var field = drawer.GetField("isLoaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            field?.SetValue(null, false);

            Debug.Log("Database info will reload on next draw");
        }
    }
}
#endif
