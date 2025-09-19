using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EventTrigger : MonoBehaviour
{
    [Header("计时结束事件")]
    public UnityEvent onTimerEnd;
    public UnityEvent onTimerEnd2;
    private GameObject Son;
    public NPCcontinueMover backtosleep;

    public UniversalExpressionEvaluator evaluator;
    readonly Dictionary<string, object> vars = new();
    // Start is called before the first frame update
    void Start()
    {
        Son = GameObject.FindGameObjectWithTag("Player");
        vars["finished"] = Timer.TimerFinished  && GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && !backtosleep.hasFinishedMoving && Timer.hasStarted;
    }

    // Update is called once per frame
    void Update()
    {
        Son = GameObject.FindGameObjectWithTag("Player");
        vars["finished"] = Timer.TimerFinished  && GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && !backtosleep.hasFinishedMoving && Timer.hasStarted;
        if(evaluator != null && evaluator.EvaluateBool(vars)){
            if (Son != null && Son.transform.position.y <= -246.0019f)
                    {
                            onTimerEnd?.Invoke();
                    }
                    else
                    {
                            onTimerEnd2?.Invoke();
                    }
        }
    }
}
