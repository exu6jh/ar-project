using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeLesson : MonoBehaviour
{
    private TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(LessonChange);
    }

    public void LessonChange(string text) {
        Globals.lesson = text;
    }
}
