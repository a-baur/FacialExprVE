using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DebugFunctions : MonoBehaviour
{
    public GameObject Character;
    
    private void OnGUI()
    {
        if (GUILayout.Button("BUILD!"))
        {
            bool success = Character.GetComponent<RigBuilder>().Build();
            Debug.Log("Build Success: " + success);
        }
        
        if (GUILayout.Button("CLEAR!"))
        {
            Character.GetComponent<RigBuilder>().Clear();
            Debug.Log("Cleared Successfully");
        }
        
    }

}
