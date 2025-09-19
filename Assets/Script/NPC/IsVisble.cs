using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsVisble : MonoBehaviour
{
    public bool disactive = false;
    public bool disvisble = false;
    public bool timevisble = false;
    public bool timeactive = false;
    public GameObject npc;
    public Timer timer;
    public UniversalExpressionEvaluator evaluator;
    readonly Dictionary<string, object> vars = new();
    // Start is called before the first frame update
    void Start()
    {
        timer = FindObjectOfType<Timer>();
        vars["hasgoout"] = NPCcontinueMover.hasgoout == true && !Timer.TimerFinished && Timer.hasStarted && GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime;
        vars["triggertime"] = NPCcontinueMover.hasgoout == true && !Timer.TimerFinished;
        if(disactive == true && evaluator.EvaluateBool(vars)){
           npc.SetActive(false); // 更保险
        }

        if(timevisble == true && evaluator != null ){
            if(evaluator.EvaluateBool(vars)){
                gameObject.SetActive(true);
            }else if(!NPCcontinueMover.doisMoving){
                gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer = FindObjectOfType<Timer>();
        Debug.Log("符合第一条件"+(timer != null && evaluator.EvaluateBool(vars)));
        Debug.Log("妈妈应该出现吗？"+(NPCcontinueMover.hasgoout == true && !Timer.TimerFinished && Timer.hasStarted));
        
        vars["hasgoout"] = NPCcontinueMover.hasgoout == true && !Timer.TimerFinished && Timer.hasStarted && GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime;
        vars["triggertime"] = NPCcontinueMover.hasgoout == true && !Timer.TimerFinished;

        if(disvisble == true && evaluator != null && evaluator.EvaluateBool(vars)){
            SetVisible(false);
        }

        // if(timevisble == true && evaluator != null ){
        //     if(evaluator.EvaluateBool(vars)){
        //         SetVisible(true);
        //     }else if(!NPCcontinueMover.doisMoving){
        //         Debug.Log("你妈死了");
        //         SetVisible(false);
        //     }
        // }

        if(timeactive == true && evaluator.EvaluateBool(vars)){
           npc.SetActive(true); // 更保险
        }
        
        if(timer != null && evaluator.EvaluateBool(vars)){
            if(!Timer.hasStarted){
                timer.StartTimer();
                Debug.Log("我尽立了");
                }
            if(timer.currentTime <= 0f){
                npc.SetActive(true);
            }
        }
        
    }

    void SetVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
            renderer.enabled = visible;

        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
            collider.enabled = visible;
    }

}
