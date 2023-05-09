using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenDeleter : MonoBehaviour
{
    private TokenEditor editor;
    private ActionRepository repo;
    private TimeDisplay display;

    void Start()
    {
        editor = (TokenEditor)FindObjectOfType(typeof(TokenEditor), true);
        repo = (ActionRepository)FindObjectOfType(typeof(ActionRepository), true);
        display = (TimeDisplay)FindObjectOfType(typeof(TimeDisplay), true);
    }

    public void DeleteHolderAndToken() {
        editor.Clear();
        ActionHolder holder = editor.GetActive();
        if(holder != null) {
            display.DeleteToken(holder);
            repo.DeleteHolder(holder);
        }
    }
}
