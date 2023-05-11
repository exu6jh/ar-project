using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TokenEditorMenu : MonoBehaviour
{
    public ActionHolder holder;
    private TokenEditorField[] fields;

    public void SetHolder(ActionHolder holder) {
        Debug.Log("Setting parameters.");
        this.holder = holder;
        this.fields = GetComponentsInChildren<TokenEditorField>(true);
        foreach(TokenEditorField field in fields) {
            field.SetMenu(this);
            // field.SetupListeners();
        }
        // UpdateFields();
    }

    private void OnEnable()
    {
        StartCoroutine(CallUpdateFields());
    }
    
    public IEnumerator CallUpdateFields()
    {
        yield return null;
        UpdateFields();
    }

    public void UpdateFields() {
        foreach (TokenEditorField field in fields)
        {
            field.UpdateValues();
        }
    }
}
