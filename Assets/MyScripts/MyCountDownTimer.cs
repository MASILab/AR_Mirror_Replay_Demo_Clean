/* Created by: Alex Wang
 * Date: 07/22/2019
 * MyCountDownTimer is responsible for displaying the countdown timer during the recording session.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCountDownTimer : MonoBehaviour {
    private float startTime = MyJointTracker.RECORDLENGTH;

    private Text countDownTimer;

	// Use this for initialization
	void Start () {
        countDownTimer = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (MySkeletonRenderer.bodyExists) {
            startTime -= 1 * Time.deltaTime;
            countDownTimer.text = startTime.ToString("0");

            //This part will not actually be executed since SceneManager automatically switches to replay scene once recording time is over.
            //Keep for the reusablity.
            if (startTime < 0)
            {
                startTime = 0;
            }
        }
	}
}
