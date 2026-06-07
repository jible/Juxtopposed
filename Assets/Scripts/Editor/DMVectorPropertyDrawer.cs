using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomPropertyDrawer(typeof(DMVector))]
public class DMVectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);


        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);


        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        float margin = 4f;
        float sectionWidth = position.width / 2f ;
        var xRect = new Rect(position.x, position.y, sectionWidth, position.height);
        var yRect = new Rect(position.x + sectionWidth + margin, position.y, sectionWidth, position.height);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 14f;
        EditorGUI.PropertyField(xRect, property.FindPropertyRelative("x"), new GUIContent("X"));
        EditorGUI.PropertyField(yRect, property.FindPropertyRelative("y"), new GUIContent("Y"));

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}


