using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomPropertyDrawer(typeof(DM64))]
public class DM64Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null) return;
        EditorGUI.BeginProperty(position, label, property);

        var rawProperty = property.FindPropertyRelative("raw");

        float floatValue = DM64.RawToFloat(rawProperty.longValue);

        EditorGUI.BeginChangeCheck();
        floatValue = EditorGUI.FloatField(position, label, floatValue);

        if (EditorGUI.EndChangeCheck())
        {
            rawProperty.longValue = DM64.FloatToRaw(floatValue);
        }
        EditorGUI.EndProperty();
    }
}

