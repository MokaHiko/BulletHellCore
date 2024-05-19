using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public GameObject dialogueCanvas;

    private GameObject canvasInstance;
    private TMP_Text nameText;
    private TMP_Text sentenceText;
    private Image image;
    private Button nextButton;

    private Queue<string> sentences;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Destroy(canvasInstance);

        canvasInstance = Instantiate(dialogueCanvas, new Vector3(), Quaternion.identity);

        nameText = canvasInstance.transform.Find("Dialogue/DialogueBox/Name").gameObject.GetComponent<TMP_Text>();
        sentenceText = canvasInstance.transform.Find("Dialogue/DialogueBox/Text").gameObject.GetComponent<TMP_Text>();
        image = canvasInstance.transform.Find("Dialogue/CharacterImage").gameObject.GetComponent<Image>();
        nextButton = canvasInstance.transform.Find("Dialogue/DialogueBox/Next").gameObject.GetComponent<Button>();
        



        sentences.Clear();
        nameText.text = dialogue.name;
        image.sprite = dialogue.characterSprite;
        nextButton.onClick.AddListener(DisplayNextSentence);

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        sentenceText.text = sentence;
    }

    public void EndDialogue()
    {
        UIEffect uiEffect = canvasInstance.GetComponentInChildren<UIEffect>();
        if (!uiEffect) { Debug.Log("No uiEffect found"); }
        else { uiEffect.Die(canvasInstance); }
    }
}
