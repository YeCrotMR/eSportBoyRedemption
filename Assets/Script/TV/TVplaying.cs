using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TVplaying : MonoBehaviour
{                
    public AudioSource aipc;
    private bool hasPlayed = false;
    public static bool isplaying = false;
    void Start()
    {

    }

    public void Play()
    {   
        if(TurnTV.tvon){
            isplaying = true;
            if(!hasPlayed){

                aipc.Play();
                hasPlayed = true;
            }
        }

        
    }

    void Update(){
        if(Timer.TimerFinished){
            aipc.Stop();
        }
        if(Timer.TimerFinished || SceneManager.GetActiveScene().name == "start" || SceneManager.GetActiveScene().name == "gameover"){
            aipc.Stop();
            hasPlayed = false;
        }
    }

}
