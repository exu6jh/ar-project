using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataManager : MonoBehaviour
{
  LessonData CurrDataEntry = null;
  DataList DataListObj;

  public static DataManager Instance;
  // Start is called before the first frame update
  void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
    }
    else
    {
      Instance = this;
    }
    DataListObj = new DataList();
    DataListObj.Entries = new List<LessonData>();
  }

  public void AddNewDataEntry(string Name)
  {
    if (CurrDataEntry != null)
    {
      StopCurrDataEntry();
    }
    LessonData newData = new LessonData();
    newData.Name = Name;
    newData.Start_Time = System.DateTime.Now.ToString();
    CurrDataEntry = newData;
    Debug.Log("Added new entry: " + Name);
  }
  public void StopCurrDataEntry()
  {
    if (CurrDataEntry != null)
    {
      CurrDataEntry.End_Time = System.DateTime.Now.ToString();
      CurrDataEntry.Total_Time = (float)(System.DateTime.Parse(CurrDataEntry.End_Time) - System.DateTime.Parse(CurrDataEntry.Start_Time)).TotalSeconds;
      DataListObj.Entries.Add(CurrDataEntry);
      CurrDataEntry = null;
      Debug.Log("Removed Current Entry");
    }
  }

  public void AddQuizData(string Name, bool Correct)
  {
    QuizData newEntry = new QuizData();
    newEntry.Name = Name;
    newEntry.Correct = Correct;
    CurrDataEntry.Quiz.Add(newEntry);
  }

  public void SubmitData(string Name)
  {
    Debug.Log("Submitting data");
    string body = JsonUtility.ToJson(DataListObj);
    this.gameObject.GetComponent<AnalyticsManager>().PostNewData(Name, body);
  }

  public void SubmitData()
  {
    Debug.Log("Submitting data");
    string Name = System.Guid.NewGuid().ToString();
    string body = JsonUtility.ToJson(DataListObj);
    this.gameObject.GetComponent<AnalyticsManager>().PostNewData(Name, body);
  }
}
