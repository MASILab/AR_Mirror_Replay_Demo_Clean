using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySkeletonPlayer : MonoBehaviour {
    private static GameObject[] _bodyJoints = new GameObject[MyStatRecorder.getNumOfJoints()];       //store joints[] of each body
    private GameObject[] _bodyBones = new GameObject[Bones.Length];

    public GameObject JointPrefab;
    public GameObject skeletonRoot;
    private readonly Vector3 NormalPoseScale = new Vector3(0.1f, 0.1f, 0.1f);

    public static int currentFrame = 0;
    private float time = 0;
    public static float playFPS = 30.0f;
    public static int NoRound;

    private static Vector3 centerVector;
    private static readonly float ZAXIS_ADJUST_VALUE = 0.8f;
    private Vector3 adjustVector;
    private Vector3 startPos;

    public static Astra.JointType playJointType = Astra.JointType.LeftHand;

    // Use this for initialization
    void Start () {
        centerVector = skeletonRoot.transform.position;
        NoRound = 1;
    }
	
	// Update is called once per frame
	void Update () {
        centerVector = skeletonRoot.transform.position;
        time += Time.deltaTime;
        if (time >= (float)1.0 / playFPS)
        {
            time = 0;
            currentFrame++;
            if (currentFrame >= MyStatRecorder.getDataSize(playJointType))
            {
                //whole replay is over, restart
                currentFrame = 1;
                NoRound = 1;
            }
            else if (NoRound < MyStatRecorder.roundLabel.Length
                && currentFrame - 1 == MyStatRecorder.roundLabel[NoRound] - 1)
            {
                currentFrame++;
                NoRound++;
                Debug.Log("Suspending...");
                //adjust starting position
                if (startPos != Vector3.zero)
                {
                    MyStatRecorder.Data data = MyStatRecorder.getStats(playJointType, currentFrame - 1);
                    adjustVector = startPos - new Vector3(data.posX, data.posY, data.posZ);

                    Debug.Log("nextPos: " + new Vector3(data.posX, data.posY, data.posZ) + " | adjustVector: " + adjustVector);
                }
            }

            //adjust the starting position
            if (currentFrame == 1)
            {
                //first round of movement - baseline
                MyStatRecorder.Data data = MyStatRecorder.getStats(playJointType, currentFrame - 1);
                startPos = new Vector3(data.posX, data.posY, data.posZ);
                adjustVector = new Vector3(0, 0, 0);

                Debug.Log("startPos: " + data.posX + ", " + data.posY + ", " + data.posZ);
                Debug.Log("playJointType: " + MyStatRecorder.TypeToString(playJointType) + " | data index: " + (currentFrame - 1));
            }
            UpdateSkeletonsFromBodies();
        }
	}

    void UpdateSkeletonsFromBodies()
    {
        #region draw joints

        for (int i = 0; i < MyStatRecorder.getNumOfJoints(); i++)
        {
            if (_bodyJoints[i] == null)
            {
                _bodyJoints[i] = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
                _bodyJoints[i].transform.localScale = NormalPoseScale;
            }

            if (!_bodyJoints[i].activeSelf)
            {
                _bodyJoints[i].SetActive(true);
            }

            var jointData1 = MyStatRecorder.getStats(MyStatRecorder.NumberToType(i), currentFrame + 1);
            _bodyJoints[i].transform.position = centerVector + new Vector3(jointData1.posX, jointData1.posY, jointData1.posZ - ZAXIS_ADJUST_VALUE) + adjustVector;
        }

        #endregion

        /*
        #region draw bones (position, scale, rotation)

        for (int i = 0; i < Bones.Length; i++)
        {
            if (_bodyBones[i] == null)
            {
                _bodyBones[i] = (GameObject)Instantiate(BonePrefab, Vector3.zero, Quaternion.identity);
            }
            if (!_bodyBones[i].activeSelf)
            {
                _bodyBones[i].SetActive(true);
            }

            var skeletonBone = _bodyBones[i];    //actual gameobject bone
            var bodyBone = Bones[i];                    //bones a body should have
            var parentJointType = bodyBone.ParentJointType;
            var endJointType = bodyBone.EndJointType;

            jointData1 = RecordStats.getStats(parentJointType, currentFrame - 1);
            jointData2 = RecordStats.getStats(endJointType, currentFrame - 1);
            Vector3 parentPosition = new Vector3(jointData1.posX, jointData1.posY, jointData1.posZ - ZAXIS_ADJUST_VALUE);
            Vector3 endPosition = new Vector3(jointData2.posX, jointData2.posY, jointData2.posZ - ZAXIS_ADJUST_VALUE);

            float magnitude = Mathf.Pow(endPosition.x - parentPosition.x, 2)
                                        + Mathf.Pow(endPosition.y - parentPosition.y, 2)
                                        + Mathf.Pow(endPosition.z - parentPosition.z, 2);
            magnitude = Mathf.Sqrt(magnitude);

            skeletonBone.transform.position = centerVector + (parentPosition + endPosition) / 2.0f + adjustVector;
            skeletonBone.transform.localEulerAngles = AngleBetweenPositions(parentPosition, endPosition);
            skeletonBone.transform.localScale = new Vector3(NormalBoneThickness, magnitude, NormalBoneThickness);
        }

        #endregion
        */

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
