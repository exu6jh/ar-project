using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenEditorMenu : MonoBehaviour
{
    public ActionHolder holder;
    private TokenEditorField[] fields;

    public void SetHolder(ActionHolder holder) {
        Debug.Log("Setting parameters.");
        this.holder = holder;
        UpdateFields();
    }

    public void UpdateFields() {
        fields = GetComponentsInChildren<TokenEditorField>(true);
        foreach(TokenEditorField field in fields) {
            field.SetMenu(this);
            field.UpdateValues();
        }
    }
}
