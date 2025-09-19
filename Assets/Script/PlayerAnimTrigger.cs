using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimTrigger : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    [SerializeField] public int index; // Now editable from the Inspector
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetInteger("state",6);
    }

    void Update()
    {
        if (dialogueSystem.clickCount == index)
        {
            anim.SetInteger("state",5);
        }

        if (dialogueSystem.clickCount == 25)
        {
            anim.SetInteger("state",0);
        }
    }
}
