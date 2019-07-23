/* Created by: Alex Wang
 * Date: 07/22/2019
 * MySkeletonPlayer is responsible for rendering the skeleton based on the recorded data.
 * It automatically switches to the record scene once the data is exhausted.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySkeletonPlayer : MonoBehaviour
{
    private readonly Vector3 JOINTSCALE = new Vector3(1f, 1f, 1f);
    public GameObject JointPrefab;
    public GameObject[] joints;
    public Transform PlayerRoot;
    private int index;
    bool isOver;

    void Start()
    {
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
        if (!isOver)
        {
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
        }
        //If over, resets the stats container
        else
        {
            Reset(MyJointTracker.jointStats);
            SceneManager.LoadScene("Record");
        }

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