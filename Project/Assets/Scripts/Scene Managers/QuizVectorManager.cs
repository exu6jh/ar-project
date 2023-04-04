using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class QuizVectorManager : MonoBehaviour
{
     public VectorManager vectorManager;
     public PointSnapConstraint PointSnapConstraint;
     private VectorQnState state;

     // private static Vector3 standard = new Vector3(1, 1);
     internal static Vector3 standard() => new Vector3(1, 1);

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as VectorQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          else
          {
               Debug.Log($"{state} has value: {state.value}");    
          }
          // vectorManager.SetNewStandardValue(state?.value ?? standard());
          PointSnapConstraint.SetFollowStandardValue(state?.value ?? standard());
     }

     public void updateQnState()
     {
          if (state != null)
          {
               state.value = vectorManager.standardValue;
          }
     }
}