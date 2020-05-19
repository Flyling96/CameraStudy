using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Cinemachine.Editor
{
    [CustomPropertyDrawer(typeof(AxisLimitPropertyAttribute))]
    internal sealed class AxisLimitPropertyDrawer : PropertyDrawer
    {
        const int vSpace = 2;
        bool mExpanded = true;
        AxisState.AxisLimit def = new AxisState.AxisLimit();

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            rect.height = height;
            mExpanded = EditorGUI.Foldout(rect, mExpanded, label, true);
            if (mExpanded)
            {
                ++EditorGUI.indentLevel;

                rect.y += height + vSpace;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative(() => def.m_LimitMode));
                if(IsAxisLimit(property))
                {
                   rect.y += height + vSpace;
                   EditorGUI.PropertyField(rect, property.FindPropertyRelative(() => def.m_RemoveLimitMode));
                   rect.y += height + vSpace;
                   InspectorUtility.MultiPropertyOnLine(
                   rect, null,
                   new[] { property.FindPropertyRelative(() => def.m_LookTarget),
                            property.FindPropertyRelative(() => def.m_Camera)},
                   new[] { GUIContent.none, null });
                    rect.y += height + vSpace;
                    InspectorUtility.MultiPropertyOnLine(
                   rect, null,
                   new[] { property.FindPropertyRelative(() => def.m_AxisVector),
                            property.FindPropertyRelative(() => def.m_LimitRange)},
                   new[] { GUIContent.none, null });

                }
                --EditorGUI.indentLevel;
            }

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + vSpace;
            if (mExpanded)
            {
                int lines = 2;
                if(IsAxisLimit(property))
                {
                    lines = 5;
                }
                height *= lines;
            }
            return height - vSpace;
        }

        bool IsAxisLimit(SerializedProperty property)
        {
            var mode = property.FindPropertyRelative(() => def.m_LimitMode);
            var value = (AxisState.AxisLimit.LimitMode)
                (System.Enum.GetValues(typeof(AxisState.AxisLimit.LimitMode))).GetValue(mode.enumValueIndex);
            return value == AxisState.AxisLimit.LimitMode.SceneLook;
        }

    }
}
