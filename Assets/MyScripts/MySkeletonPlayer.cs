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
    public LineRenderer leftArmRenderer;
    public LineRenderer rightArmRenderer;
    public LineRenderer leftLegRenderer;
    public LineRenderer rightLegRenderer;
    public LineRenderer torsoRenderer;

    // Lines representing the body
    private Vector3[] leftArmPos = new Vector3[5];
    private Vector3[] rightArmPos = new Vector3[5];
    private Vector3[] leftLegsPos = new Vector3[4];
    private Vector3[] rightLegsPos = new Vector3[4];
    private Vector3[] torsoPos = new Vector3[5];

    private readonly Vector3 JOINTSCALE = new Vector3(0.01f, 0.01f, 0.01f);
    public GameObject JointPrefab;
    public GameObject[] joints;
    public Transform PlayerRoot;
    private int index;
    bool isOver;

    private const int MAXROUNDS = 1;
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
    bool DisplaySkeleton(Dictionary<Astra.JointType, List<Vector3>> jointStats, int frameIndex)
    {
        for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
        {
            Astra.JointType jointType = MyJointTracker.Joints[i];
            //End of the record session
            if (frameIndex >= jointStats[jointType].Count)
            {
                return true;
            }

            if (MyJointTracker.jointStats[jointType][frameIndex] != Vector3.zero) {
                if (!joints[i].activeSelf) {
                    joints[i].SetActive(true);
                }
                joints[i].transform.localPosition = (Vector3)MyJointTracker.jointStats[jointType][frameIndex];
                joints[i].transform.localScale = JOINTSCALE;
            }
            else
            {
                if (joints[i].activeSelf) {
                    joints[i].SetActive(false);
                }
            }

            // LEFT ARM
            if (i == (int)Astra.JointType.LeftHand)
            {
                leftArmPos[0] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.LeftWrist)
            {
                leftArmPos[1] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.LeftElbow)
            {
                leftArmPos[2] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.LeftShoulder)
            {
                leftArmPos[3] = joints[i].transform.position;
            }
            // RIGHT ARM
            else if (i == (int)Astra.JointType.RightHand)
            {
                rightArmPos[0] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.RightWrist)
            {
                rightArmPos[1] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.RightElbow)
            {
                rightArmPos[2] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.RightShoulder)
            {
                rightArmPos[3] = joints[i].transform.position;
            }
            // LEFT LEG
            else if (i == (int)Astra.JointType.LeftFoot)
            {
                leftLegsPos[0] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.LeftKnee)
            {
                leftLegsPos[1] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.LeftHip)
            {
                leftLegsPos[2] = joints[i].transform.position;
            }
            // RIGHT LEG
            else if (i == (int)Astra.JointType.RightFoot)
            {
                rightLegsPos[0] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.RightKnee)
            {
                rightLegsPos[1] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.RightHip)
            {
                rightLegsPos[2] = joints[i].transform.position;
            }
            // TORSO
            else if (i == (int)Astra.JointType.Head)
            {
                torsoPos[0] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.Neck)
            {
                torsoPos[1] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.MidSpine)
            {
                torsoPos[3] = joints[i].transform.position;
            }
            // COMMON JOINTS
            else if (i == (int)Astra.JointType.ShoulderSpine)
            {
                leftArmPos[4] = joints[i].transform.position;
                rightArmPos[4] = joints[i].transform.position;
                torsoPos[2] = joints[i].transform.position;
            }
            else if (i == (int)Astra.JointType.BaseSpine)
            {
                leftLegsPos[3] = joints[i].transform.position;
                rightLegsPos[3] = joints[i].transform.position;
                torsoPos[4] = joints[i].transform.position;
            }

            if (leftArmRenderer != null)
            {
                leftArmRenderer.SetPositions(leftArmPos);
            }
            if (rightArmRenderer != null)
            {
                rightArmRenderer.SetPositions(rightArmPos);
            }
            if (leftLegRenderer != null)
            {
                leftLegRenderer.SetPositions(leftLegsPos);
            }
            if (rightLegRenderer != null)
            {
                rightLegRenderer.SetPositions(rightLegsPos);
            }
            if (torsoRenderer != null)
            {
                torsoRenderer.SetPositions(torsoPos);
            }
        }

        return false;
    }

    //Clear all data
    void Reset(Dictionary<Astra.JointType, List<Vector3>> jointStats)
    {
        for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
        {
            Astra.JointType jointType = MyJointTracker.Joints[i];
            jointStats[jointType].Clear();
        }
    }
}