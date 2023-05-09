using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TokenCreator : MonoBehaviour
{
    private ActionRepository repo;
    private TimeDisplay display;
    private ActionReader reader;
    
    public TMP_Dropdown choice;
    public TMP_InputField time;

    void Awake()
    {
        repo = (ActionRepository)FindObjectOfType(typeof(ActionRepository), true);
        display = (TimeDisplay)FindObjectOfType(typeof(TimeDisplay), true);
        reader = (ActionReader)FindObjectOfType(typeof(ActionReader), true);
    }

    public void CreateHolderAndToken() {
        ActionHolder newHolder = new ActionHolder();
        switch(choice.value) {
            case 0:
                newHolder.command = CommandType.CreateObject;
                break;
            case 1:
                newHolder.command = CommandType.CreateMatrix;
                break;
            case 2:
                newHolder.command = CommandType.DeleteObject;
                break;
            case 3:
                newHolder.command = CommandType.AssignProperty;
                break;
            case 4:
                newHolder.command = CommandType.PlaySound;
                break;
            case 5:
                newHolder.command = CommandType.DrawGrid;
                break;
            case 6:
                newHolder.command = CommandType.DrawPoint;
                break;
            case 7:
                newHolder.command = CommandType.DrawVector;
                break;
            case 8:
                newHolder.command = CommandType.ApplyMatrix;
                break;
            default:
                Debug.Log("Error: unrecognized create command.");
                return;
        }
        try {
            float time_f = float.Parse(time.text);
            if(time_f < 0) {
                newHolder.time = 0;
                Debug.Log("Negative time is not supported; creating event " + newHolder.command.ToString() + " at default time 0 seconds.");
            } else {
                newHolder.time = time_f;
                Debug.Log("Creating event " + newHolder.command.ToString() + " at time " + time_f.ToString() + " seconds.");
            }
        } catch {
            Debug.Log("Invalid time; must be a float. Defaulting to 0");
            time.text = "0";
        }
        
        repo.AddHolder(newHolder);
        display.AddToken(newHolder);
    }

    public void Open() {
        gameObject.SetActive(true);
        reader.Stop();
    }

    public void Close() {
        gameObject.SetActive(false);
    }

}
