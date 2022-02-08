using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;
using Avatar;
using Networking;

public class GameSettings : MonoBehaviour
{
    public char gender;
    public bool useLipSync;
    public bool enableEyeTracking;
    public bool enableLipTracking;
    public string subjectID = "";
    private bool _lastEyeTrackingState;
    private bool _lastLipTrackingState;
    
    // Start is called before the first frame update
    void Start()
    {
        /*Process[] pname = Process.GetProcessesByName("sr_runtime");
        if (pname.Length == 0)
        {
            Debug.LogError("[SRanipal] Runtime Not Running. Facial Tracking Will Not Be Available!");
            enableEyeTracking = false;
            enableLipTracking = false;
        }*/

        if (!IsVRAvailable())
        {
            Application.Quit();
        }
        
        _lastEyeTrackingState = enableEyeTracking;
        _lastLipTrackingState = enableLipTracking;

        SetTracking();
    }

    private void Update()
    {
        if (enableEyeTracking != _lastEyeTrackingState || enableLipTracking != _lastLipTrackingState)
        {
            SetTracking();
        }
    }


    private void Awake () 
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ActivateLogging()
    {
        FaceShapeLogger faceShapeLogger = FindObjectOfType<FaceShapeLogger>();
        GazeFocusLogger gazeFocusLogger = FindObjectOfType<GazeFocusLogger>();

        faceShapeLogger.StartLogging();
        gazeFocusLogger.StartLogging();

    }

    private void SetTracking()
    {
        // (De)activate gameobjects by settings.
        // EYE SETTINGS
        var eyeAvatarV1 = FindObjectOfType<SRanipal_AvatarEyeSample>(true);
        var eyeAvatarV2 = FindObjectOfType<SRanipal_AvatarEyeSample_v2>(true);
        var eyeAvatarNetwork = FindObjectOfType<NetworkSRanipalEye>(true);
        var eyeFramework = FindObjectOfType<SRanipal_Eye_Framework>(true);
        var eyeSettings = FindObjectOfType<SRanipal_EyeSettingSample>(true);

        if (eyeAvatarV1) eyeAvatarV1.gameObject.GetComponent<SRanipal_AvatarEyeSample>().enabled = enableEyeTracking;
        if (eyeAvatarV2) eyeAvatarV2.gameObject.GetComponent<SRanipal_AvatarEyeSample_v2>().enabled = enableEyeTracking;
        if (eyeAvatarNetwork) eyeAvatarNetwork.gameObject.GetComponent<NetworkSRanipalEye>().enabled = enableEyeTracking;
        if (eyeFramework) eyeFramework.gameObject.GetComponent<SRanipal_Eye_Framework>().enabled = enableEyeTracking;
        if (eyeSettings) eyeSettings.gameObject.GetComponent<SRanipal_EyeSettingSample>().enabled = enableEyeTracking;

        // LIP SETTINGS
        var lipAvatarV1 = FindObjectOfType<SRanipal_AvatarLipSample>(true);
        var lipAvatarV2 = FindObjectOfType<SRanipal_AvatarLipSample_v2>(true);
        var lipAvatarNetwork = FindObjectOfType<NetworkSRanipalLip>(true);
        var lipFramework = FindObjectOfType<SRanipal_Lip_Framework>(true);

        if (lipAvatarV1) lipAvatarV1.gameObject.GetComponent<SRanipal_AvatarLipSample>().enabled = enableLipTracking;
        if (lipAvatarV2) lipAvatarV2.gameObject.GetComponent<SRanipal_AvatarLipSample_v2>().enabled = enableLipTracking;
        if (lipAvatarNetwork) lipAvatarNetwork.gameObject.GetComponent<NetworkSRanipalLip>().enabled = enableLipTracking;
        if (lipFramework) lipFramework.gameObject.GetComponent<SRanipal_Lip_Framework>().enabled = enableLipTracking;
    }

    private bool IsVRAvailable()
    {
        bool xrDeviceFound = false;
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displaySubsystems);

        foreach (var subsystem in displaySubsystems)
        {
            Debug.Log("[GameSettings] XR Found Device: " + subsystem.subsystemDescriptor.id);
            xrDeviceFound = true;
        }
        
        return xrDeviceFound;
    }

    private void OnGUI()
    {
        GUILayout.Label("Subject ID: ");
        subjectID = GUILayout.TextField(subjectID, 25);

        if (GUILayout.Button("Start Logging"))
        {
            if (subjectID.Length == 0)
            {
                Debug.LogError("[GameSettings] Must assign subject id before logging.");
                return;
            }

            FaceShapeLogger fsl = FindObjectOfType<FaceShapeLogger>();
            GazeFocusLogger gfl = FindObjectOfType<GazeFocusLogger>();
            if (fsl && gfl)
            {
                fsl.StartLogging();
                gfl.StartLogging();
            }
            else
            {
                if (!fsl) Debug.LogError("FaceShapeLogger not found!");
                if (!gfl) Debug.LogError("GazeFocusLogger not found!");
            }
        }

        if (GUILayout.Button("Write GazeFocus Logs"))
        {
            GazeFocusLogger gfl = FindObjectOfType<GazeFocusLogger>();
            if (gfl) gfl.WriteLogs();
            Debug.LogError("Written GazeFocus logs");
        }

        if (GUILayout.Button("Quit Application"))
        {
            #if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        

    }
}


