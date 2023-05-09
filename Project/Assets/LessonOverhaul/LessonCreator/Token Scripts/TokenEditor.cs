using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenEditor : MonoBehaviour
{
    public GameObject openEditorButton;

    // All editor menus
    private Dictionary<CommandType, TokenEditorMenu> menus;
    private ActionHolder active;
    private ActionReader reader;

    // Start is called before the first frame update
    void Awake()
    {
        menus = new Dictionary<CommandType, TokenEditorMenu>();
        TokenEditorMenu[] childMenus = GetComponentsInChildren<TokenEditorMenu>(true);
        foreach(TokenEditorMenu childMenu in childMenus) {
            CommandType childCommandType = (CommandType)Enum.Parse(typeof(CommandType), childMenu.transform.name);
            menus.Add(childCommandType, childMenu);
        }
        reader = (ActionReader)FindObjectOfType(typeof(ActionReader), true);
    }

    public void SwapTo(ActionHolder holder) {
        if(active != null) {
            menus[active.command].gameObject.SetActive(false);
        }
        active = holder;
        menus[holder.command].SetHolder(holder);
        menus[holder.command].gameObject.SetActive(true);
    }

    public ActionHolder GetActive() {
        return active;
    }

    public void Open() {
        openEditorButton.SetActive(false);
        gameObject.SetActive(true);
        reader.Stop();
    }

    public void Close() {
        openEditorButton.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Clear() {
        if(active != null) {
            menus[active.command].gameObject.SetActive(false);
        }
    }
}
