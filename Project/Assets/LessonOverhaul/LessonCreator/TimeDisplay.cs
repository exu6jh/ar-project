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
    public List<GameObject> tokens;

    public bool timeView;

    private float start;
    private float end;
    private float time;

    private const float MIN_RANGE = 0.05f;
    private const float MAX_RANGE = 7200f;
    private const float SCROLL_THRESHOLD = 0.01f;
    private const float SCROLL_MULTIPLIER = 0.1f;

    // Start is called before the first frame update
    void Awake()
    {
        start = 0f;
        time = 0f;
        end = 60f;
        slider.value = 0f;
        slider.onValueChanged.AddListener(delegate {ValueChange();});
    }

    void Start()
    {
        CreateTokens(repository);
    }

    // Update is called once per frame
    void FixedUpdate()
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
        }

        if(timeView) {
            startText.text = string.Format("{0:0.00}", start);
            endText.text = string.Format("{0:0.00}", end);
            timeText.text = string.Format("{0:0.00}", time);
            slider.value = (time - start) / (end - start);
        } else {
            startText.text = string.Format("{0:0}", start);
            endText.text = string.Format("{0:0}", end);
            timeText.text = "";
            slider.value = 0;
        }
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

    public void ViewChange() {
        slider.enabled = !slider.enabled;
        if(timeView) {
            slider.value = 0f;
        }
        timeView = !timeView;
    }

    public void AddToken(ActionHolder token) {
        GameObject tokenInstance = Instantiate(tokenPrefab);
        tokenInstance.transform.SetParent(transform);
        tokenInstance.GetComponent<Token>().SetParameters(token);
    }

    public void DeleteToken(ActionHolder holder) {
        foreach(Token token in GetComponentsInChildren<Token>()) {
            if(token.GetAction() == holder) {
                Destroy(token.gameObject);
            }
        }
    }

    public void CreateTokens(ActionRepository repo) {
        List<ActionHolder> actions = repo.GetActions();
        for(int i = 0; i < actions.Count; i++) {
            AddToken(actions[i]);
        }
    }
}
