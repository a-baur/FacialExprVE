using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;

public class GazeFocusLogger : MonoBehaviour
{
    [SerializeField] public bool activateGazeTracking;
    [Range(0.05f, 0.25f)] [SerializeField] private float trackingRate = 0.1f; // # of checks for focused object per second 

    private Dictionary<Transform, float> focusObjects = new Dictionary<Transform, float>();
    private string _subjectID;
    private string _logFolder;
    private string _logFile;

    private FocusInfo _focusInfo;
    private readonly float MaxDistance = 20;
    private readonly GazeIndex[] _gazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
    private static EyeData _eyeData = new EyeData();
    private bool _eyeCallbackRegistered = false;

    private bool active;


    public void StartLogging()
    {
        if (!activateGazeTracking | !SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        _subjectID = FindObjectOfType<GameSettings>().subjectID;
        _logFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Logging\\GazeFocusLogs";  // to documents folder
        // _logFolder = "Z:\\Bachelor\\Data\\Logging\\GazeFocusLogs"; // to network drive
        _logFile = _logFolder + $"\\subj_{_subjectID}.tsv";

        Debug.Log("[GazeFocusLogger] Starting logging in " + _logFile);

        InvokeRepeating(nameof(UpdateFocusedObjects), 0, trackingRate);
       
    }


    private void UpdateFocusedObjects()
    {
        // Only update if tracking is activated and objects to track focus on exist.
        if (!activateGazeTracking) return;

        // Framework checkup and callback instantiation
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && _eyeCallbackRegistered == false)
        {
            SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eyeCallbackRegistered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && _eyeCallbackRegistered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eyeCallbackRegistered = false;
        }

        // UpdateMeshes();  // Dynamicly update Mesh collider -> Hitbox performs way better

        // 
        foreach (GazeIndex index in _gazePriority)
        {
            Ray gazeRay;
            bool eyeFocus;
            int focus_layer_id = LayerMask.NameToLayer("GazeObject");

            if (_eyeCallbackRegistered)
                eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out _focusInfo, 0, MaxDistance, (1 << focus_layer_id), _eyeData);  // -> Bitshift like in example. Reason?
            else
                eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out _focusInfo, 0, MaxDistance, (1 << focus_layer_id));

            if (eyeFocus)
            {
                Transform focusedObj = _focusInfo.transform;

                Debug.Log("Focus Object: " + focusedObj);

                if (focusObjects.ContainsKey(focusedObj))
                {
                    focusObjects[focusedObj] += trackingRate;  // Count up time, every tick adds <tracking rate> seconds
                }
                else
                {
                    focusObjects[focusedObj] = trackingRate;
                }
                
                break;
            }
        }
    }

    private void UpdateMeshes()
    {
        foreach (Transform focusObj in focusObjects.Keys)
        {
            GameObject focusedGameObj = focusObj.gameObject;

            SkinnedMeshRenderer smr = focusedGameObj.GetComponent<SkinnedMeshRenderer>();
            Mesh bakeMesh = new Mesh();
            smr.BakeMesh(bakeMesh);
            focusedGameObj.AddComponent<MeshCollider>().sharedMesh = bakeMesh;
        }
    }

    private void OnApplicationQuit()
    {
        if (_logFile == null) return;

        String logString = String.Join(
            Environment.NewLine,
            focusObjects.Select(d => $"{d.Key}\t{d.Value}"));

        File.WriteAllText(_logFile, logString);
    }

    public void WriteLogs()
    {
        if (_logFile == null) return;

        String logString = String.Join(
            Environment.NewLine,
            focusObjects.Select(d => $"{d.Key}\t{d.Value}"));

        File.WriteAllText(_logFile, logString);
    }


    private void Release()
    {
        if (_eyeCallbackRegistered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eyeCallbackRegistered = false;
        }
    }


    private static void EyeCallback(ref EyeData eye_data)
    {
        _eyeData = eye_data;
    }


    public void AddFocusObject(Transform focusObj)
    {
        focusObjects.Add(focusObj, 0.0f);
    }

}