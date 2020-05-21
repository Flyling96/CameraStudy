using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;
using System;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that adds a final offset to the camera
/// </summary>
[AddComponentMenu("")] // Hide in menu
#if UNITY_2018_3_OR_NEWER
[ExecuteAlways]
#else
[ExecuteInEditMode]
#endif
public class CinemachineCameraOffset : CinemachineExtension
{
    [Tooltip("Offset the camera's position by this much (camera space)")]
    public Vector3 m_Offset = Vector3.zero;

    [Tooltip("When to apply the offset")]
    public CinemachineCore.Stage m_ApplyAfter = CinemachineCore.Stage.Aim;

    [Tooltip("If applying offset after aim, re-adjust the aim to preserve the screen position"
        + " of the LookAt target as much as possible")]
    public bool m_PreserveComposition;


    [Serializable]
    public struct AutoAdjust
    {
        public enum AutoAdjustMode
        {
            NearFar,
            Turn,
        }

        public AutoAdjust(AutoAdjustMode mode)
        {
            m_AutoAdjustMode = (AutoAdjustMode)(1 << (int)mode);
            m_AdjustNearFarRange = new Vector2(2,3);
            m_AdjustDampingZ = 0;
            m_TurnAxisName = "";
            m_AdjustRangeX = Vector2.zero;
            m_AdjustDampingX = 0;
            m_IsTrigger = false;
            m_TriggerTarget = new Vector3(0.7f, 0, 0);
        }

        public AutoAdjustMode m_AutoAdjustMode;
        [Tooltip("根据远近自动调整的范围")]
        public Vector2 m_AdjustNearFarRange;
        public float m_AdjustDampingZ;

        public string m_TurnAxisName;
        public Vector3 m_AdjustRangeX;
        public float m_AdjustDampingX;

        public bool m_IsTrigger;
        public Vector3 m_TriggerTarget;

        public void AutoAdjustOffset(CinemachineVirtualCameraBase vcam,
             CameraState state, float deltaTime, ref Vector3 offset)
        {
            //在特定触发器的状态下不调整
            if (m_IsTrigger)
            {
                Vector3 targetOffset = m_TriggerTarget - offset;
                offset += Damper.Damp(targetOffset, m_AdjustDampingX, deltaTime);
                return;
            }

            if (IsSelectAutoAdjustMode(AutoAdjust.AutoAdjustMode.NearFar))
            {
                if (m_AdjustNearFarRange.x >= m_AdjustNearFarRange.y) return;
                Quaternion inverseRawOrientation = Quaternion.Inverse(state.RawOrientation);
                float dis = (inverseRawOrientation * (vcam.LookAt.position - state.FinalPosition)).z;
                if (dis < m_AdjustNearFarRange.x)
                {
                    dis = dis - m_AdjustNearFarRange.x;
                    offset.z += Damper.Damp(dis, m_AdjustDampingZ, deltaTime);
                }
                else if (dis > m_AdjustNearFarRange.y)
                {
                    dis = dis - m_AdjustNearFarRange.y;
                    offset.z += Damper.Damp(dis, m_AdjustDampingZ, deltaTime);
                }
            }

            if (IsSelectAutoAdjustMode(AutoAdjustMode.Turn))
            {
                if (m_TurnAxisName == "" || m_AdjustRangeX.x >= m_AdjustRangeX.y) return;
                float value = CinemachineCore.GetInputAxis(m_TurnAxisName);
                if(value < 0)
                {
                    float target = Mathf.Lerp(m_AdjustRangeX.y,m_AdjustRangeX.x, -value);
                    float targetOffset = target - offset.x;
                    offset.x += Damper.Damp(targetOffset, m_AdjustDampingX, deltaTime);
                }
                else if(value > 0)
                {
                    float target = Mathf.Lerp(m_AdjustRangeX.y, m_AdjustRangeX.z, value);
                    float targetOffset = target - offset.x;
                    offset.x += Damper.Damp(targetOffset, m_AdjustDampingX, deltaTime);
                }
                else
                {
                    float targetOffset = m_AdjustRangeX.y - offset.x;
                    offset.x += Damper.Damp(targetOffset, m_AdjustDampingX, deltaTime);
                }
            }
        }

        private bool IsSelectAutoAdjustMode(AutoAdjustMode type)
        {
            int index = 1 << (int)type;
            int result = (int)m_AutoAdjustMode;
            if ((result & index) == index)
            {
                return true;
            }
            return false;
        }
    }

    public AutoAdjust m_AutoAdjust = new AutoAdjust(AutoAdjust.AutoAdjustMode.NearFar);

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == m_ApplyAfter)
        {
            bool preserveAim = m_PreserveComposition
                && state.HasLookAt && stage > CinemachineCore.Stage.Body;

            Vector3 screenOffset = Vector2.zero;
            if (preserveAim)
            {
                screenOffset = state.RawOrientation.GetCameraRotationToTarget(
                    state.ReferenceLookAt - state.CorrectedPosition, state.ReferenceUp);
            }

            Vector3 offset = state.RawOrientation * m_Offset;
            state.PositionCorrection += offset;
            if (!preserveAim)
            {
                state.ReferenceLookAt += offset;
            }
            else
            {
                var q = Quaternion.LookRotation(
                    state.ReferenceLookAt - state.CorrectedPosition, state.ReferenceUp);
                q = q.ApplyCameraRotation(-screenOffset, state.ReferenceUp);
                state.RawOrientation = q;
            }

            m_AutoAdjust.AutoAdjustOffset(vcam, state, deltaTime, ref m_Offset);

        }
    }

    public void SetTargetEnterTrigger(string offset)
    {
        string[] strs = offset.Split(',');
        if (strs.Length != 3) return;
        m_AutoAdjust.m_TriggerTarget = new Vector3(float.Parse(strs[0]),float.Parse(strs[1]),float.Parse(strs[2]));
        m_AutoAdjust.m_IsTrigger = true;
    }

    public void SetTargetExitTrigger(string offset)
    {
        string[] strs = offset.Split(',');
        if (strs.Length != 3) return;
        m_AutoAdjust.m_TriggerTarget = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
        m_AutoAdjust.m_IsTrigger = false;
    }


}

