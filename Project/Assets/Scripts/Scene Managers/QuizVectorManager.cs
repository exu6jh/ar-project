using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class QuizVectorManager : MonoBehaviour
{
     public VectorManager vectorManager;
     private VectorQnState state;

     private static Vector3 standard = new Vector3(1, 1, 0);

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as VectorQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          vectorManager.SetNewStandardValue(state?.value ?? standard);
     }

     public void updateQnState()
     {
          if (state != null)
          {
               state.value = vectorManager.standardValue;
          }
     }
}