using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class AnalyticsManager : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public void GetAllAnalytics()
  {
    StartCoroutine(GetAllDataAPI(value =>
    {
      Debug.Log(value);
    }));
  }

  public void GetSingleAnalytic(string Name)
  {
    StartCoroutine(GetSingleDataAPI(Name, value =>
    {
      Debug.Log(value);
    }));
  }

  public void PostNewData(string title, string dataJson)
  {
    StartCoroutine(PostNewDataAPI(title, dataJson));
  }

  IEnumerator PostNewDataAPI(string title, string dataJson)
  {
    WWWForm form = new WWWForm();
    form.AddField("title", title);
    form.AddField("content", dataJson);

    using (UnityWebRequest www = UnityWebRequest.Post("https://next-jshl-2.vercel.app/api/addData", form))
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

  IEnumerator GetAllDataAPI(System.Action<string> callback = null)
  {

    using (UnityWebRequest www = UnityWebRequest.Get("https://next-jshl-2.vercel.app/api/getAllData"))
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

  IEnumerator GetSingleDataAPI(string name, System.Action<string> callback = null)
  {
    using (UnityWebRequest www = UnityWebRequest.Get("https://next-jshl-2.vercel.app/api/Data/" + name))
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
