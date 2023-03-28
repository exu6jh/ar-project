using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class SetText : MonoBehaviour
{

    public TextMeshProUGUI tpro;
    

    public void SetTextTo(SliderEventData data)
    {
        tpro.SetText((data.NewValue * 10).ToString("N2"));
    }
    
}