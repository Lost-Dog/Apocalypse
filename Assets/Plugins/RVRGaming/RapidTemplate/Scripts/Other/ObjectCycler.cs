using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCreator.Runtime.Common;
using System.Collections.Generic;

public class ObjectCycler : MonoBehaviour
{
    public List<GameObject> objectsToCycle;
    public List<string> objectTexts;

    public TextMeshProUGUI displayText;

    public Button nextButton;
    public Button previousButton;

    private PropertyGetString m_Text = new PropertyGetString();
    public PropertySetString m_Set = new PropertySetString();

    private int currentIndex = 0;
    private Args args;

    void Start()
    {
        this.args = new Args(this.gameObject);

        for (int i = 1; i < objectsToCycle.Count; i++)
        {
            objectsToCycle[i].SetActive(false);
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(NextObject);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousObject);

        UpdateText();
    }

    public void NextObject()
    {
        ChangeActiveObject((currentIndex + 1) % objectsToCycle.Count);
    }

    public void PreviousObject()
    {
        ChangeActiveObject((currentIndex - 1 + objectsToCycle.Count) % objectsToCycle.Count);
    }

    private void ChangeActiveObject(int newIndex)
    {
        if (objectsToCycle.Count > 0)
            objectsToCycle[currentIndex].SetActive(false);

        currentIndex = newIndex;
        objectsToCycle[currentIndex].SetActive(true);
        UpdateText();
    }

    private void UpdateText()
    {
        if (displayText != null && objectTexts.Count > currentIndex)
        {
            string currentText = objectTexts[currentIndex];
            displayText.text = currentText;

            this.m_Set.Set(currentText, args);
        }
    }
}
