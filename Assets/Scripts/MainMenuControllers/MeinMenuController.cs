using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MeinMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickHideButton() {
        SceneManager.LoadScene("InstantTrackingInitScene");
    }
    public void OnClickRunButton() {
        SceneManager.LoadScene("InstantTrackingInitSceneRunGame");
    }
}
