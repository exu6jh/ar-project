using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CollectTimeData : MonoBehaviour
{
  public string SceneName;
  string timeStamp;

  float totalTime;
  // Start is called before the first frame update

  [SerializeField]
  ChangeScene changeScene;

  [SerializeField]
  GetScene getScene;
  void Start()
  {
    timeStamp = System.DateTime.UtcNow.ToLocalTime().ToString("MM-dd-yyyy  HH:mm");
  }

  void Update()
  {
    totalTime += Time.deltaTime;
  }

  public void SubmitData()
  {
    StartCoroutine(SendDataAPI());
  }

  IEnumerator SendDataAPI()
  {
    WWWForm form = new WWWForm();
    form.AddField("title", timeStamp + " -> " + SceneName);
    form.AddField("content", totalTime.ToString());

    using (UnityWebRequest www = UnityWebRequest.Post("https://next-jshl-2.vercel.app/api/addData", form))
    {
      yield return www.SendWebRequest();

      changeScene.GoToScene(getScene);

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

}