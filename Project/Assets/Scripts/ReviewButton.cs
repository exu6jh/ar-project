using System;
using TMPro;
using UnityEngine;
public class ReviewButton : MonoBehaviour
{
    public TextMeshPro buttonText;
    public int reviewNum;

    private void Start()
    {
        buttonText.SetText(Globals.activeSession.reviewScenes[reviewNum].publicName);
    }

    public void Review()
    {
        DataManager.Instance.AddNewDataEntry(Globals.activeSession.reviewScenes[reviewNum].publicName);
        Globals.activeSession.reviewScenes[reviewNum].GoToScene();
    }
}