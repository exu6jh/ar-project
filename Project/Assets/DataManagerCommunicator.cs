using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManagerCommunicator : MonoBehaviour
{
  public void CallAddNewDataEnrty(string Name)
  {
    DataManager.Instance.AddNewDataEntry(Name);
  }

  public void CallStopCurrDataEntry()
  {
    DataManager.Instance.StopCurrDataEntry();
  }

  public void CallAddQuizData(string Name, bool Correct)
  {
    DataManager.Instance.AddQuizData(Name, Correct);
  }

  public void CallSubmitData(string Name)
  {
    DataManager.Instance.SubmitData(Name);
  }

  public void CallSubmitData()
  {
    DataManager.Instance.SubmitData();
  }
}
