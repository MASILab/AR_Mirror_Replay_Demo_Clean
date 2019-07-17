/* Created by: Alex Wang, Anjie Wang
 * Date: 07/01/2019
 * MySkeletonRenderer is responsible for creating and rendering the joints and the bones.
 * It is adapted from the original SkeletonRenderer from the Astra Orbbec SDK 2.0.16.
 */
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MySkeletonRenderer : MonoBehaviour
{
    private long _lastFrameIndex = -1;

    private Astra.Body[] _bodies;
    private Dictionary<int, GameObject[]> _bodySkeletons;

    private readonly Vector3 NormalPoseScale = new Vector3(0.05f, 0.05f, 0.05f);
    private readonly Vector3 GripPoseScale = new Vector3(0.2f, 0.2f, 0.2f);

    public GameObject JointPrefab;
    public Transform JointRoot;

    public Toggle ToggleSeg = null;
    public Toggle ToggleSegBody = null;
    public Toggle ToggleSegBodyHand = null;

    public Toggle ToggleProfileFull = null;
    public Toggle ToggleProfileUpperBody = null;
    public Toggle ToggleProfileBasic = null;

    public Toggle ToggleOptimizationAccuracy = null;
    public Toggle ToggleOptimizationBalanced = null;
    public Toggle ToggleOptimizationMemory = null;
    public Slider SliderOptimization = null;

    private Astra.BodyTrackingFeatures _previousTargetFeatures = Astra.BodyTrackingFeatures.HandPose;
    private Astra.SkeletonProfile _previousSkeletonProfile = Astra.SkeletonProfile.Full;
    private Astra.SkeletonOptimization _previousSkeletonOptimization = Astra.SkeletonOptimization.BestAccuracy;

    #region Record&Replay variables
    public static Astra.JointType recordJointType = Astra.JointType.LeftHand;
    private Astra.Joint recordJoint = null;
    public static JointStats recordJointStats = new JointStats();
    public static JointStats[] AllJointStats = new JointStats[19];
    #endregion

    void Start()
    {
        _bodySkeletons = new Dictionary<int, GameObject[]>();
        _bodies = new Astra.Body[Astra.BodyFrame.MaxBodies];

        AllJointStats[0] = new JointStats(Astra.JointType.BaseSpine.ToString());
        AllJointStats[1] = new JointStats(Astra.JointType.Head.ToString());
        AllJointStats[2] = new JointStats(Astra.JointType.LeftElbow.ToString());
        AllJointStats[3] = new JointStats(Astra.JointType.LeftFoot.ToString());
        AllJointStats[4] = new JointStats(Astra.JointType.LeftHand.ToString());
        AllJointStats[5] = new JointStats(Astra.JointType.LeftHip.ToString());
        AllJointStats[6] = new JointStats(Astra.JointType.LeftKnee.ToString());
        AllJointStats[7] = new JointStats(Astra.JointType.LeftShoulder.ToString());
        AllJointStats[8] = new JointStats(Astra.JointType.LeftWrist.ToString());
        AllJointStats[9] = new JointStats(Astra.JointType.MidSpine.ToString());
        AllJointStats[10] = new JointStats(Astra.JointType.Neck.ToString());
        AllJointStats[11] = new JointStats(Astra.JointType.RightElbow.ToString());
        AllJointStats[12] = new JointStats(Astra.JointType.RightFoot.ToString());
        AllJointStats[13] = new JointStats(Astra.JointType.RightHand.ToString());
        AllJointStats[14] = new JointStats(Astra.JointType.RightHip.ToString());
        AllJointStats[15] = new JointStats(Astra.JointType.RightKnee.ToString());
        AllJointStats[16] = new JointStats(Astra.JointType.RightShoulder.ToString());
        AllJointStats[17] = new JointStats(Astra.JointType.RightWrist.ToString());
        AllJointStats[18] = new JointStats(Astra.JointType.ShoulderSpine.ToString());
    }

    public void OnNewFrame(Astra.BodyStream bodyStream, Astra.BodyFrame frame)
    {
        if (frame.Width == 0 ||
            frame.Height == 0)
        {
            return;
        }

        if (_lastFrameIndex == frame.FrameIndex)
        {
            return;
        }

        _lastFrameIndex = frame.FrameIndex;

        frame.CopyBodyData(ref _bodies);
        UpdateSkeletonsFromBodies(_bodies);
        UpdateBodyFeatures(bodyStream, _bodies);
        UpdateSkeletonProfile(bodyStream);
        UpdateSkeletonOptimization(bodyStream);
    }


    void UpdateSkeletonsFromBodies(Astra.Body[] bodies)
    {
        foreach (var body in bodies)
        {

            if (body.Status == Astra.BodyStatus.NotTracking)
            {
                continue;
            }

            GameObject[] joints;
            bool newBody = false;

            if (!_bodySkeletons.ContainsKey(body.Id))
            {
                //Instantiate joint gameobjects
                joints = new GameObject[body.Joints.Length];
                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i] = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
                    joints[i].transform.SetParent(JointRoot);
                }
                _bodySkeletons.Add(body.Id, joints);
                newBody = true;
            }
            else
            {
                joints = _bodySkeletons[body.Id];
            }

            //Log if a new body is detected
            if (newBody)
            {
                StartCoroutine(GetRequest("https://docs.google.com/forms/d/e/1FAIpQLSe9t2ffOIQF2zNo-W3mGsA0jW0Fpba65AW1vk8C8YI9o1Akyg/formResponse?entry.365241968=REPLAYDEMO&fvv=1"));
            }

            //Render the joints
            for (int i = 0; i < body.Joints.Length; i++)
            {
                var skeletonJoint = joints[i];
                var bodyJoint = body.Joints[i];

                if (bodyJoint.Status != Astra.JointStatus.NotTracked)
                {
                    skeletonJoint.transform.localPosition =
                        new Vector3(bodyJoint.WorldPosition.X / 1000f,
                                    bodyJoint.WorldPosition.Y / 1000f,
                                    bodyJoint.WorldPosition.Z / 1000f);


                    //skel.Joints[i].Orient.Matrix:
                    // 0, 			1,	 		2,
                    // 3, 			4, 			5,
                    // 6, 			7, 			8
                    // -------
                    // right(X),	up(Y), 		forward(Z)

                    //Vector3 jointRight = new Vector3(
                    //    bodyJoint.Orientation.M00,
                    //    bodyJoint.Orientation.M10,
                    //    bodyJoint.Orientation.M20);

                    Vector3 jointUp = new Vector3(
                        bodyJoint.Orientation.M01,
                        bodyJoint.Orientation.M11,
                        bodyJoint.Orientation.M21);

                    Vector3 jointForward = new Vector3(
                        bodyJoint.Orientation.M02,
                        bodyJoint.Orientation.M12,
                        bodyJoint.Orientation.M22);

                    skeletonJoint.transform.rotation =
                        Quaternion.LookRotation(jointForward, jointUp);

                    skeletonJoint.transform.localScale = NormalPoseScale;


                    int index = MyStatRecorder.TypeToNumber(bodyJoint.Type);
                    AllJointStats[index].updateStats(bodyJoint.WorldPosition.X / 1000f,
                                                     bodyJoint.WorldPosition.Y / 1000f,
                                                     bodyJoint.WorldPosition.Z / 1000f);

                    /*
                    string stats = AllJointStats[index].toString();
                    Debug.Log(stats);
                    */

                    /*
                    if (bodyJoint.Type == recordJointType)
                    {
                        recordJoint = bodyJoint;

                        recordJointStats.updateStats(recordJoint.WorldPosition.X / 1000f,
                                                     recordJoint.WorldPosition.Y / 1000f,
                                                     recordJoint.WorldPosition.Z / 1000f);
                    }
                    */
                }
                else
                {
                    if (skeletonJoint.activeSelf) skeletonJoint.SetActive(false);
                }
            }
        }
    }

    #region Helper Methods
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
            }
        }
    }
    #endregion

    private void UpdateHandPoseVisual(GameObject skeletonJoint, Astra.HandPose pose)
    {
        Vector3 targetScale = NormalPoseScale;
        if (pose == Astra.HandPose.Grip)
        {
            targetScale = GripPoseScale;
        }
        skeletonJoint.transform.localScale = targetScale;
    }

    private void UpdateBodyFeatures(Astra.BodyStream bodyStream, Astra.Body[] bodies)
    {
        if (ToggleSeg != null &&
            ToggleSegBody != null &&
            ToggleSegBodyHand != null)
        {
            Astra.BodyTrackingFeatures targetFeatures = Astra.BodyTrackingFeatures.Segmentation;
            if (ToggleSegBodyHand.isOn)
            {
                targetFeatures = Astra.BodyTrackingFeatures.HandPose;
            }
            else if (ToggleSegBody.isOn)
            {
                targetFeatures = Astra.BodyTrackingFeatures.Skeleton;
            }

            if (targetFeatures != _previousTargetFeatures)
            {
                _previousTargetFeatures = targetFeatures;
                foreach (var body in bodies)
                {
                    if (body.Status != Astra.BodyStatus.NotTracking)
                    {
                        bodyStream.SetBodyFeatures(body.Id, targetFeatures);
                    }
                }
                bodyStream.SetDefaultBodyFeatures(targetFeatures);
            }
        }
    }

    private void UpdateSkeletonProfile(Astra.BodyStream bodyStream)
    {
        if (ToggleProfileFull != null &&
            ToggleProfileUpperBody != null &&
            ToggleProfileBasic != null)
        {
            Astra.SkeletonProfile targetSkeletonProfile = Astra.SkeletonProfile.Full;
            if (ToggleProfileFull.isOn)
            {
                targetSkeletonProfile = Astra.SkeletonProfile.Full;
            }
            else if (ToggleProfileUpperBody.isOn)
            {
                targetSkeletonProfile = Astra.SkeletonProfile.UpperBody;
            }
            else if (ToggleProfileBasic.isOn)
            {
                targetSkeletonProfile = Astra.SkeletonProfile.Basic;
            }

            if (targetSkeletonProfile != _previousSkeletonProfile)
            {
                _previousSkeletonProfile = targetSkeletonProfile;
                bodyStream.SetSkeletonProfile(targetSkeletonProfile);
            }
        }
    }

    private void UpdateSkeletonOptimization(Astra.BodyStream bodyStream)
    {
        if (ToggleOptimizationAccuracy != null &&
            ToggleOptimizationBalanced != null &&
            ToggleOptimizationMemory != null &&
            SliderOptimization != null)
        {
            int targetOptimizationValue = (int)_previousSkeletonOptimization;
            if (ToggleOptimizationAccuracy.isOn)
            {
                targetOptimizationValue = (int)Astra.SkeletonOptimization.BestAccuracy;
            }
            else if (ToggleOptimizationBalanced.isOn)
            {
                targetOptimizationValue = (int)Astra.SkeletonOptimization.Balanced;
            }
            else if (ToggleOptimizationMemory.isOn)
            {
                targetOptimizationValue = (int)Astra.SkeletonOptimization.MinimizeMemory;
            }

            if (targetOptimizationValue != (int)_previousSkeletonOptimization)
            {
                Debug.Log("Set optimization slider: " + targetOptimizationValue);
                SliderOptimization.value = targetOptimizationValue;
            }

            Astra.SkeletonOptimization targetSkeletonOptimization = Astra.SkeletonOptimization.Balanced;
            int sliderValue = (int)SliderOptimization.value;

            switch (sliderValue)
            {
                case 1:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization1;
                    break;
                case 2:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization2;
                    break;
                case 3:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization3;
                    break;
                case 4:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization4;
                    break;
                case 5:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization5;
                    break;
                case 6:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization6;
                    break;
                case 7:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization7;
                    break;
                case 8:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization8;
                    break;
                case 9:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization9;
                    break;
                default:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization9;
                    SliderOptimization.value = 9;
                    break;
            }

            if (targetSkeletonOptimization != _previousSkeletonOptimization)
            {
                UpdateOptimizationToggles(targetSkeletonOptimization);

                Debug.Log("SetSkeletonOptimization: " + targetSkeletonOptimization);
                _previousSkeletonOptimization = targetSkeletonOptimization;
                bodyStream.SetSkeletonOptimization(targetSkeletonOptimization);
            }
        }
    }

    private void UpdateOptimizationToggles(Astra.SkeletonOptimization optimization)
    {
        ToggleOptimizationMemory.isOn = optimization == Astra.SkeletonOptimization.MinimizeMemory;
        ToggleOptimizationBalanced.isOn = optimization == Astra.SkeletonOptimization.Balanced;
        ToggleOptimizationAccuracy.isOn = optimization == Astra.SkeletonOptimization.BestAccuracy;
    }

    #region jointStats Class

    public class JointStats
    {
        public static float fps = 30;   //needs to be fixed

        public Astra.Joint joint = null;
        public string jointType;
        public float posX, posY, posZ;
        private float posX0 = 0, posY0 = 0, posZ0 = 0;

        public float velX, velY, velZ;
        private float velX0 = 0, velY0 = 0, velZ0 = 0;
        public float VelMag = 0;

        public float accX, accY, accZ;
        private float accX0 = 0, accY0 = 0, accZ0 = 0;
        public float AccMag = 0;

        public JointStats(string type = "unknown joint", float posX1 = 0, float posY1 = 0, float posZ1 = 0)
        {
            jointType = type;
            posX = posX1; posY = posY1; posZ = posZ1;
            posX0 = posX; posY0 = posY; posZ0 = posZ;
        }

        public void updateStats(float posX1, float posY1, float posZ1)
        {
            float t = (float)1.0 / fps;

            posX0 = posX; posY0 = posY; posZ0 = posZ;
            posX = posX1; posY = posY1; posZ = posZ1;

            velX0 = velX; velY0 = velY; velZ0 = velZ;
            velX = (posX - posX0) / t; velY = (posY - posY0) / t; velZ = (posZ - posZ0) / t;
            VelMag = Mathf.Pow(velX, 2) + Mathf.Pow(velY, 2) + Mathf.Pow(velZ, 2);
            VelMag = Mathf.Sqrt(VelMag);

            accX0 = accX; accY0 = accY; accZ0 = accZ;
            accX = (velX - velX0) / t; accY = (velY - velY0) / t; accZ = (velZ - velZ0) / t;
            AccMag = Mathf.Pow(accX, 2) + Mathf.Pow(accY, 2) + Mathf.Pow(accZ, 2);
            AccMag = Mathf.Sqrt(AccMag);
        }

        public string toString()
        {
            return jointType + "\n"
                + "Position " + string.Format("X: {0:+00.00;-00.00}", posX) + string.Format(" Y: {0:+00.00;-00.00}", posX) + string.Format(" Z: {0:+00.00;-00.00}", posX) + "\n";
        }

    }

    #endregion
}