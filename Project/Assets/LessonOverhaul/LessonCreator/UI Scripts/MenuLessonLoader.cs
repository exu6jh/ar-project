using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MenuLessonLoader : MonoBehaviour
{
    public GameObject[] mainMenuObjects;
    public GameObject[] createMenuObjects;
    public TMP_InputField lessonName;
    
    void Start() {
        lessonName.onEndEdit.AddListener(delegate {TextChange();});
    }

    void TextChange() {
        Debug.Log("Text change. " + lessonName.text);
        lessonName.text = new string(lessonName.text.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    public void OpenLesson() {
        string path = EditorUtility.OpenFilePanel("Lesson", "", "txt");
        if(path.Length != 0) {
            Globals.lessonCreate = path;
            ChangeScene.GoToScene(SCENES.CREATE);
        }
        else
        {
            Debug.Log("No Lesson Selected!");
        }
    }

    public void CreateLesson() {
        if(lessonName.text == "") {
            Globals.lessonCreate = "Assets/LessonOverhaul/Lessons/default.txt";
            Debug.Log("Lesson set to default.txt!");
        } else {
            Globals.lessonCreate = "Assets/LessonOverhaul/Lessons/" + lessonName.text + ".txt";
            Debug.Log($"Lesson set to {lessonName.text}.txt!");
        }
        ChangeScene.GoToScene(SCENES.CREATE);
    }

    public void MainMenu() {
        foreach(GameObject obj in createMenuObjects) {
            obj.SetActive(false);
        }
        foreach(GameObject obj in mainMenuObjects) {
            obj.SetActive(true);
        }
    }

    public void NewLessonMenu() {
        foreach(GameObject obj in mainMenuObjects) {
            obj.SetActive(false);
        }
        foreach(GameObject obj in createMenuObjects) {
            obj.SetActive(true);
        }
    }
}
