#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomPropertyDrawer(typeof(DatabaseColumnReference))]
public class DatabaseColumnReferenceDrawer : PropertyDrawer
{
    private static List<string> tableNames = new List<string>();
    private static Dictionary<string, List<string>> tableColumns = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<int>> tableIDs = new Dictionary<string, List<int>>();
    private static bool isLoaded = false;

    private const float LINE_HEIGHT = 18f;
    private const float SPACING = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LoadDatabaseInfo();

        EditorGUI.BeginProperty(position, label, property);

        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, LINE_HEIGHT),
            property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float yPos = position.y + LINE_HEIGHT + SPACING;

            SerializedProperty tableName = property.FindPropertyRelative("tableName");
            SerializedProperty columnName = property.FindPropertyRelative("columnName");
            SerializedProperty recordID = property.FindPropertyRelative("recordID");

            // Table Dropdown
            if (tableNames.Count > 0)
            {
                int tableIndex = Mathf.Max(0, tableNames.IndexOf(tableName.stringValue));
                tableIndex = EditorGUI.Popup(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                    "Table", tableIndex, tableNames.ToArray());
                tableName.stringValue = tableNames[tableIndex];
                yPos += LINE_HEIGHT + SPACING;

                // Column Dropdown
                if (tableColumns.ContainsKey(tableName.stringValue))
                {
                    var columns = tableColumns[tableName.stringValue];
                    int columnIndex = Mathf.Max(0, columns.IndexOf(columnName.stringValue));
                    columnIndex = EditorGUI.Popup(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                        "Column", columnIndex, columns.ToArray());
                    columnName.stringValue = columns[columnIndex];
                    yPos += LINE_HEIGHT + SPACING;
                }

                // Record ID Dropdown
                if (tableIDs.ContainsKey(tableName.stringValue))
                {
                    var ids = tableIDs[tableName.stringValue];
                    int idIndex = Mathf.Max(0, ids.IndexOf(recordID.intValue));
                    // Show Record ID as read-only label
                    EditorGUI.LabelField(new Rect(position.x, yPos, position.width * 0.4f, LINE_HEIGHT),
                        "Record ID");
                    EditorGUI.LabelField(new Rect(position.x + position.width * 0.4f, yPos, position.width * 0.6f, LINE_HEIGHT),
                        recordID.intValue.ToString());
                    yPos += LINE_HEIGHT + SPACING;
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return LINE_HEIGHT;
        return (LINE_HEIGHT + SPACING) * 4; // Foldout + 3 fields
    }

    private static void LoadDatabaseInfo()
    {
        if (isLoaded) return;

        string dbPath = Path.Combine(Application.dataPath, "StreamingAssets", "ltlrn01.db");
        if (!File.Exists(dbPath)) return;

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var tables = connection.Query<TableInfo>("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");
                tableNames = tables.Select(t => t.name).ToList();

                foreach (var tableName in tableNames)
                {
                    var columns = connection.GetTableInfo(tableName);
                    tableColumns[tableName] = columns.Select(c => c.Name).ToList();

                    try
                    {
                        var ids = connection.Query<IDRecord>($"SELECT ID FROM [{tableName}]");
                        tableIDs[tableName] = ids.Select(r => r.ID).ToList();
                    }
                    catch { tableIDs[tableName] = new List<int>(); }
                }

                isLoaded = true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading database: {ex.Message}");
        }
    }

    private class TableInfo { public string name { get; set; } }
    private class IDRecord { public int ID { get; set; } }
}
#endif
