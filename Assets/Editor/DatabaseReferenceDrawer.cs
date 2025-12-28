#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomPropertyDrawer(typeof(DatabaseReference))]
public class DatabaseReferenceDrawer : PropertyDrawer
{
    private static List<string> tableNames = new List<string>();
    private static Dictionary<string, List<string>> tableColumns = new Dictionary<string, List<string>>();
    private static Dictionary<string, Dictionary<int, string>> tableRecords = new Dictionary<string, Dictionary<int, string>>();
    private static bool isLoaded = false;

    private const float LINE_HEIGHT = 18f;
    private const float SPACING = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LoadDatabaseInfo();

        EditorGUI.BeginProperty(position, label, property);

        // Draw foldout
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, LINE_HEIGHT),
            property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float yPos = position.y + LINE_HEIGHT + SPACING;

            // Get properties
            SerializedProperty tableName = property.FindPropertyRelative("tableName");
            SerializedProperty columnName = property.FindPropertyRelative("columnName");
            SerializedProperty recordID = property.FindPropertyRelative("recordID");
            SerializedProperty value = property.FindPropertyRelative("value");
            SerializedProperty whereColumn = property.FindPropertyRelative("whereColumn");
            SerializedProperty whereValue = property.FindPropertyRelative("whereValue");

            // Table Name Dropdown
            if (tableNames.Count > 0)
            {
                int tableIndex = Mathf.Max(0, tableNames.IndexOf(tableName.stringValue));
                int newTableIndex = EditorGUI.Popup(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                    "Table", tableIndex, tableNames.ToArray());

                // If table changed, reset
                if (newTableIndex != tableIndex)
                {
                    columnName.stringValue = "";
                    recordID.intValue = 0;
                    value.stringValue = "";
                }

                tableName.stringValue = tableNames[newTableIndex];
                yPos += LINE_HEIGHT + SPACING;

                // Column Name Dropdown
                if (tableColumns.ContainsKey(tableName.stringValue))
                {
                    var columns = tableColumns[tableName.stringValue];
                    int columnIndex = Mathf.Max(0, columns.IndexOf(columnName.stringValue));
                    int newColumnIndex = EditorGUI.Popup(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                        "Column", columnIndex, columns.ToArray());

                    string newColumnName = columns[newColumnIndex];

                    // If column changed, load records
                    if (newColumnIndex != columnIndex)
                    {
                        LoadColumnRecords(tableName.stringValue, newColumnName);
                        recordID.intValue = 0;
                        value.stringValue = "";
                    }

                    columnName.stringValue = newColumnName;
                    yPos += LINE_HEIGHT + SPACING;
                }
                else
                {
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, LINE_HEIGHT), columnName);
                    yPos += LINE_HEIGHT + SPACING;
                }

                // Value Dropdown (shows content, auto-fills ID)
                string cacheKey = GetCacheKey(tableName.stringValue, columnName.stringValue);
                if (tableRecords.ContainsKey(cacheKey) && !string.IsNullOrEmpty(columnName.stringValue))
                {
                    var records = tableRecords[cacheKey];

                    if (records.Count > 0)
                    {
                        var ids = records.Keys.ToList();
                        var values = records.Values.ToList();

                        // Create display options showing just the value
                        string[] displayOptions = values.ToArray();

                        // Find current index by value or ID
                        int currentIndex = values.IndexOf(value.stringValue);
                        if (currentIndex < 0)
                        {
                            currentIndex = ids.IndexOf(recordID.intValue);
                        }
                        if (currentIndex < 0) currentIndex = 0;

                        // Show dropdown
                        int newIndex = EditorGUI.Popup(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                            "Value", currentIndex, displayOptions);

                        // Update both value and ID
                        if (newIndex >= 0 && newIndex < ids.Count)
                        {
                            value.stringValue = values[newIndex];
                            recordID.intValue = ids[newIndex];
                        }
                        yPos += LINE_HEIGHT + SPACING;

                        // Show ID as read-only
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.IntField(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                            "Record ID", recordID.intValue);
                        EditorGUI.EndDisabledGroup();
                        yPos += LINE_HEIGHT + SPACING;
                    }
                    else
                    {
                        EditorGUI.LabelField(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                            "Value", "No records found");
                        yPos += LINE_HEIGHT + SPACING;
                    }
                }
                else if (!string.IsNullOrEmpty(columnName.stringValue))
                {
                    // Load button if records not loaded
                    if (GUI.Button(new Rect(position.x, yPos, position.width, LINE_HEIGHT), "Load Values"))
                    {
                        LoadColumnRecords(tableName.stringValue, columnName.stringValue);
                    }
                    yPos += LINE_HEIGHT + SPACING;
                }
                else
                {
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, LINE_HEIGHT), value);
                    yPos += LINE_HEIGHT + SPACING;
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, LINE_HEIGHT), recordID);
                    yPos += LINE_HEIGHT + SPACING;
                }

                // OR separator
                EditorGUI.LabelField(new Rect(position.x, yPos, position.width, LINE_HEIGHT), "--- OR use WHERE clause ---");
                yPos += LINE_HEIGHT + SPACING;

                // Where Column Dropdown
                if (tableColumns.ContainsKey(tableName.stringValue))
                {
                    var columns = tableColumns[tableName.stringValue];
                    int whereColumnIndex = Mathf.Max(0, columns.IndexOf(whereColumn.stringValue));
                    whereColumnIndex = EditorGUI.Popup(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                        "Where Column", whereColumnIndex, columns.ToArray());
                    whereColumn.stringValue = columns[whereColumnIndex];
                }
                else
                {
                    EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, LINE_HEIGHT), whereColumn);
                }
                yPos += LINE_HEIGHT + SPACING;

                // Where Value (text field)
                EditorGUI.PropertyField(new Rect(position.x, yPos, position.width, LINE_HEIGHT), whereValue);
            }
            else
            {
                EditorGUI.LabelField(new Rect(position.x, yPos, position.width, LINE_HEIGHT),
                    "Database not found. Make sure ltlrn01.db is in StreamingAssets.");
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return LINE_HEIGHT;

        return (LINE_HEIGHT + SPACING) * 8 + LINE_HEIGHT; // 8 fields + foldout
    }

    private static string GetCacheKey(string tableName, string columnName)
    {
        return $"{tableName}::{columnName}";
    }

    private static void LoadColumnRecords(string tableName, string columnName)
    {
        if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(columnName))
            return;

        string dbPath = Path.Combine(Application.dataPath, "StreamingAssets", "ltlrn01.db");
        string cacheKey = GetCacheKey(tableName, columnName);

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var records = new Dictionary<int, string>();

                // Query with column name in brackets to handle reserved words
                var results = connection.Query<RecordData>(
                    $"SELECT ID, [{columnName}] as Value FROM [{tableName}]"
                );

                foreach (var row in results)
                {
                    records[row.ID] = row.Value ?? "(null)";
                }

                tableRecords[cacheKey] = records;
                Debug.Log($"✓ Loaded {records.Count} records from {tableName}.{columnName}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading records: {ex.Message}");
            tableRecords[cacheKey] = new Dictionary<int, string>();
        }
    }

    private static void LoadDatabaseInfo()
    {
        if (isLoaded) return;

        string dbPath = Path.Combine(Application.dataPath, "StreamingAssets", "ltlrn01.db");

        if (!File.Exists(dbPath))
        {
            Debug.LogWarning($"Database not found at: {dbPath}");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                // Get all table names
                var tables = connection.Query<TableInfo>("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");
                tableNames = tables.Select(t => t.name).ToList();

                // Get columns for each table
                foreach (var tableName in tableNames)
                {
                    var columns = connection.GetTableInfo(tableName);
                    tableColumns[tableName] = columns.Select(c => c.Name).ToList();
                }

                isLoaded = true;
                Debug.Log($"Loaded {tableNames.Count} tables from database");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading database info: {ex.Message}");
        }
    }

    private class TableInfo
    {
        public string name { get; set; }
    }

    private class RecordData
    {
        public int ID { get; set; }
        public string Value { get; set; }
    }
}
#endif