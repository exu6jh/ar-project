using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    private ActionHolder action;
    private TokenEditor editor;
    private TimeDisplay display;

    void Start()
    {
        editor = (TokenEditor)FindObjectOfType(typeof(TokenEditor), true);
        display = (TimeDisplay)FindObjectOfType(typeof(TimeDisplay), true);
    }

    // Update is called once per frame
    void Update()
    {
        if(display.timeView) {
            if(action.time < display.GetStart() || action.time > display.GetEnd()) {
                GetComponent<RectTransform>().position = new Vector3(0f, 0f, 0f);
                GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
            } else {
                GetComponent<RectTransform>().position = new Vector3(
                    (0.1f + 0.8f * (action.time - display.GetStart())/(display.GetEnd() - display.GetStart())) * Screen.width,
                    0.2f * Screen.height,
                    0f
                );
                GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }
        } else {
            int index = display.GetIndex(action);
            if(index < display.GetStart() || index > display.GetEnd()) {
                GetComponent<RectTransform>().position = new Vector3(0f, 0f, 0f);
                GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
            } else {
                GetComponent<RectTransform>().position = new Vector3(
                    (0.1f + 0.8f * (index - display.GetStart())/(display.GetEnd() - display.GetStart())) * Screen.width,
                    0.2f * Screen.height,
                    0f
                );
                GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }
    
    public void SetParameters(ActionHolder action) {
        this.action = action;
    }

    public ActionHolder GetAction() {
        return action;
    }

    public void OpenEditor() {
        editor.Open();
        editor.SwapTo(action);
    }
}
