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
        reviewScene = Globals.activeSession.GetNestedActiveScene().closestAncestor<ReviewScene>();
        buttonText.SetText(reviewScene.GetNameOf(reviewNum));
    }

    public void Review()
    {
        // DataManager.Instance.AddNewDataEntry(reviewScene.reviewScenes[reviewNum].publicName);
        // reviewScene.activeReviewScene = reviewScene.reviewScenes[reviewNum];
        // reviewScene.reviewScenes[reviewNum].GoToScene();
        reviewScene.GoToReviewSceneAt(reviewNum);
    }
}