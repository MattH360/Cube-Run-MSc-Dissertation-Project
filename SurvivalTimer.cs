using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalTimer : MonoBehaviour
{
    /* This script maintains the current played time by the player and displays it visually on the screen.
     */
    Text timerText;
    float currentTime;
    float timeHours;
    float timeMinutes;
    float timeSeconds;
    string textHours;
    string textMinutes;
    string textSeconds;
    float timeSpeed = 1.0f;
    
    void Start()
    {
        timerText = GetComponent<Text>();
    }

    void Update()
    {   //Update the time on screen 
        /*Determine current time by adding the current time to the time elapsed since the previous frame,
         * Mathf.Floor returns the largest integer smaller than or equal to the parameter e.g. 10.7f = 10.0f.
         * Modulo is used in each case to prevent each counter reaching its upper limit e.g. counter moves to 59 seconds and then 1 minute (not 60 seconds)
         * determine hours, minutes and seconds and convert to strings of format 00 with a leading zero from 0-9
         */
        currentTime += Time.deltaTime * timeSpeed;
        timeHours = Mathf.Floor((currentTime % 216000) / 3600);
        timeMinutes = Mathf.Floor((currentTime % 3600) / 60);
        timeSeconds = Mathf.Floor(currentTime % 60);
        textHours = timeHours.ToString("00");
        textMinutes = timeMinutes.ToString("00");
        textSeconds = timeSeconds.ToString("00");
        timerText.text = textHours + ":" + textMinutes + ":" + textSeconds;
        
    }
}
