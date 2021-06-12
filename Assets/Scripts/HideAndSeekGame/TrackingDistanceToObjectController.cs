using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrackingDistanceToObjectController : MonoBehaviour
{
    // Start is called before the first frame update

    private InstantTrackingController _controller;
    private void Start() {
        _controller = GetComponent<InstantTrackingController>();
    }

    IEnumerator DeleteObject(GameObject obj){
        yield return new WaitForSeconds(5);
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
                if (distance > 2 ) {obj.SetActive(false);}
                if (distance < 2){ 
                    obj.SetActive(true);
                    StartCoroutine(DeleteObject(obj));
                }
            }
        }
    }
}
