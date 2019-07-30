using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MyDataExporter : MonoBehaviour {
    public string fileName = "TestRecord.csv";
    private System.IO.StreamWriter file;

    // Use this for initialization
    private void Start () {
        CreateFile();
        WriteToFile();
    }

    private void CreateFile()
    {
        try
        {
            file = new System.IO.StreamWriter(@fileName, true);
            {
                file.WriteLine("All joint Positions");
                foreach (var joint in MyJointTracker.Joints)
                {
                    file.Write(joint.ToString() + ",");
                }
                file.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void WriteToFile()
    {
        try
        {
            for (int frameIndex = 0; frameIndex < MyJointTracker.jointStats[MyJointTracker.Joints[0]].Count; ++frameIndex)
            {
                foreach (var joint in MyJointTracker.Joints)
                {
                    file.Write(DataToString(MyJointTracker.jointStats[joint][frameIndex]) + ",");
                }
                file.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //joint position in game = joint world position / 1000
    //Outputing the world position
    private string DataToString(Vector3 vector3)
    {
        return "(" + (vector3.x * 1000).ToString("0.##") + " " + (vector3.y * 1000).ToString("0.##") + " " + (vector3.z * 1000).ToString("0.##") + ")";
    }
}
