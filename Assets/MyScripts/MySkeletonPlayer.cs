/* Created by: Alex Wang
 * Date: 07/22/2019
 * MySkeletonPlayer is responsible for rendering the skeleton based on the recorded data.
 * It automatically switches to the record scene after one round of replay.
 * Round is adjustable.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MySkeletonPlayer : MonoBehaviour
{
    private readonly Vector3 JOINTSCALE = new Vector3(0.01f, 0.01f, 0.01f);
    public GameObject JointPrefab;
    public GameObject[] joints;
    public GameObject[] bones;
    public Transform PlayerRoot;
    public Transform BoneRoot;
    private int index;
    bool isOver;

    private const int MAXROUNDS = 1;
    private int roundCounter;
    public Text roundText;

    #region 3d body model prefabs
    //Bone Prefabs
    public GameObject Prefab_Head_Neck;
    public GameObject Prefab_MidSpine_ShoulderSpine;
    public GameObject Prefab_BaseSpine_MidSpine;
    public GameObject Prefab_LeftShoulder_LeftElbow;
    public GameObject Prefab_LeftElbow_LeftWrist;
    public GameObject Prefab_ShoudlerSpine_LeftShoulder;
    public GameObject Prefab_ShoulderSpine_RightShoulder;
    public GameObject Prefab_RightShoulder_RightElbow;
    public GameObject Prefab_RightElbow_RightWrist;
    public GameObject Prefab_ShoulderSpine_Neck;
    public GameObject Prefab_BaseSpine_LeftHip;
    public GameObject Prefab_LeftHip_LeftKnee;
    public GameObject Prefab_LeftKnee_LeftFoot;
    public GameObject Prefab_BaseSpine_RightHip;
    public GameObject Prefab_RightHip_RightKnee;
    public GameObject Prefab_RightKnee_RightFoot;
    public GameObject Prefab_Head_bone;
    public GameObject Prefab_LeftHand;
    public GameObject Prefab_RightHand;

    private readonly float TestBoneThickness = 1f;
    private readonly float HeadBoneThickness = 0.2f;

    #endregion

    #region Bone Data Structure
    /// <summary>
    /// Bone is connector of two joints
    /// </summary>
    private struct Bone
    {
        public Astra.JointType _startJoint;
        public Astra.JointType _endJoint;

        public Bone(Astra.JointType startJoint, Astra.JointType endJoint)
        {
            _startJoint = startJoint;
            _endJoint = endJoint;
        }
    };

    /// <summary>
    /// Skeleton structure = list of bones = list of joint connectors
    /// </summary>
    private Bone[] Bones = new Bone[]
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

        bones = new GameObject[Bones.Length];
        bones[0] = (GameObject)Instantiate(Prefab_BaseSpine_MidSpine, Vector3.zero, Quaternion.identity);
        bones[0].transform.SetParent(BoneRoot);
        bones[1] = (GameObject)Instantiate(Prefab_MidSpine_ShoulderSpine, Vector3.zero, Quaternion.identity);
        bones[1].transform.SetParent(BoneRoot);
        bones[2] = (GameObject)Instantiate(Prefab_ShoulderSpine_Neck, Vector3.zero, Quaternion.identity);
        bones[2].transform.SetParent(BoneRoot);
        bones[3] = (GameObject)Instantiate(Prefab_Head_bone, Vector3.zero, Quaternion.identity);
        bones[3].transform.SetParent(BoneRoot);
        bones[4] = (GameObject)Instantiate(Prefab_ShoudlerSpine_LeftShoulder, Vector3.zero, Quaternion.identity);
        bones[4].transform.SetParent(BoneRoot);
        bones[5] = (GameObject)Instantiate(Prefab_LeftShoulder_LeftElbow, Vector3.zero, Quaternion.identity);
        bones[5].transform.SetParent(BoneRoot);
        bones[6] = (GameObject)Instantiate(Prefab_LeftElbow_LeftWrist, Vector3.zero, Quaternion.identity);
        bones[6].transform.SetParent(BoneRoot);
        bones[7] = (GameObject)Instantiate(Prefab_LeftHand, Vector3.zero, Quaternion.identity);
        bones[7].transform.SetParent(BoneRoot);
        bones[8] = (GameObject)Instantiate(Prefab_ShoulderSpine_RightShoulder, Vector3.zero, Quaternion.identity);
        bones[8].transform.SetParent(BoneRoot);
        bones[9] = (GameObject)Instantiate(Prefab_RightShoulder_RightElbow, Vector3.zero, Quaternion.identity);
        bones[9].transform.SetParent(BoneRoot);
        bones[10] = (GameObject)Instantiate(Prefab_RightElbow_RightWrist, Vector3.zero, Quaternion.identity);
        bones[10].transform.SetParent(BoneRoot);
        bones[11] = (GameObject)Instantiate(Prefab_RightHand, Vector3.zero, Quaternion.identity);
        bones[11].transform.SetParent(BoneRoot);
        bones[12] = (GameObject)Instantiate(Prefab_BaseSpine_LeftHip, Vector3.zero, Quaternion.identity);
        bones[12].transform.SetParent(BoneRoot);
        bones[13] = (GameObject)Instantiate(Prefab_LeftHip_LeftKnee, Vector3.zero, Quaternion.identity);
        bones[13].transform.SetParent(BoneRoot);
        bones[14] = (GameObject)Instantiate(Prefab_LeftKnee_LeftFoot, Vector3.zero, Quaternion.identity);
        bones[14].transform.SetParent(BoneRoot);
        bones[15] = (GameObject)Instantiate(Prefab_BaseSpine_RightHip, Vector3.zero, Quaternion.identity);
        bones[15].transform.SetParent(BoneRoot);
        bones[16] = (GameObject)Instantiate(Prefab_RightHip_RightKnee, Vector3.zero, Quaternion.identity);
        bones[16].transform.SetParent(BoneRoot);
        bones[17] = (GameObject)Instantiate(Prefab_RightKnee_RightFoot, Vector3.zero, Quaternion.identity);
        bones[17].transform.SetParent(BoneRoot);
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

            if (MyJointTracker.jointStats[jointType][frameIndex] != Vector3.zero)
            {
                if (!joints[i].activeSelf)
                {
                    joints[i].SetActive(true);
                }
                joints[i].transform.localPosition = (Vector3)MyJointTracker.jointStats[jointType][frameIndex];
                joints[i].transform.localScale = JOINTSCALE;
            }
            else
            {
                if (joints[i].activeSelf)
                {
                    joints[i].SetActive(false);
                }
            }

        }

        //Render the bones
        for (int i = 0; i < Bones.Length; i++)
        {
            //actual gameobject bones
            var skeletonBone = bones[i];
            //bones a body should have
            var bodyBone = Bones[i];
            int startIndex = (int)bodyBone._startJoint;
            int endIndex = (int)bodyBone._endJoint;
            var startJoint = joints[startIndex];
            var endJoint = joints[endIndex];

            if (startJoint.activeSelf && endJoint.activeSelf)
            {
                if (!skeletonBone.activeSelf)
                {
                    skeletonBone.SetActive(true);
                }


                #region Draw all bones
                Vector3 startPosition = startJoint.transform.position;
                Vector3 endPosition = endJoint.transform.position;

                float squaredMagnitude = Mathf.Pow(endPosition.x - startPosition.x, 2) + Mathf.Pow(endPosition.y - startPosition.y, 2);
                float magnitude = Mathf.Sqrt(squaredMagnitude);

                skeletonBone.transform.position = (startPosition + endPosition) / 2.0f;
                skeletonBone.transform.localEulerAngles = new Vector3(0, 0, find2DAngles(endPosition.x - startPosition.x, endPosition.y - startPosition.y));

                //Scale the head
                //Not sure why the calculated magnitude is off. I am guessing it is because the squaredMagnitude ignored the z coordination
                //08/06/2019 Alex
                if (startIndex == (int)Astra.JointType.Neck)
                {
                    skeletonBone.transform.localScale = new Vector3(HeadBoneThickness, magnitude * 0.3f, HeadBoneThickness);
                }
                //Scale the hands
                else if (startIndex == (int)Astra.JointType.LeftWrist || startIndex == (int)Astra.JointType.RightWrist)
                {
                    skeletonBone.transform.localScale = JOINTSCALE;
                }
                //Scale other bones
                else
                {
                    skeletonBone.transform.localScale = new Vector3(TestBoneThickness, magnitude * 0.3f, TestBoneThickness);
                }
                #endregion
            }
            else
            {
                if (skeletonBone.activeSelf) skeletonBone.SetActive(false);
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

    #region Helper Methods
    private float find2DAngles(float x, float y)
    {
        return -RadiansToDegrees((float)Mathf.Atan2(x, y));
    }

    private float RadiansToDegrees(float radians)
    {
        float angle = radians * 180 / (float)Mathf.PI;
        return angle;
    }
    #endregion
}