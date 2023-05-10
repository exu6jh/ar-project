using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    public Slider slider;
    public TMP_Text startText;
    public TMP_Text endText;
    public TMP_Text timeText;

    public ActionRepository repository;
    public ActionReader reader;

    public GameObject tokenPrefab;
    public GameObject polytokenPrefab;

    private float start;
    private float end;
    private float time;
    private const float MIN_RANGE = 0.05f;
    private const float MAX_RANGE = 7200f;
    private const float SCROLL_THRESHOLD = 0.01f;
    private const float SCROLL_MULTIPLIER = 0.1f;
    private const float OBSTRUCTION_THRESHOLD = 0.03f;

    private List<Token> tokens;
    private List<TokenPoly> polytokens;

    // Start is called before the first frame update
    void Awake()
    {
        start = 0f;
        time = 0f;
        end = 60f;
        slider.value = 0f;
        slider.onValueChanged.AddListener(delegate {ValueChange();});
        tokens = new List<Token>();
        polytokens = new List<TokenPoly>();
    }

    void Start()
    {
        CreateTokens(repository);
    }

    // Update is called once per frame
    void Update()
    {
        if(reader.executing) {
            time = Time.time - reader.startTime;
        }

        float scroll = Input.mouseScrollDelta.y;
        if(Mathf.Abs(scroll) > SCROLL_THRESHOLD) {
            float newWidth = (end - start) * (1 - SCROLL_MULTIPLIER * Input.mouseScrollDelta.y);
            if(newWidth < MIN_RANGE) {
                newWidth = MIN_RANGE;
            }
            if (newWidth > MAX_RANGE) {
                newWidth = MAX_RANGE;
            }

            Vector3 mousePos = Input.mousePosition;
            float targetTime = (mousePos.x / Screen.width - 0.1f) / 0.8f * (end - start) + start;
            start = targetTime - newWidth * (targetTime - start) / (end - start);
            end = start + newWidth;
            if(start < 0) {
                end = end - start;
                start = 0;
            }
            if(end > MAX_RANGE) {
                start = start - (end - MAX_RANGE);
                end = MAX_RANGE;
            }
            DeleteTokens();
            CreateTokens(repository);
        }

        startText.text = string.Format("{0:0.00}", start);
        endText.text = string.Format("{0:0.00}", end);
        timeText.text = string.Format("{0:0.00}", time);
        slider.value = (time - start) / (end - start);
    }
    
    public float GetStart() {
        return start;
    }

    public float GetEnd() {
        return end;
    }

    public int GetIndex(ActionHolder holder) {
        return repository.GetIndex(holder);
    }

    private void ValueChange() {
        time = slider.value * (end - start) + start;
        timeText.text = string.Format("{0:0.00}", time);
        if(!reader.executing) {
            reader.pauseTime = time;
        }
    }

    public void CreateToken(ActionHolder token) {
        GameObject tokenInstance = Instantiate(tokenPrefab);
        tokenInstance.transform.SetParent(transform);
        tokenInstance.GetComponent<Token>().SetParameters(token, false, 0);
        tokens.Add(tokenInstance.GetComponent<Token>());
    }

    public void CreatePolytoken(List<ActionHolder> tokenList) {
        GameObject polytokenInstance = Instantiate(polytokenPrefab);
        polytokenInstance.transform.SetParent(transform);
        polytokenInstance.GetComponent<TokenPoly>().SetParameters(tokenList);
        polytokens.Add(polytokenInstance.GetComponent<TokenPoly>());
    }

    public void CreateTokens(ActionRepository repo) {
        List<ActionHolder> actions = repo.GetActions();
        if(actions.Count == 0) {
            Debug.Log("Lesson is empty!");
            return;
        }
        float curActiveTime = actions[0].time;
        List<ActionHolder> subactions = new List<ActionHolder>();
        for(int i = 0; i < actions.Count; i++) {
            if(actions[i].time >= start && actions[i].time <= end) {
                if((actions[i].time - curActiveTime) / (end - start) < OBSTRUCTION_THRESHOLD) {
                    subactions.Add(actions[i]);
                } else {
                    if(subactions.Count > 1) {
                        CreatePolytoken(subactions);
                    } else if(subactions.Count == 1) {
                        CreateToken(subactions[0]);
                    }
                    curActiveTime = actions[i].time;
                    subactions = new List<ActionHolder>();
                    subactions.Add(actions[i]);
                }
            }
        }
        if(subactions.Count > 1) {
            // Generate new multitoken
            Debug.Log(string.Format("Creating final polytoken at {0}", subactions[0].time));
            CreatePolytoken(subactions);
        } else if(subactions.Count == 1) {
            // Generate new regular token
            Debug.Log(string.Format("Creating final token at {0}", subactions[0].time));
            CreateToken(subactions[0]);
        }
    }

    public void DeleteToken(ActionHolder holder) {
        for(int i = tokens.Count - 1; i >= 0; i--) {
            if(tokens[i].GetAction() == holder) {
                tokens.RemoveAt(i);
            }
        }

        for(int i = polytokens.Count - 1; i >= 0; i--) {
            List<ActionHolder> actions = polytokens[i].GetActions();
            for(int j = actions.Count - 1; j >= 0; j--) {
                if(actions[j] == holder) {
                    actions.RemoveAt(j);
                }
            }
            if(actions.Count == 0) {
                // This shouldn't happen, but just in case.
                polytokens.RemoveAt(i);
            }
        }
        
        DeleteTokens();
        CreateTokens(repository);
    }

    public void DeleteTokens() {
        for(int i = 0; i < tokens.Count; i++) {
            Destroy(tokens[i].gameObject);
        }
        tokens.Clear();

        for(int i = 0; i < polytokens.Count; i++) {
            Destroy(polytokens[i].gameObject);
        }
        polytokens.Clear();
    }

    public void ClosePolytokens() {
        foreach(TokenPoly polytoken in polytokens) {
            polytoken.Close();
        }
    }
}
