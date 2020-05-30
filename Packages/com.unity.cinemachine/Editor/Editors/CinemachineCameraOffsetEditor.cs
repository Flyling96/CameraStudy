using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Cinemachine.Editor
{
    [CustomEditor(typeof(CinemachineCameraOffset))]
    internal sealed class CinemachineCameraOffsetEditor : BaseEditor<CinemachineCameraOffset>
    {
        CinemachineCameraOffset.AutoAdjust def = new CinemachineCameraOffset.AutoAdjust();
        bool mExpanded;

        protected override List<string> GetExcludedPropertiesInInspector()
        {
            List<string> excluded = base.GetExcludedPropertiesInInspector();
            excluded.Add(FieldPath(x => x.m_AutoAdjust));
            return excluded;
        }


        public override void OnInspectorGUI()
        {
            BeginInspector();
            DrawRemainingPropertiesInInspector();
            EditorGUILayout.Space();
            DrawAutoAdjust(FindProperty(x => x.m_AutoAdjust));
        }

        void DrawAutoAdjust(SerializedProperty p)
        {
            mExpanded = EditorGUILayout.Foldout(mExpanded, "AutoAdjust");
            if(mExpanded)
            {
                SerializedProperty autoAdjustMode = p.FindPropertyRelative(()=> def.m_AutoAdjustMode);
                autoAdjustMode.intValue = EditorGUILayout.MaskField(autoAdjustMode.displayName, autoAdjustMode.intValue, autoAdjustMode.enumNames);

                int index = 1 << (int)CinemachineCameraOffset.AutoAdjust.AutoAdjustMode.Distance;
                bool isNearFarCamera = (autoAdjustMode.intValue & index) == index;
                if (isNearFarCamera)
                {
                    SerializedProperty property = p.FindPropertyRelative(() => def.m_AdjustNearFarRange);
                    EditorGUILayout.PropertyField(property);
                    property = p.FindPropertyRelative(() => def.m_AdjustDampingZ);
                    EditorGUILayout.PropertyField(property);
                    EditorGUILayout.Space();
                }

                index = 1 << (int)CinemachineCameraOffset.AutoAdjust.AutoAdjustMode.Turn;
                bool isTurn = (autoAdjustMode.intValue & index) == index;
                if (isTurn)
                {
                    SerializedProperty property = p.FindPropertyRelative(() => def.m_TurnAxisName);
                    EditorGUILayout.PropertyField(property);
                    property = p.FindPropertyRelative(() => def.m_AdjustRangeX);
                    EditorGUILayout.PropertyField(property);
                    property = p.FindPropertyRelative(() => def.m_AdjustDampingX);
                    EditorGUILayout.PropertyField(property);
                    EditorGUILayout.Space();
                }

                index = 1 << (int)CinemachineCameraOffset.AutoAdjust.AutoAdjustMode.RunFar;
                bool isRunFar = (autoAdjustMode.intValue & index) == index;
                if(isRunFar)
                {
                    SerializedProperty property = p.FindPropertyRelative(() => def.m_AdjustMinDic);
                    EditorGUILayout.PropertyField(property);
                    if(!isNearFarCamera)
                    {
                        property = p.FindPropertyRelative(() => def.m_AdjustDampingZ);
                        EditorGUILayout.PropertyField(property);
                    }
                }

                p.serializedObject.ApplyModifiedProperties();
            }
        }

    }
}
