using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldIDListener : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private TMP_InputField inputField;
    
    public void Start()
    {
        //Adds a listener to the main input field and invokes a method when the value changes.
        inputField.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
    }

    // Invoked when the value of the text field changes.
    public void ValueChangeCheck()
    {
        Debug.Log($"[GameSettings] Changed Subject ID to '{inputField.text}'");
        gameSettings.subjectID = inputField.text;
    }

    public void AddCharacter(string c)
    {
        inputField.text += c;
    }

    public void DelLastCharacter()
    {
        string fieldText = inputField.text;
        inputField.text = fieldText.Remove(fieldText.Length - 1); ;
    }

    public void DelAllCharacter()
    {
        inputField.text = "";
    }
}
