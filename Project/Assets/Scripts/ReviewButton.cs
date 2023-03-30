using System;
using TMPro;
using UnityEngine;
public class ReviewButton : MonoBehaviour
{
    public TextMeshPro buttonText;
    public int reviewNum;
    public ReviewScene reviewScene;

    private void Start()
    {
        reviewScene = Globals.activeSession.activeScene as ReviewScene;
        buttonText.SetText(reviewScene.reviewScenes[reviewNum].publicName);
    }

    public void Review()
    {
        DataManager.Instance.AddNewDataEntry(reviewScene.reviewScenes[reviewNum].publicName);
        reviewScene.reviewScenes[reviewNum].GoToScene();
    }
}