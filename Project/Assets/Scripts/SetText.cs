using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class SetText : MonoBehaviour
{

    public TextMeshProUGUI tpro;
    

    public void SetTextTo(SliderEventData data)
    {
        tpro.SetText((data.NewValue * 10 - 5).ToString("N2"));
    }

    public void SetTextTo(CompositeQnState state)
    {
        List<QuizQnState> states = state.value;
        tpro.SetText($"({(states[0] as SliderQnState).value * 10 - 5 : N2}, {(states[1] as SliderQnState).value * 10 - 5 : N2})");
    }

    public void SetTextTo(TwoSliderQnState state)
    {
        // tpro.SetText($"({state.value1 * 10 - 5 : N2}, {state.value2 * 10 - 5 : N2})");
    }
    
}