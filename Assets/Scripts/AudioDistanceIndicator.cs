using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class AudioDistanceIndicator : MonoBehaviour
{

    private InstantTrackingController _controller;
    public AudioClip audioClip;
    public AudioSource audio;
    private double oldDistance = 10000000000;
    private float timeBetweenShots = 0.5f;
    private float timer;
    private bool isDistanceSet = false;
    private float delayTime = 0;

    void Start()
    {
        _controller = GetComponent<InstantTrackingController>();
    }

   
    void Update()
    {
        if (_controller.ActiveModels.Count != 0)
        {
            var activeObj = _controller.ActiveModels;
            var obj = activeObj.First();
            Vector3 CameraPos = Camera.main.transform.position;
            Vector3 ObjectPos = obj.transform.position;
            double x = Math.Pow((ObjectPos.x - CameraPos.x), 2);
            double y = Math.Pow((ObjectPos.y - CameraPos.y), 2);
            double z = Math.Pow((ObjectPos.z - CameraPos.z), 2);
            double distance = Math.Sqrt(x + y + z);
            
            //if(!isDistanceSet){
                if (distance > 20){
                    timeBetweenShots = 4f;
                }
                else if(distance > 10){
                    timeBetweenShots = 2f;
                }
                else if(distance > 5){
                    timeBetweenShots = 1f;
                }
                else if(distance > 3){
                    timeBetweenShots = 0.5f;
                }
                else{
                    timeBetweenShots = 0.25f;
                }
             //   isDistanceSet = true;
           // }
            oldDistance = distance;
            timer += Time.deltaTime;
            if (timer > timeBetweenShots)
            {
                audio.PlayOneShot(audioClip,0.5f);
                timer = 0;
              /*   if (distance > oldDistance){
                    timeBetweenShots -= Time.deltaTime;
                }
                else
                {
                    timeBetweenShots += Time.deltaTime;
                }

                if(timeBetweenShots > 0.75f){
                    timeBetweenShots = 0.75f;
                }
                if(timeBetweenShots < 0.15f){
                    timeBetweenShots = 0.15f;
                }
            
                delayTime = timeBetweenShots; */

            }
        }
    }
}
