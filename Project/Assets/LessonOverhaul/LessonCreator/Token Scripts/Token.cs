using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    private ActionHolder action;
    private TokenEditor editor;
    private bool subtoken;

    void Start()
    {
        editor = (TokenEditor)FindObjectOfType(typeof(TokenEditor), true);
    }
    
    public void SetParameters(ActionHolder action, bool subtoken, int index) {
        this.action = action;
        this.subtoken = subtoken;
        if(subtoken) {
            GetComponent<RectTransform>().localPosition = new Vector3(
                30f * (index - 1), 0f, 0f
            );
        } else {
            TimeDisplay display = (TimeDisplay)FindObjectOfType(typeof(TimeDisplay), true);
            GetComponent<RectTransform>().position = new Vector3(
                (0.1f + 0.8f * (action.time - display.GetStart())/(display.GetEnd() - display.GetStart())) * Screen.width,
                0.2f * Screen.height,
                0f
            );
        }
    }

    public ActionHolder GetAction() {
        return action;
    }

    public void OpenEditor() {
        if(subtoken) {
            transform.parent.gameObject.SetActive(false);
        }
        editor.Open();
        editor.SwapTo(action);
    }
}
