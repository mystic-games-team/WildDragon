﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Interactivity : MonoBehaviour
{
    public enum InteractingStates
    {
        NO_RANGE_TO_INTERACT,
        WAITING_INTERACTION,
        INTERACTING,

        NONE
    }

    [System.Serializable]
    public struct DialogOptions
    {
        public string option_text;
        public int next_node;
    }

    [System.Serializable]
    public struct DialogNodes
    {
        public int node_id;
        public List<string> node_text;
        public List<DialogOptions> options;
        public bool safe_node;
    }

    public GameObject interacting_anim;
    public GameObject dialog_panel;
    public DialogManager.DialogType dialog_type = DialogManager.DialogType.NONE;
    public InteractingStates interacting_state = InteractingStates.NO_RANGE_TO_INTERACT;
    public string npc_name;
    public Sprite portrait_npc;
    private GameObject copy_panel;
    [SerializeField]
    public List<DialogNodes> dialog_node = new List<DialogNodes>();

    [HideInInspector]
    public DialogNodes actual_node;

    // Start is called before the first frame update
    public void Start()
    {
        copy_panel = dialog_panel;

        int start_node = GameObject.Find("GameController").GetComponent<GameController>().SetNPCStartNode(dialog_type, dialog_node[0].node_id);

        if (start_node != -1) // player has talked with that npc and has progressd the story
        {
            for (int i = 0;i<dialog_node.Count;++i)
            {
                if (dialog_node[i].node_id == start_node)
                {
                    actual_node = dialog_node[i];
                    break;
                }
            }
        }
        else // first talking time or the npc has no interactivity 
        {
            actual_node = dialog_node[0];
        }
    }

    public void Update()
    {
        if (interacting_state == InteractingStates.WAITING_INTERACTION) // player is in range to talk
        {
            if (Input.GetButtonDown("Interact")) // player begins talking
            {
                interacting_state = InteractingStates.INTERACTING;
                dialog_panel = Instantiate(copy_panel); // create the prefab of the dialog panel
                
                // set the dialog data and pass it to the dialog manager
                DialogManager.DialogData data;
                data.actual_node = actual_node;
                data.dialog_node = dialog_node;
                data.dialog_panel = dialog_panel;
                data.interactive_target = gameObject;
                data.portrait_npc = portrait_npc;
                data.name_npc = npc_name;
                GameObject.Find("DialogManager").SendMessage("StartDialog", data);
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") // player in range now
        {
            interacting_state = InteractingStates.WAITING_INTERACTION;
            GameObject obj = Instantiate(interacting_anim);
            obj.transform.position = new Vector3(
                transform.position.x + GetComponentInParent<SpriteRenderer>().size.x / 2 - obj.GetComponent<SpriteRenderer>().size.x / 2,
                transform.position.y,
                transform.position.z);
            Debug.Log(GetComponentInParent<SpriteRenderer>().size.x);
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player") // player has left the range
        {
            interacting_state = InteractingStates.NO_RANGE_TO_INTERACT;
            Destroy(GameObject.Find("InteractingAnim(Clone)"));
            if (dialog_panel != null)
                Destroy(dialog_panel);
        }
    }

    public void SetToWaitingInteraction()
    {
        GameObject.Find("GameController").GetComponent<GameController>().SaveNPC(actual_node.node_id, dialog_type);
        interacting_state = InteractingStates.WAITING_INTERACTION;
    }
}
