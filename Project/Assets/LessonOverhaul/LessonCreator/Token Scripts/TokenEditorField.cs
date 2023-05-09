using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenEditorField : MonoBehaviour
{
    [Header("Time")]
    public bool isTime;
    public bool isDuration;
    [Header("Names")]
    public bool isInternalName;
    public bool isEditorName;
    [Header("Affiliated objects")]
    public bool isAffiliatedObject;
    public int affiliationIndex;
    [Header("Properties")]
    public bool isPos;
    public bool isRot;
    public bool isScale;
    [Header("Matrix values")]
    public bool isMatrixFields;
    public int matrixRow;
    public int matrixCol;

    private TokenEditorMenu menu;
    private TMP_InputField textField;
    private Toggle toggle;

    void TextChange() {
        Debug.Log("Text change. " + textField.text);

        if(isTime) {
            try {
                menu.holder.time = float.Parse(textField.text);
            } catch {
                Debug.Log("Error parsing time: " + textField.text);
                textField.text = menu.holder.time.ToString();
            }
        }

        if(isDuration) {
            try {
                menu.holder.duration = float.Parse(textField.text);
            } catch {
                Debug.Log("Error parsing duration: " + textField.text);
                textField.text = menu.holder.duration.ToString();
            }
        }

        if(isInternalName) {
            menu.holder.internalObjectName = textField.text;
        }

        if(isEditorName) {
            menu.holder.editorObjectName = textField.text;
        }

        if(isAffiliatedObject) {
            menu.holder.affiliatedObjects[affiliationIndex] = textField.text;
        }

        if(isMatrixFields) {
            try {
                menu.holder.matrixFields[matrixRow, matrixCol] = float.Parse(textField.text);
            } catch {
                Debug.Log("Error parsing matrix value at row " + matrixRow.ToString() + " and col " + matrixCol.ToString() + ": " + textField.text);
                textField.text = menu.holder.matrixFields[matrixRow, matrixCol].ToString();
            }
        }

        menu.UpdateFields();
    }

    void ToggleChange() {
        if(isPos && toggle.isOn) {
            menu.holder.property = "POS";
        }

        if(isRot && toggle.isOn) {
            menu.holder.property = "ROT";
        }

        if(isScale && toggle.isOn) {
            menu.holder.property = "SCALE";
        }

        menu.UpdateFields();
    }

    public void SetMenu(TokenEditorMenu menu) {
        this.menu = menu;
    }

    public void UpdateValues() {
        if(isTime || isInternalName || isAffiliatedObject || isEditorName || isMatrixFields || isDuration) {
            textField = GetComponent<TMP_InputField>();
            textField.onEndEdit.AddListener(delegate {TextChange();});
        } else {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate {ToggleChange();});
        }
        
        if(isTime) {
            textField.text = menu.holder.time.ToString();
        }

        if(isDuration) {
            textField.text = menu.holder.duration.ToString();
        }

        if(isInternalName) {
            textField.text = menu.holder.internalObjectName;
        }

        if(isEditorName) {
            textField.text = menu.holder.editorObjectName;
        }

        if(isAffiliatedObject) {
            textField.text = menu.holder.affiliatedObjects[affiliationIndex];
        }

        if(isPos) {
            toggle.isOn = menu.holder.property.Equals("POS");
        }

        if(isRot) {
            toggle.isOn = menu.holder.property.Equals("ROT");
        }

        if(isScale) {
            toggle.isOn = menu.holder.property.Equals("SCALE");
        }

        if(isMatrixFields) {
            textField.text = menu.holder.matrixFields[matrixRow, matrixCol].ToString();
        }
    }
}
