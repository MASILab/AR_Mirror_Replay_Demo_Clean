using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyJointTracker : MonoBehaviour {
    private static Dictionary<Astra.JointType, List<Vector3?>> _jointGroup = new Dictionary<Astra.JointType, List<Vector3?>>() {
        {Astra.JointType.BaseSpine, new List<Vector3?>() },
        {Astra.JointType.Head, new List<Vector3?>() },
        {Astra.JointType.LeftElbow, new List<Vector3?>() },
        {Astra.JointType.LeftFoot, new List<Vector3?>() },
        {Astra.JointType.LeftHand, new List<Vector3?>() },
        {Astra.JointType.LeftHip, new List<Vector3?>() },
        {Astra.JointType.LeftKnee, new List<Vector3?>() },
        {Astra.JointType.LeftShoulder, new List<Vector3?>() },
        {Astra.JointType.LeftWrist, new List<Vector3?>() },
        {Astra.JointType.MidSpine, new List<Vector3?>() },
        {Astra.JointType.Neck, new List<Vector3?>() },
        {Astra.JointType.RightElbow, new List<Vector3?>() },
        {Astra.JointType.RightFoot, new List<Vector3?>() },
        {Astra.JointType.RightHand, new List<Vector3?>() },
        {Astra.JointType.RightHip, new List<Vector3?>() },
        {Astra.JointType.RightKnee, new List<Vector3?>() },
        {Astra.JointType.RightShoulder, new List<Vector3?>() },
        {Astra.JointType.RightWrist, new List<Vector3?>() },
        {Astra.JointType.ShoulderSpine, new List<Vector3?>() }
    };

    //To be used to interate through the joints
    private readonly Astra.JointType[] Joints = new Astra.JointType[]
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
    public Transform PlayerRoot;
    private bool onRecord;
    private float beginTime;
    public GameObject JointPrefab;
    private GameObject leftHand;
    private GameObject[] joints;
    private int index;

    // Use this for initialization
    void Start () {
        beginTime = Time.time;
        onRecord = true;
        index = 0;

        /*
        joints = new GameObject[19];
        for (int i = 0; i < Joints.Length; ++i)
        {
            joints[i] = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
            joints[i].name = "Replay_" + Joints[i].ToString();
            joints[i].transform.SetParent(PlayerRoot);
        }
        */
        
    }
	

	// Update is called once per frame
	void Update () {
        if (Time.time - beginTime >= 5)
        {
            onRecord = false;
            //GameObject.Find("SkeletonViewer").SetActive(false);

            /*
            for (int i = 0; i < Joints.Length; ++i)
            {
                Astra.JointType jointType = Joints[i];
                if (index < _jointGroup[jointType].Count)
                {
                    if (_jointGroup[jointType][index] == null)
                    {
                        joints[i].SetActive(false);
                        Debug.Log(joints[i].name + " index: " + index + "does not have data");
                    }
                    else
                    {
                        joints[i].SetActive(true);
                        joints[i].transform.position = (Vector3)_jointGroup[jointType][index];
                        Debug.Log(joints[i].name + " index: " + index + " position: " + joints[i].transform.position);
                    }
                }
            }
            index++;
            */




            //Debug.Log("End of Record");

            ///*
            leftHand = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
            if (index < _jointGroup[Astra.JointType.LeftHand].Count)
            {
                if (_jointGroup[Astra.JointType.LeftHand][index] == null)
                {
                    leftHand.SetActive(false);
                    Debug.Log(index + " is null");
                    index++;
                }
                else
                {
                    leftHand.SetActive(true);
                    leftHand.transform.position = (Vector3)_jointGroup[Astra.JointType.LeftHand][index];
                    Debug.Log(index + " position: " + _jointGroup[Astra.JointType.LeftHand][index]);
                    index++;
                }
            }
            else
            {
                Destroy(leftHand);
                Debug.Log("leftHand is destoryed");
            }
            //*/
            

        }
        if (onRecord)
        {
            Record();
        }
	}

    private void Record()
    {
        foreach (var joint in Joints)
        {
            GameObject myJoint = JointRoot.transform.Find(joint.ToString()).gameObject;
            if (myJoint == null)
            {
                _jointGroup[joint] = null;
                Debug.Log(joint.ToString() + " not detected");
            }
            else
            {
                _jointGroup[joint].Add(myJoint.transform.position);
                Debug.Log(joint.ToString() + " Stats: " + _jointGroup[joint][_jointGroup[joint].Count - 1]);
            }
        }
    }
}
