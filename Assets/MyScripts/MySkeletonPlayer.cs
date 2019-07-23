/* Created by: Alex Wang
 * Date: 07/22/2019
 * MySkeletonPlayer is responsible for rendering the skeleton based on the recorded data.
 * It automatically switches to the record scene after three rounds of replay.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MySkeletonPlayer : MonoBehaviour
{
    private readonly Vector3 JOINTSCALE = new Vector3(1f, 1f, 1f);
    public GameObject JointPrefab;
    public GameObject[] joints;
    public Transform PlayerRoot;
    private int index;
    bool isOver;

    private const int MAXROUNDS = 3;
    private int roundCounter;
    public Text roundText;

    void Start()
    {
        roundCounter = 1;
        roundText.text = roundCounter.ToString();
        isOver = false;
        index = 0;
        joints = new GameObject[19];
        for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
        {
            joints[i] = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
            joints[i].name = "Replay_" + MyJointTracker.Joints[i].ToString();
            joints[i].transform.SetParent(PlayerRoot);
        }
    }

    void Update()
    {
        if (roundCounter <= MAXROUNDS)
        {
            if (!isOver)
            {
                isOver = DisplaySkeleton(MyJointTracker.jointStats, index);
                index++;
                /*
                for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
                {
                    Astra.JointType jointType = MyJointTracker.Joints[i];
                    if (index < MyJointTracker.jointStats[jointType].Count)
                    {
                        if (MyJointTracker.jointStats[jointType][index] == null)
                        {
                            joints[i].SetActive(false);
                            //Debug.Log(joints[i].name + " index: " + index + "does not have data");
                        }
                        else
                        {
                            joints[i].SetActive(true);
                            joints[i].transform.localPosition = (Vector3)MyJointTracker.jointStats[jointType][index];
                            joints[i].transform.localScale = JOINTSCALE;
                            //Debug.Log(joints[i].name + " index: " + index + " position: " + joints[i].transform.position);
                        }
                    }
                    //Reached the end of the recorded stats if index >= Count
                    else
                    {
                        isOver = true;
                    }
                }
                index++;
                */
            }
            else
            {
                index = 0;
                isOver = false;
                roundCounter++;
                roundText.text = roundCounter.ToString();
            }
        }
        //Resets the stats container after all the rounds
        else
        {
            roundText.text = "Switching to recording mode";
            Reset(MyJointTracker.jointStats);
            SceneManager.LoadScene("Record");
        }

    }

    //Return true if frameIndex goes beyond jointsStats
    bool DisplaySkeleton(Dictionary<Astra.JointType, List<Vector3?>> jointStats, int frameIndex)
    {
        for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
        {
            Astra.JointType jointType = MyJointTracker.Joints[i];

            //End of the record session
            if (frameIndex >= jointStats[jointType].Count)
            {
                return true;
            }

            joints[i].SetActive(true);
            joints[i].transform.localPosition = (Vector3)MyJointTracker.jointStats[jointType][frameIndex];
            joints[i].transform.localScale = JOINTSCALE;
        }
        return false;
    }

    void Reset(Dictionary<Astra.JointType, List<Vector3?>> _jointGroup)
    {
        for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
        {
            Astra.JointType jointType = MyJointTracker.Joints[i];
            _jointGroup[jointType].Clear();
        }
    }
}