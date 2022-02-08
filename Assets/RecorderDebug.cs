using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecorderDebug : MonoBehaviour
{
    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            if (Microphone.IsRecording(device)) Debug.Log("Recording with " + device);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
