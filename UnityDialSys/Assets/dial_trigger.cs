using UnityEngine;
using System;

[Serializable]
public class dial_info
{
    public string character_name;
    public string location;
}

public class dial_trigger: MonoBehaviour
{
    public dial_info DialInfo;
    public void TriggerDialogue()
    {
        dial_ui_info.location = DialInfo.location;
        dial_ui_info.character_name = DialInfo.character_name;
        dial_ui_info.dial_visibility = true;
        dial_ui_info.opendial = true;
        Debug.Log("button_triggered!!");
        Debug.Log(DialInfo.character_name);
        Debug.Log(DialInfo.location);
    }
    private void Update()
    {
        
    }
}
