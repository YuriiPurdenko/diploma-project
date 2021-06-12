using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class PanelGameControllerRunGame : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject sampleHeader;
    public GameObject eggsPanel;
    public GameObject endPanel;
    public List<Image> endScoreEggs;
    public Text ScoreTime;

    public List<Image> eggs;
    int index = 0;
    int prevCount = 0;
    int count = 0;
    private int InitCount;
    private float timer = 0;
    private bool endGame = false;
    private InstantTrackingControllerRunGame _controller;

    void OnEnable() {
        eggsPanel.SetActive(true);
    }
    void OnDisable() {
        eggsPanel.SetActive(false);
    }

    void Start()
    {
        _controller = GetComponent<InstantTrackingControllerRunGame>();
        if (eggsPanel.activeSelf){
            count = _controller.ActiveModels.Count;
            for (int i = 0; i < count; i++) {
                eggs[i].gameObject.SetActive(true);
            }
            prevCount = count;
            InitCount = count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!endGame)
        {
            timer += Time.deltaTime;
            EggsPanelControll();
        }
    }


    void setEggsAlpha(int x) {
        for(int i = 0; i < x; i++) { 
            var tempColor = endScoreEggs[i].color;
            tempColor.a = 1f;
            endScoreEggs[i].color= tempColor;
        }
    }

    void EndPanelControll() {
        sampleHeader.SetActive(false);
        float timeForOneEgg = timer / InitCount;
        if (timeForOneEgg <= 30) {
            setEggsAlpha(3);
        }
        else if (timeForOneEgg <= 50) { 
            setEggsAlpha(2);
        }
        else { 
            setEggsAlpha(1);
        }
        ScoreTime.text = "" + (int)timer;
        endPanel.SetActive(true);
    }

    void EggsPanelControll() { 
        count = _controller.ActiveModels.Count;
        if (count == 0) {
            eggsPanel.SetActive(false);
            endGame = true;
            EndPanelControll();
        }
        if (count > 0 && count != prevCount){
            var tempColor = eggs[index].color;
            tempColor.a = 1f;
            eggs[index].color= tempColor;
            index++;
            prevCount = count;
        }
    }

}
