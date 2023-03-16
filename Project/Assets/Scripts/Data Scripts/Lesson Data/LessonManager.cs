using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
public class LessonManager : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public void GetAllLessons()
  {
    StartCoroutine(GetAllLessonsAPI(value =>
    {
      Debug.Log(value);
    }));
  }

  public void GetSingleLesson(string Name)
  {
    StartCoroutine(GetSingleLessonAPI(Name, value =>
    {
      Debug.Log(value);
    }));
  }

  public void PostNewLesson(string title, string dataJson)
  {
    StartCoroutine(PostNewLessonAPI(title, dataJson));
  }

  IEnumerator PostNewLessonAPI(string title, string dataJson)
  {
    WWWForm form = new WWWForm();
    form.AddField("title", title);
    form.AddField("content", dataJson);

    using (UnityWebRequest www = UnityWebRequest.Post("https://next-jshl-2.vercel.app/api/addLesson", form))
    {
      yield return www.SendWebRequest();

      if (www.result != UnityWebRequest.Result.Success)
      {
        Debug.Log(www.error);
      }
      else
      {
        Debug.Log("Form upload complete!");
      }
    }
  }

  IEnumerator GetAllLessonsAPI(System.Action<string> callback = null)
  {

    using (UnityWebRequest www = UnityWebRequest.Get("https://next-jshl-2.vercel.app/api/getAllLessons"))
    {
      yield return www.SendWebRequest();

      if (www.result != UnityWebRequest.Result.Success)
      {
        Debug.Log(www.error);
      }
      else
      {
        Debug.Log("Request Sucessful");
        callback(www.downloadHandler.text);
      }
    }
  }

  IEnumerator GetSingleLessonAPI(string name, System.Action<string> callback = null)
  {
    using (UnityWebRequest www = UnityWebRequest.Get("https://next-jshl-2.vercel.app/api/Lesson/" + name))
    {
      yield return www.SendWebRequest();

      if (www.result != UnityWebRequest.Result.Success)
      {
        Debug.Log(www.error);
      }
      else
      {
        Debug.Log("Form upload complete!");
        callback(www.downloadHandler.text);
      }
    }
  }
}
