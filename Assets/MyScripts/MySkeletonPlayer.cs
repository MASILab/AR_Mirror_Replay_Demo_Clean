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

    public LineRenderer leftArm;
    public LineRenderer rightArm;
    public LineRenderer leftLeg;
    public LineRenderer rightLeg;
    public LineRenderer torso;



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

    //Clear all data
    void Reset(Dictionary<Astra.JointType, List<Vector3?>> _jointGroup)
    {
        for (int i = 0; i < MyJointTracker.Joints.Length; ++i)
        {
            Astra.JointType jointType = MyJointTracker.Joints[i];
            _jointGroup[jointType].Clear();
        }
    }

    #region Bones structure

    /// <summary>
    /// Bone is connector of two joints
    /// </summary>
    private struct Bone
    {
        public Astra.JointType ParentJointType;
        public Astra.JointType EndJointType;

        public Bone(Astra.JointType parentJointType, Astra.JointType endJointType)
        {
            ParentJointType = parentJointType;
            EndJointType = endJointType;
        }
    };

    /// <summary>
    /// Skeleton structure = list of bones = list of joint connectors
    /// </summary>
    private static readonly Bone[] Bones = new Bone[]
    {
            // spine, neck, and head
            new Bone(Astra.JointType.BaseSpine, Astra.JointType.MidSpine),
            new Bone(Astra.JointType.MidSpine, Astra.JointType.ShoulderSpine),
            new Bone(Astra.JointType.ShoulderSpine, Astra.JointType.Neck),
            new Bone(Astra.JointType.Neck, Astra.JointType.Head),
            // left arm
            new Bone(Astra.JointType.ShoulderSpine, Astra.JointType.LeftShoulder),
            new Bone(Astra.JointType.LeftShoulder, Astra.JointType.LeftElbow),
            new Bone(Astra.JointType.LeftElbow, Astra.JointType.LeftWrist),
            new Bone(Astra.JointType.LeftWrist, Astra.JointType.LeftHand),
            // right arm
            new Bone(Astra.JointType.ShoulderSpine, Astra.JointType.RightShoulder),
            new Bone(Astra.JointType.RightShoulder, Astra.JointType.RightElbow),
            new Bone(Astra.JointType.RightElbow, Astra.JointType.RightWrist),
            new Bone(Astra.JointType.RightWrist, Astra.JointType.RightHand),
            // left leg
            new Bone(Astra.JointType.BaseSpine, Astra.JointType.LeftHip),
            new Bone(Astra.JointType.LeftHip, Astra.JointType.LeftKnee),
            new Bone(Astra.JointType.LeftKnee, Astra.JointType.LeftFoot),
            // right leg
            new Bone(Astra.JointType.BaseSpine, Astra.JointType.RightHip),
            new Bone(Astra.JointType.RightHip, Astra.JointType.RightKnee),
            new Bone(Astra.JointType.RightKnee, Astra.JointType.RightFoot),
    };

    #endregion
}