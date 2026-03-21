#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestionT02))]
public class QuestionT02Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Refresh Database Info"))
        {
            // Force reload for DB drawer
            var drawer = typeof(DatabaseReferenceDrawer);
            var field = drawer.GetField(
                "isLoaded",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            field?.SetValue(null, false);

            //columns drawer for answer column
            var drawerC = typeof(DatabaseColumnReferenceDrawer);
            var fieldC = drawerC.GetField(
                "isLoaded",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            fieldC?.SetValue(null, false);

            Debug.Log("Database info will reload on next draw (T02)");
        }
    }
}
#endif
