using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// This class is attached to a TextMeshPro InputField object so that upon the content being changed,
// the global lesson variable is changed.
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
