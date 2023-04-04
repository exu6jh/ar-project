using System;
using TMPro;
using UnityEngine;
public class QuizBack : MonoBehaviour
{
    public int answerNum;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Answer(int answer)
    {
        _meshRenderer.enabled = answerNum == answer;
    }
}