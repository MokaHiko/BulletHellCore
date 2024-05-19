using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public GameObject dialogueCanvas;
    public float letterTime = 0.02f;
    public float readingTimeMultiplier = 0.05f;
    public float minReadingTime = 1.0f;

    private GameObject canvasInstance;
    private TMP_Text nameText;
    private TMP_Text sentenceText;
    private Image profilePicture;
    private Image banner;
    private Button nextButton;

    private Queue<string> sentences;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue, string name = "", bool auto = true, bool stopTime = false)
    {
        if (canvasInstance) { Destroy(canvasInstance); }

        canvasInstance = Instantiate(dialogueCanvas, new Vector3(), Quaternion.identity);

        nameText = canvasInstance.transform.Find("Dialogue/DialogueBox/Name").gameObject.GetComponent<TMP_Text>();
        sentenceText = canvasInstance.transform.Find("Dialogue/DialogueBox/Text").gameObject.GetComponent<TMP_Text>();
        profilePicture = canvasInstance.transform.Find("Dialogue/DialogueBox/CharacterPP").gameObject.GetComponent<Image>();
        banner = canvasInstance.transform.Find("Dialogue/Banner").gameObject.GetComponent<Image>();
        nextButton = canvasInstance.transform.Find("Dialogue/DialogueBox/Next").gameObject.GetComponent<Button>();
        



        sentences.Clear();
        nameText.text = name;
        if (dialogue.characterPP) { profilePicture.sprite = dialogue.characterPP; }
        if (dialogue.characterBanner) { banner.sprite = dialogue.characterBanner; }
        else { Destroy(canvasInstance.transform.Find("Dialogue/Banner").gameObject); }


        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        if (auto)
        {
            StartCoroutine(AutoDisplaySentences(stopTime));
        }
        else
        {
            //TODO: enable button for next message
            nextButton.onClick.AddListener(DisplayNextSentence);
            DisplayNextSentence();
        }
        
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(sentence));
    }

    public void EndDialogue()
    {
        UIEffect uiEffect = canvasInstance.GetComponentInChildren<UIEffect>();
        if (!uiEffect) { Debug.Log("No uiEffect found"); }
        else { uiEffect.Die(canvasInstance); }
    }

    IEnumerator AutoDisplaySentences(bool stopTime = false)
    {
        if (stopTime) { GameManager.Instance.IndefinedStop(); }
        
        UIEffect uiEffect = canvasInstance.GetComponentInChildren<UIEffect>();
        if (!uiEffect) { Debug.Log("No uiEffect found"); }
        else { yield return new WaitForSecondsRealtime(uiEffect.fade_in_duration); }

        while (sentences.Count > 0)
        {
            string sentence = sentences.Dequeue();
            yield return StartCoroutine(TypeSentence(sentence));
            yield return new WaitForSecondsRealtime(Mathf.Max(sentence.Length * readingTimeMultiplier, minReadingTime));
        }
        EndDialogue();
        if (stopTime) { GameManager.Instance.ContinueTime(); }
    }

    IEnumerator TypeSentence(string sentence)
    {
        sentenceText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            sentenceText.text += letter;
            yield return new WaitForSecondsRealtime(letterTime);
        }
    }
}
