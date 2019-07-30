/* Created by: Alex Wang
 * Date: 07/22/2019
 * MyJointTracker is responsible for locating and recording the positions of the joints in the game.
 * It records for a fixed amount of time and automatically switches to the replay scene.  
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MyJointTracker : MonoBehaviour {
    public static Dictionary<Astra.JointType, List<Vector3>> jointStats = new Dictionary<Astra.JointType, List<Vector3>>() {
        {Astra.JointType.BaseSpine, new List<Vector3>() },
        {Astra.JointType.Head, new List<Vector3>() },
        {Astra.JointType.LeftElbow, new List<Vector3>() },
        {Astra.JointType.LeftFoot, new List<Vector3>() },
        {Astra.JointType.LeftHand, new List<Vector3>() },
        {Astra.JointType.LeftHip, new List<Vector3>() },
        {Astra.JointType.LeftKnee, new List<Vector3>() },
        {Astra.JointType.LeftShoulder, new List<Vector3>() },
        {Astra.JointType.LeftWrist, new List<Vector3>() },
        {Astra.JointType.MidSpine, new List<Vector3>() },
        {Astra.JointType.Neck, new List<Vector3>() },
        {Astra.JointType.RightElbow, new List<Vector3>() },
        {Astra.JointType.RightFoot, new List<Vector3>() },
        {Astra.JointType.RightHand, new List<Vector3>() },
        {Astra.JointType.RightHip, new List<Vector3>() },
        {Astra.JointType.RightKnee, new List<Vector3>() },
        {Astra.JointType.RightShoulder, new List<Vector3>() },
        {Astra.JointType.RightWrist, new List<Vector3>() },
        {Astra.JointType.ShoulderSpine, new List<Vector3>() }
    };

    //To be used to interate through the joints
    public static readonly Astra.JointType[] Joints = new Astra.JointType[]
    {
        Astra.JointType.BaseSpine,
        Astra.JointType.Head,
        Astra.JointType.LeftElbow,
        Astra.JointType.LeftFoot,
        Astra.JointType.LeftHand,
        Astra.JointType.LeftHip,
        Astra.JointType.LeftKnee,
        Astra.JointType.LeftShoulder,
        Astra.JointType.LeftWrist,
        Astra.JointType.MidSpine,
        Astra.JointType.Neck,
        Astra.JointType.RightElbow,
        Astra.JointType.RightFoot,
        Astra.JointType.RightHand,
        Astra.JointType.RightHip,
        Astra.JointType.RightKnee,
        Astra.JointType.RightShoulder,
        Astra.JointType.RightWrist,
        Astra.JointType.ShoulderSpine
    };

    public Transform JointRoot;
    public Text MessageOnPause;
    private float beginTime;
    private float pauseTime;
    //Set to be public so that it is visible to MyCountDownTimer class
    public const float RECORDLENGTH = 15f;

    // Use this for initialization
    void Start () {
        beginTime = Time.time;
    }
	

	// Update is called once per frame
	void Update () {
        if (Time.time - beginTime >= RECORDLENGTH + pauseTime)
        {
            SceneManager.LoadScene("Replay");
        }

        if (MySkeletonRenderer.bodyExists) {
            if (MessageOnPause.IsActive())
            {
                MessageOnPause.gameObject.SetActive(false);
                //Debug.Log("MessageOnPause is now disabled");
            }
            Record();
        }
        else
        {
            if (!MessageOnPause.IsActive())
            {
                MessageOnPause.gameObject.SetActive(true);
                //Debug.Log("MessageOnPause is now enabled");
            }
            pauseTime += 1 * Time.deltaTime;
            //Debug.Log("Pause Time: " + pauseTime);
        }
    }

    private void Record()
    {
        foreach (var joint in Joints)
        {
            GameObject myJoint = JointRoot.transform.Find(joint.ToString()).gameObject;
            jointStats[joint].Add(myJoint.transform.position);
        }
    }

}
