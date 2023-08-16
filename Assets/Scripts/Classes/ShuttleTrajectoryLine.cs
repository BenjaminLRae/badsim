using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SceneManager;

public class ShuttleTrajectoryLine : MonoBehaviour
{
    public Text mouseOverInfoText;
    public ShuttleType shuttleType;

    public SceneManager sceneManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        sceneManager.SetShuttleSpeedTooltip(true, shuttleType);
        //mouseOverInfoText.text = "Line";
        //Debug.Log("Over line");
    }

    private void OnMouseExit()
    {
        sceneManager.SetShuttleSpeedTooltip(false, shuttleType);
    }
}
