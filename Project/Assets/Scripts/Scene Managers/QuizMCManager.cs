using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;

public class QuizMCManager : MonoBehaviour
{
     private DiscreteQnState state;

     public UnityEvent<int> broadcastMC;

     private void Start()
     {
          state = (Globals.activeSession.GetNestedActiveScene() as QuizQnScene)?.qnState as DiscreteQnState;
          if (state == null)
          {
               Debug.Log("Oops!");
          }
          updateQnState(state?.value ?? 0);
     }

     public void updateQnState(int answer)
     {
          if (state != null)
          {
               state.value = answer;
          }
          broadcastMC.Invoke(answer);
     }
}