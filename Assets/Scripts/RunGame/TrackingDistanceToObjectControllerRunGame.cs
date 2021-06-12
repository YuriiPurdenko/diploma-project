using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrackingDistanceToObjectControllerRunGame : MonoBehaviour
{
    // Start is called before the first frame update

    private InstantTrackingControllerRunGame _controller;
    private void Start() {
        _controller = GetComponent<InstantTrackingControllerRunGame>();
    }

    IEnumerator DeleteObject(GameObject obj){
        yield return new WaitForSeconds(1);
        obj.SetActive(false);
         Destroy(obj);
        _controller.RemoveObject(obj);
    }
    // Update is called once per frame
    void Update()
    {
        if (_controller.ActiveModels.Count > 0) {
            foreach (var obj in _controller.ActiveModels) {
                Vector3 CameraPos = Camera.main.transform.position;
                Vector3 ObjectPos = obj.transform.position;
                double x = Math.Pow((ObjectPos.x - CameraPos.x),2);
                double y = Math.Pow((ObjectPos.y - CameraPos.y),2);
                double z = Math.Pow((ObjectPos.z - CameraPos.z),2);
                double distance = Math.Sqrt(x + y + z);
                if (distance < 2){ 
                    StartCoroutine(DeleteObject(obj));
                }
            }
        }
    }
}
