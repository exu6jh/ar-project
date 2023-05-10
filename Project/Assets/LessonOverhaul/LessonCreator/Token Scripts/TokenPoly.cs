using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenPoly : MonoBehaviour
{
    public GameObject tokenPrefab;
    public GameObject subtokenPanel;
    public GameObject[] buttons;
    private TimeDisplay display;
    private List<ActionHolder> actions;
    private List<Token> tokens;
    private int index;

    // Start is called before the first frame update
    void Start()
    {
        display = (TimeDisplay)FindObjectOfType(typeof(TimeDisplay), true);
        GetComponent<RectTransform>().position = new Vector3(
            (0.1f + 0.8f * (actions[0].time - display.GetStart())/(display.GetEnd() - display.GetStart())) * Screen.width,
            0.2f * Screen.height,
            0f
        );
        tokens = new List<Token>();
        index = 0;
    }

    public void SetParameters(List<ActionHolder> actions) {
        this.actions = actions;
    }

    public List<ActionHolder> GetActions() {
        return actions;
    }

    public void Generate() {
        display.ClosePolytokens();
        subtokenPanel.SetActive(true);
        for(int i = 0; i < Mathf.Min(actions.Count, 3); i++) {
            GameObject tokenInstance = Instantiate(tokenPrefab);
            tokenInstance.transform.SetParent(subtokenPanel.transform);
            tokenInstance.GetComponent<Token>().SetParameters(actions[index + i], true, i);
            tokens.Add(tokenInstance.GetComponent<Token>());
        }
        UpdateButtons();
    }

    private void Delete() {
        foreach(Token token in tokens) {
            Destroy(token.gameObject);
        }
        tokens.Clear();
    }

    public void Close() {
        Delete();
        subtokenPanel.SetActive(false);
    }

    public void IncreaseIndex() {
        Delete();
        if(index < actions.Count - 3) {
            index++;
        }
        Generate();
    }

    public void DecreaseIndex() {
        Delete();
        if(index > 0) {
            index--;
        }
        Generate();
    }

    private void UpdateButtons() {
        buttons[0].SetActive(index > 0);
        buttons[1].SetActive(index < actions.Count - 3);
    }
}
