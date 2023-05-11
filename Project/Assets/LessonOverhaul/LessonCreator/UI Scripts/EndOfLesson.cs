using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfLesson : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TimeDisplay display = (TimeDisplay)FindObjectOfType(typeof(TimeDisplay), true);
        if(display.repository.GetLength() > 0) {
            float endTime = display.repository.GetActions()[display.repository.GetLength() - 1].time;
            RectTransform rect = GetComponent<RectTransform>();
            rect.position = new Vector3(
                (0.1f + 0.8f * (endTime - display.GetStart())/(display.GetEnd() - display.GetStart())) * Screen.width,
                0.25f * Screen.height,
                1f
            );
            rect.localScale = ((endTime >= display.GetStart() && endTime <= display.GetEnd()) ? new Vector3(1f, 1f, 1f) : new Vector3(0f, 0f, 0f));
        } else {
            RectTransform rect = GetComponent<RectTransform>();
            rect.position = new Vector3(
                (0.1f - 0.8f * (display.GetStart())/(display.GetEnd() - display.GetStart())) * Screen.width,
                0.25f * Screen.height,
                1f
            );
            rect.localScale = (display.GetStart() == 0 ? new Vector3(1f, 1f, 1f) : new Vector3(0f, 0f, 0f));
        }
    }
}
