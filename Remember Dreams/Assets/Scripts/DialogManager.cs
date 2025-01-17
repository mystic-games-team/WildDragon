﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogManager : MonoBehaviour
{
    public enum DialogType
    {
        NPC_TEST,

        NONE
    }

    public struct DialogData
    {
        public GameObject dialog_panel;
        public Sprite portrait_npc;
        public List<Interactivity.DialogNodes> dialog_node;
        public Interactivity.DialogNodes actual_node;
        public GameObject interactive_target;
        public string name_npc;
    }

    [HideInInspector]
    public DialogData dialog_data;
    [HideInInspector]
    public bool active = false;
    private bool can_pass_dialog = true;

    private bool phrase_completed = false;
    private string hole_phrase;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (can_pass_dialog)
            {
                if (Input.GetButtonDown("Interact") && !dialog_data.dialog_panel.transform.Find("Option1").gameObject.activeInHierarchy)
                {
                    if (phrase_completed)
                        PerformNextPhrase();
                    else
                    {
                        phrase_completed = true;
                        StopCoroutine(WritePhrase(hole_phrase));
                        dialog_data.dialog_panel.transform.Find("Text").GetComponent<Text>().text = hole_phrase;
                    }
                }
            }
            else
                can_pass_dialog = true;
        }
    }

    public void StartDialog(DialogData data)
    {
        // set struct
        dialog_data = data;

        // always ui elemtns must have as a parent canvas
        dialog_data.dialog_panel.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
        //dialog_data.dialog_panel.transform.position = new Vector3(271, 75, dialog_data.dialog_panel.transform.position.z);

        // set the npc portrait image
        dialog_data.dialog_panel.transform.Find("PortraitNPC").GetComponent<Image>().sprite = dialog_data.portrait_npc;

        // set the npc_name
        dialog_data.dialog_panel.transform.Find("NPCName").GetComponent<Text>().text = dialog_data.name_npc;

        // set the first phrase
        //dialog_data.dialog_panel.transform.Find("Text").GetComponent<Text>().text = dialog_data.actual_node.node_text[0];
        StartCoroutine(WritePhrase(dialog_data.actual_node.node_text[0]));

        // disable options
        dialog_data.dialog_panel.transform.Find("Option1").gameObject.SetActive(false);
        dialog_data.dialog_panel.transform.Find("Option2").gameObject.SetActive(false);

        can_pass_dialog = false;
        active = true;
    }
    public void PerformNextPhrase()
    {
         for (int i = 0; i <= dialog_data.actual_node.node_text.Count; ++i)
            {
                if (dialog_data.actual_node.node_text[i] == dialog_data.dialog_panel.transform.Find("Text").GetComponent<Text>().text)
                {
                    if (i + 1 < dialog_data.actual_node.node_text.Count) // there's another phrase before player can talk
                    {
                        StartCoroutine(WritePhrase(dialog_data.actual_node.node_text[i + 1]));
                        break;
                    }
                    else // there's no more npc phrase in this node, player options appear
                    {
                        GameObject obj1 = dialog_data.dialog_panel.transform.Find("Option1").gameObject; 
                        obj1.SetActive(true);
                        obj1.GetComponent<Text>().text = dialog_data.actual_node.options[0].option_text;
                        obj1.GetComponent<Button>().onClick.AddListener(delegate { PerformFirstNodePhrase(dialog_data.actual_node.options[0].next_node); });
                        //obj1.GetComponent<Button>().Select();

                        EventSystem.current.SetSelectedGameObject(obj1, null);
                        can_pass_dialog = false;
                        if (dialog_data.actual_node.options.Count > 1) // player has more than 1 option to talk
                        {
                            GameObject obj2 = dialog_data.dialog_panel.transform.Find("Option2").gameObject;
                            obj2.SetActive(true);
                            obj2.GetComponent<Text>().text = dialog_data.actual_node.options[1].option_text;
                            obj2.GetComponent<Button>().onClick.AddListener(delegate { PerformFirstNodePhrase(dialog_data.actual_node.options[1].next_node); }); // per passar parametres
                        }
                        break;
                    }
                }
            }
    }
    public void PerformFirstNodePhrase(int node)
    {
        if (!can_pass_dialog)
            return;
        if (node == 0) // conversation finished
        {
            if (dialog_data.dialog_panel.transform.Find("Option1").gameObject.activeInHierarchy)
            {
                active = false;
                dialog_data.interactive_target.GetComponent<Interactivity>().Invoke("SetToWaitingInteraction", 0.2F);
                Destroy(dialog_data.dialog_panel);
            }
        }
        else // another node is called
        {
            can_pass_dialog = false;
            for (int i = 0; i < dialog_data.dialog_node.Count; ++i) // set the actual_node, must be the one that has the same id that the function has
            {
                if (dialog_data.dialog_node[i].node_id == node)
                {
                    dialog_data.actual_node = dialog_data.dialog_node[i];
                    if (dialog_data.actual_node.safe_node)
                        dialog_data.interactive_target.GetComponent<Interactivity>().actual_node = dialog_data.actual_node;
                    break;
                }
            }

            // disable option buttons
            dialog_data.dialog_panel.transform.Find("Option1").gameObject.SetActive(false);
            dialog_data.dialog_panel.transform.Find("Option2").gameObject.SetActive(false);

            // set the first phrase of the node
            StartCoroutine(WritePhrase(dialog_data.actual_node.node_text[0]));
        }
        
    }

    IEnumerator WritePhrase(string text)
    {
        hole_phrase = text;
        phrase_completed = false;
        dialog_data.dialog_panel.transform.Find("Text").GetComponent<Text>().text = null;

        for (int i = 0; dialog_data.dialog_panel.transform.Find("Text").GetComponent<Text>().text != text; ++i)
        {
            dialog_data.dialog_panel.transform.Find("Text").GetComponent<Text>().text += text[i];
            yield return new WaitForSeconds(0.03F);
        }
        phrase_completed = true;
    }
}
