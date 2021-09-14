using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;

public class GazeFocusLogger : MonoBehaviour
{
    [SerializeField] public bool activateGazeTracking;
    [SerializeField] private string logFolder = "C:/Logging";
    [Range(0.05f, 0.25f)] [SerializeField] private float trackingRate = 0.1f; // # of checks for focused object per second 
    [SerializeField] private List<Transform> focusObjects;

    private string _subjectID;
    private string _logFile;
    private FocusInfo _focusInfo;
    private readonly float MaxDistance = 20;
    private readonly GazeIndex[] _gazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };
    private static EyeData _eyeData = new EyeData();
    private bool _eyeCallbackRegistered = false;

    private List<string> _focusHistory;

    private bool active;

    public void StartLogging()
    {
        if (!activateGazeTracking | !SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        _subjectID = FindObjectOfType<GameSettings>().subjectID;
        _logFile = logFolder + $"\\subj_{_subjectID}.txt";

        // Create file or clear file of potential contents
        using (FileStream fs = File.Open(_logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            lock (fs)
            {
                fs.SetLength(0);
            }
        }

        InvokeRepeating(nameof(UpdateFocusedObjects), 0, trackingRate);
       
    }

    private void UpdateFocusedObjects()
    {
        // Only update if tracking is activated and objects to track focus on exist.
        if (!activateGazeTracking | !focusObjects.Any()) return;
        
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

        // 
        foreach (GazeIndex index in _gazePriority)
        {
            Ray gazeRay;
            bool eyeFocus;
            
            if (_eyeCallbackRegistered)
                eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out _focusInfo, 0, MaxDistance,  _eyeData);
            else
                eyeFocus = SRanipal_Eye.Focus(index, out gazeRay, out _focusInfo, 0, MaxDistance);

            if (eyeFocus)
            {
                Transform focusedObj = _focusInfo.transform;
                _focusHistory.Add(focusObjects.Contains(focusedObj) ? focusedObj.name : "");
                break;
            }
        }
    }
    private void Release()
    {
        if (_eyeCallbackRegistered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            _eyeCallbackRegistered = false;
        }

        // Write focus history into log file
        using (StreamWriter _sw = File.CreateText(_logFile))
        {
            foreach (string focusedObj in _focusHistory)
            {
                _sw.Write(focusedObj + '\n');
            }
        }
    }
    private static void EyeCallback(ref EyeData eye_data)
    {
        _eyeData = eye_data;
    }

    public void AddFocusObject(Transform focusObj)
    {
        focusObjects.Add(focusObj);
    }
}