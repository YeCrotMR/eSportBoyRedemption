using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTrigger : MonoBehaviour
{
    public AudioSource teleportSound; // 从 Inspector 拖入第一个音效
    public AudioSource closeSound;    // 第二个音效
    public DialogueSystem dialogueSystem;
    public NPCMultiMover npc;
    [SerializeField] public int index;
    private Animator anim;
    private bool triggered = false;
    private bool hasPlayedCloseSound = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        TriggerTeleportIfNeeded();
        ResetStateIfFinished();
    }

    void TriggerTeleportIfNeeded()
    {
        if (!triggered && dialogueSystem != null && dialogueSystem.clickCount == index)
        {
            triggered = true;
            Debug.Log("begin!");
            teleportSound?.Play();
            anim.SetInteger("door", 1);
        }
    }

    void ResetStateIfFinished()
    {
        if (npc != null && npc.hasFinishedMoving && !hasPlayedCloseSound)
        {
            Debug.Log("卧槽!");
            anim.SetInteger("door", 2);
            closeSound?.Play();
            hasPlayedCloseSound = true;
        }
    }
}
