using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AWS1HourTest : MonoBehaviour
{

    public Transform moveable;

    public UnityEngine.UI.Text fps;
    public UnityEngine.UI.Text uptime;
    public UnityEngine.UI.Text trials;


    void Update()
    {
        moveable.position = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
        fps.text = string.Format("{0:0}FPS", 1.0f / Time.smoothDeltaTime);

        TimeSpan t = TimeSpan.FromSeconds(Time.time);

        uptime.text = string.Format("{0:D2}h, {1:D2}m, {2:D2}s",
                      t.Hours,
                      t.Minutes,
                      t.Seconds);
        }

    public void CreateTrials()
    {
        UXF.Session.instance.CreateBlock(500);
        UXF.Session.instance.BeginNextTrial();
    }

    public void BeginTrialTimer()
    {
        trials.text = string.Format("Trial {0}", UXF.Session.instance.currentTrialNum.ToString());
        UXF.Session.instance.Invoke("EndCurrentTrial", 30f);
    }

}
