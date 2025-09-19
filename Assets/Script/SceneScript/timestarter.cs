using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timestarter : MonoBehaviour
{
    public DialogueSystem chat;
    private bool triggered = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(chat.dialogueFinished == true && triggered == false && GlobalTimer.IsRunning == false){
            GlobalTimer.ResetTimer();
            GlobalTimer.StartTimer(true);
            GuideManager.Instance.ShowHintByID("pressE");
            triggered = true;
        }
    }
}
