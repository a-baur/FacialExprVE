//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
//========= Modified for networking with Photon

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using Photon.Pun;
using Unity.VisualScripting;


namespace ViveSR.anipal.Eye
{
    public class NetworkSRanipalEye_v1 : MonoBehaviour
    {
        [SerializeField] private Transform[] EyesModels = new Transform[0];
        [SerializeField] private Transform Head;
        [SerializeField] private List<EyeShapeTable> EyeShapeTables;
        /// <summary>
        /// Customize this curve to fit the blend shapes of your avatar.
        /// </summary>
        [SerializeField] private AnimationCurve EyebrowAnimationCurveUpper;
        /// <summary>
        /// Customize this curve to fit the blend shapes of your avatar.
        /// </summary>
        [SerializeField] private AnimationCurve EyebrowAnimationCurveLower;
        /// <summary>
        /// Customize this curve to fit the blend shapes of your avatar.
        /// </summary>
        [SerializeField] private AnimationCurve EyebrowAnimationCurveHorizontal;

        public bool NeededToGetData = true;
        private Dictionary<EyeShape, float> EyeWeightings = new Dictionary<EyeShape, float>();
        private AnimationCurve[] EyebrowAnimationCurves = new AnimationCurve[(int)EyeShape.Max];
        private const int NUM_OF_EYES = 2;
        private static EyeData eyeData = new EyeData();
        private bool eye_callback_registered = false;

        private PhotonView _photonView;

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
            if (!_photonView.IsMine) return;

            /**if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                Debug.LogError("[SRanipal] Eye disabled!");
                enabled = false;
                return;
            }*/

            SetEyesModels(EyesModels[0], EyesModels[1]);
            SetEyeShapeTables(EyeShapeTables);

            AnimationCurve[] curves = new AnimationCurve[(int)EyeShape.Max];
            for (int i = 0; i < EyebrowAnimationCurves.Length; ++i)
            {
                if (i == (int)EyeShape.Eye_Left_Up || i == (int)EyeShape.Eye_Right_Up) curves[i] = EyebrowAnimationCurveUpper;
                else if (i == (int)EyeShape.Eye_Left_Down || i == (int)EyeShape.Eye_Right_Down) curves[i] = EyebrowAnimationCurveLower;
                else curves[i] = EyebrowAnimationCurveHorizontal;
            }
            SetEyeShapeAnimationCurves(curves);
        }

        private void Update()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

            if (!NeededToGetData | !_photonView.IsMine) return;

            if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
            {
                SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
                eye_callback_registered = true;
            }
            else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
            {
                SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
                eye_callback_registered = false;
            }
            else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false)
                SRanipal_Eye_API.GetEyeData(ref eyeData);

            bool isLeftEyeActive = false;
            bool isRightEyeAcitve = false;
            if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
            {
                isLeftEyeActive = eyeData.no_user;
                isRightEyeAcitve = eyeData.no_user;
            }
            else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
            {
                isLeftEyeActive = true;
                isRightEyeAcitve = true;
            }

            if (isLeftEyeActive || isRightEyeAcitve)
            {
                if (eye_callback_registered == true)
                    SRanipal_Eye.GetEyeWeightings(out EyeWeightings, eyeData);
                else
                    SRanipal_Eye.GetEyeWeightings(out EyeWeightings);

                Dictionary<byte, float> binEyeWeightings = BinaryEyeWeightings(EyeWeightings);
                _photonView.RPC(nameof(UpdateEyeShapes), RpcTarget.All, binEyeWeightings); 
            }
            else
            {
                for (int i = 0; i < (int)EyeShape.Max; ++i)
                {
                    bool isBlink = ((EyeShape)i == EyeShape.Eye_Left_Blink || (EyeShape)i == EyeShape.Eye_Right_Blink);
                    EyeWeightings[(EyeShape)i] = isBlink ? 1 : 0;
                }

                Dictionary<byte, float> binEyeWeightings = BinaryEyeWeightings(EyeWeightings);
                _photonView.RPC(nameof(UpdateEyeShapes), RpcTarget.All, binEyeWeightings);
                return;
            }

            Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal = Vector3.zero;
            if (eye_callback_registered == true)
            {
                if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
                else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
                else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            }
            else
            {
                if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
                else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }

            }
            
            _photonView.RPC(nameof(UpdateGazeRay), RpcTarget.All, GazeDirectionCombinedLocal);

        }

        private void Release()
        {
            if (eye_callback_registered == true)
            {
                SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
                eye_callback_registered = false;
            }
        }

        public void SetEyesModels(Transform leftEye, Transform rightEye)
        {
            if (leftEye != null && rightEye != null)
            {
                EyesModels = new Transform[NUM_OF_EYES] { leftEye, rightEye };
            }
        }

        // Initilize EyeShape-Blendshape mapping
        public void SetEyeShapeTables(List<EyeShapeTable> eyeShapeTables)
        {
            bool valid = true;
            if (eyeShapeTables == null)
            {
                valid = false;
            }
            else
            {
                for (int table = 0; table < eyeShapeTables.Count; ++table)
                {
                    SkinnedMeshRenderer tableSMR = eyeShapeTables[table].skinnedMeshRenderer;
                    if (eyeShapeTables[table].skinnedMeshRenderer == null)
                    {
                        valid = false;
                        break;
                    }
                    for (int shape = 0; shape < eyeShapeTables[table].eyeShapes.Length; ++shape)
                    {
                        EyeShape eyeShape = eyeShapeTables[table].eyeShapes[shape];
                        if (eyeShape > EyeShape.Max || eyeShape < 0)
                        {
                            string blendshape = tableSMR.sharedMesh.GetBlendShapeName(shape);
                            Debug.LogWarning($"[SRanipal] Blendshape '{blendshape}' assigned to invalid EyeShape '{eyeShape}'. Facial animation may not be accurate!");
                            continue;
                        }
                    }
                }
            }
            if (valid)
            {
                EyeShapeTables = eyeShapeTables;
            }
        }

        public void SetEyeShapeAnimationCurves(AnimationCurve[] eyebrowAnimationCurves)
        {
            if (eyebrowAnimationCurves.Length == (int)EyeShape.Max)
                EyebrowAnimationCurves = eyebrowAnimationCurves;
        }

        // Set direction of gaze.
        // Call on remote.
        [PunRPC]
        public void UpdateGazeRay(Vector3 gazeDirectionCombinedLocal)
        {
            for (int i = 0; i < EyesModels.Length; ++i)
            {
                // Use head direction as default, GazeDirection as deviation
                Vector3 target = Head.transform.TransformPoint(gazeDirectionCombinedLocal);
                EyesModels[i].LookAt(target);
            }
        }


        // Update all blendshapes models in EyeShapeTables.
        // Call on remote.
        [PunRPC]
        public void UpdateEyeShapes(Dictionary<byte, float> binEyeWeightings)
        {
            Dictionary<EyeShape, float> eyeWeightings = EnumEyeWeightings(binEyeWeightings);

            foreach (var table in EyeShapeTables)
                RenderModelEyeShape(table, eyeWeightings);
        }

        // Apply Eye Tracker weightings onto blendshape model
        private void RenderModelEyeShape(EyeShapeTable eyeShapeTable, Dictionary<EyeShape, float> weighting)
        {
            for (int i = 0; i < eyeShapeTable.eyeShapes.Length; ++i)
            {
                EyeShape eyeShape = eyeShapeTable.eyeShapes[i];
                if (eyeShape > EyeShape.Max || eyeShape < 0) continue;

                if (eyeShape == EyeShape.Eye_Left_Blink || eyeShape == EyeShape.Eye_Right_Blink)
                    eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[eyeShape] * 100f);
                else
                {
                    AnimationCurve curve = EyebrowAnimationCurves[(int)eyeShape];
                    eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, curve.Evaluate(weighting[eyeShape]) * 100f);
                }
            }
        }

        private static void EyeCallback(ref EyeData eye_data)
        {
            eyeData = eye_data;
        }

        /// <summary>
        /// Converts EyeWeightings to be serializable by Photon.
        /// </summary> 
        /// <param name="eyeWeightings">EyeWeightings to be converted.</param>
        /// <returns>
        /// Serializable Dictionary of EyeWeightings with byte keys.
        /// </returns>
        private Dictionary<byte, float> BinaryEyeWeightings(Dictionary<EyeShape, float> eyeWeightings)
        {
            Dictionary<byte, float> binEyeWeightings = new Dictionary<byte, float>();

            foreach (KeyValuePair<EyeShape, float> eyeWeight in eyeWeightings)
            {
                binEyeWeightings[(byte)eyeWeight.Key] = eyeWeight.Value;
            }

            return binEyeWeightings;
        }

        /// <summary>
        /// Converts EyeWeightings from binary to enumerated.
        /// </summary>
        /// <param name="eyeWeightings">EyeWeightings to be converted.</param>
        /// <returns>
        /// Dictionary with enumeration of EyeShapes.
        /// </returns>
        private Dictionary<EyeShape, float> EnumEyeWeightings(Dictionary<byte, float> binEyeWeightings)
        {
            Dictionary<EyeShape, float> eyeWeightings = new Dictionary<EyeShape, float>();

            foreach (KeyValuePair<byte, float> eyeWeight in binEyeWeightings)
            {
                eyeWeightings[(EyeShape)eyeWeight.Key] = eyeWeight.Value;
            }

            return eyeWeightings;
        }
    }
}