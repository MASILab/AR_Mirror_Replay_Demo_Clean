using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class MyStatRecorder
{
    //time, position xyz
    public struct Data
    {
        public int time;
        public float posX, posY, posZ, velMag, accMag;
        public Data(int t, float x, float y, float z, float vel, float acc)
        {
            time = t;
            posX = x; posY = y; posZ = z;
            velMag = vel; accMag = acc;
        }

        public string toString()
        {
            return time + ":" + posX + "," + posY + "," + posZ;
        }
    }

    //Data of all joints
    private static Dictionary<Astra.JointType, List<Data>> _jointGroup = new Dictionary<Astra.JointType, List<Data>>()
    {
        {Astra.JointType.BaseSpine, new List<Data>() },
        {Astra.JointType.Head, new List<Data>() },
        {Astra.JointType.LeftElbow, new List<Data>() },
        {Astra.JointType.LeftFoot, new List<Data>() },
        {Astra.JointType.LeftHand, new List<Data>() },
        {Astra.JointType.LeftHip, new List<Data>() },
        {Astra.JointType.LeftKnee, new List<Data>() },
        {Astra.JointType.LeftShoulder, new List<Data>() },
        {Astra.JointType.LeftWrist, new List<Data>() },
        {Astra.JointType.MidSpine, new List<Data>() },
        {Astra.JointType.Neck, new List<Data>() },
        {Astra.JointType.RightElbow, new List<Data>() },
        {Astra.JointType.RightFoot, new List<Data>() },
        {Astra.JointType.RightHand, new List<Data>() },
        {Astra.JointType.RightHip, new List<Data>() },
        {Astra.JointType.RightKnee, new List<Data>() },
        {Astra.JointType.RightShoulder, new List<Data>() },
        {Astra.JointType.RightWrist, new List<Data>() },
        {Astra.JointType.ShoulderSpine, new List<Data>() }
    };

    //Data Export File
    public static readonly string DATA_EXPORT_FILE = "/FILE_TRACKING_DATA.txt";
    public static int[] roundLabel;

    private static int framesToRecord = 0;
    public static readonly int MAX_FRAME_TO_RECORD = 30 * 90;       //recording time limit (can be changed)
    public static float maxVel = -1;
    public static float maxAcc = -1;
    public static int numOfRecords;
    private static readonly float MIN_VALID_VEL = 0.8f;                 //for normalization of the data

    public static readonly int NUM_OF_JOINTS = 19;

    public static void findRoundLabel()
    {
        roundLabel = new int[numOfRecords];       //index in the list<data> of recordJointType
        roundLabel[0] = 0;
        for (int i = 1; i < numOfRecords; i++)
        {
            int j = roundLabel[i - 1] + 1;
            while (j < _jointGroup[MySkeletonRenderer.recordJointType].Count
                && _jointGroup[MySkeletonRenderer.recordJointType][j].time != -1)
            {
                j++;
            }
            roundLabel[i] = j + 1;
        }

        //Debug
        string str = "RoundLabel: ";
        foreach (int element in roundLabel)
        {
            str += element + ", ";
        }
        Debug.Log(str);
    }

    //data export
    public static void dataExport()
    {
        findRoundLabel();

        string filePath = Application.dataPath + DATA_EXPORT_FILE;
        StreamWriter sWriter;

        if (!File.Exists(filePath))
        {
            sWriter = File.CreateText(Application.dataPath + DATA_EXPORT_FILE);
        }
        else
        {
            sWriter = new StreamWriter(filePath);
        }


        sWriter.WriteLine("Data exported on: " + DateTime.Now);
        sWriter.WriteLine("Number of Rounds: " + numOfRecords);
        sWriter.WriteLine("Each line of the following data contains: " +
            "frame number, position on x-axis, position on y-axis, position on z-axis, velocity and acceleration.");

        for (int round = 1; round <= numOfRecords; round++)
        {
            sWriter.WriteLine("");
            sWriter.WriteLine("Round " + round);
            sWriter.WriteLine("");
            for (int i = 0; i < _jointGroup.Count; i++)
            {
                sWriter.WriteLine("Joint: " + TypeToString(NumberToType(i)));

                int startIndex = roundLabel[round - 1];
                int endIndex;
                if (round == numOfRecords)
                {
                    endIndex = _jointGroup[MySkeletonRenderer.recordJointType].Count - 1;
                }
                else
                {
                    endIndex = roundLabel[round] - 2;
                }

                for (int j = startIndex; j <= endIndex; j++)
                {
                    Data tmpData = _jointGroup[NumberToType(i)][j];
                    sWriter.WriteLine(string.Format("{0:+0000;-0000}", tmpData.time) + "\t"
                                                    + string.Format("{0:+00.000000;-00.000000}", tmpData.posX) + "\t"
                                                    + string.Format("{0:+00.000000;-00.000000}", tmpData.posY) + "\t"
                                                    + string.Format("{0:+00.000000;-00.000000}", tmpData.posZ) + "\t"
                                                    + string.Format("{0:+00.000000;-00.000000}", tmpData.velMag) + "\t"
                                                    + string.Format("{0:+00.000000;-00.000000}", tmpData.accMag));
                }

            }
        }

        sWriter.Close();
    }

    //data cut (ensure timeline are the same for all joints)
    public static void dataCut()
    {
        int minLen = -1;
        for (int i = 0; i < _jointGroup.Count; i++)
        {
            if (minLen == -1 || _jointGroup[NumberToType(i)].Count < minLen)
            {
                minLen = _jointGroup[NumberToType(i)].Count;
            }
        }
        for (int i = 0; i < _jointGroup.Count; i++)
        {
            while (_jointGroup[NumberToType(i)].Count > minLen)
            {
                _jointGroup[NumberToType(i)].RemoveAt(_jointGroup[NumberToType(i)].Count - 1);
            }
        }
    }

    //normalization of multiple rounds of data (Adjust Time by Velocity)
    public static void VelNormalization()
    {
        #region preprocess data (set to same length)

        dataCut();      //set each joint's data list to same time length

        List<Data> list = _jointGroup[MySkeletonRenderer.recordJointType];
        int num = numOfRecords;
        int dataLength = 0;                 //find the max length of a round of data
        int tmpLength = 0;
        string dataStr = "data:\n";
        for (int i = 0; i < list.Count; i++)
        {
            dataStr += list[i].time + "-" + list[i].velMag + "\n";
            if (list[i].time != -1)
            {
                tmpLength++;
            }
            else
            {
                if (tmpLength > dataLength)
                {
                    dataLength = tmpLength;
                }
                tmpLength = 0;
            }
        }
        if (tmpLength > dataLength)
        {
            dataLength = tmpLength;
        }
        Debug.Log(dataStr);
        Debug.Log("totalLen: " + list.Count);
        Debug.Log("maxdataLen: " + dataLength);

        /*
         * dataLength = 3
         * 0 1 2 3 4 5 6 7 8
         *_ _ _ * * * _ _ _
         */
        float[,] data = new float[num, dataLength * 3];
        for (int i = 0; i < num; i++)                                           //initialize 2d array to zeros
        {
            for (int j = 0; j < dataLength * 3; j++)
            {
                data[i, j] = 0f;
            }
        }

        int row = 0, col = dataLength;
        for (int i = 0; i < list.Count; i++)                                //assign each round of data into the 2d array
        {
            if (list[i].time != -1)
            {
                data[row, col] = list[i].velMag;
                col++;
            }
            else
            {
                row++; col = dataLength;
            }
        }

        string str1 = "data\n";                         //Debug purpose only
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < dataLength * 3; j++)
            {
                str1 += j + ":" + data[i, j] + "  ";
            }
            str1 += "\n";
        }
        Debug.Log(str1);

        for (int i = 0; i < num; i++)                   //find the highest data point & original length of each round of data
        {
            int Len = 0;
            int leftlndex = dataLength;
            int rightIndex = dataLength * 2 - 1;
            for (int j = dataLength; j < dataLength * 2; j++)
            {
                if (leftlndex == dataLength && data[i, j] >= MIN_VALID_VEL)
                {
                    leftlndex = j;
                }
                if (rightIndex == dataLength * 2 - 1 || data[i, j] >= MIN_VALID_VEL)
                {
                    rightIndex = j;
                }

                if (data[i, j] != 0f)
                {
                    Len++;
                }
            }
            data[i, 1] = leftlndex - dataLength;           //store leftIndex in data[row, 1]
            data[i, 2] = rightIndex - dataLength;        //store rightIndex in data[row,2]
            data[i, 3] = Len;                   //store the original length of this round of data in data[row, 3]
            Debug.Log("Len: " + data[i, 3]);
        }

        #endregion

        #region calculate the baseline (average data / first round of data / expert data)

        /*
         * dataLength = 3
         * 0 1 2 
         * # # #
         */
        float[] baseline = new float[dataLength];
        for (int i = 0; i < dataLength; i++)
        {
            /*
            //average data
            baseline[i] = 0;
            for (int j=0; j<num; j++)
            {
                baseline[i] += data[j, dataLength + i];
            }
            baseline[i] = baseline[i] / (float)num;
            */

            //first round of data
            baseline[i] = data[0, dataLength + i];
        }

        #endregion

        #region adjust each round of data to baseline (min std dev)

        /*
         * dataLength = 3
         * 0 1 2 3 4 5 6 7 8
         *_ _ _ a b c _ _ _         baseline
         *_ _ _ a b c _ _ _          data (move to adjust std dev)
         *_ _ a b c _ _ _ _         leftward (-1)
         *_ _ _ _ a b c _ _         rightward (+1)
         */
        for (int i = 0; i < num; i++)
        {
            float minStdDev = -1;
            int minOffset = 0;

            for (int offset = (int)(-data[i, 1]); offset <= (int)(data[i, 3] - 1 - data[i, 2])/*(dataLength - 1 - data[i,2])*/; offset++)
            {
                //calculate std dev
                float stdDev = 0;
                for (int j = 0; j < dataLength; j++)
                {
                    float avg = baseline[j];
                    float sample = data[i, j + dataLength - offset];
                    stdDev += Mathf.Pow(sample - avg, 2);
                }
                stdDev = Mathf.Sqrt(stdDev / dataLength);

                if (minStdDev == -1 || stdDev < minStdDev)
                {
                    minStdDev = stdDev;
                    minOffset = offset;
                }
            }

            data[i, 0] = minOffset;         //save offset at col 0
        }

        #endregion

        #region put processed data back into the list (timeLine hold still)

        //if the round of data is moved rightward (postponed), fill the beginning period with original starting position
        //if the round of data is moved leftward, fill the ending period with original ending position
        //int NoRound = 1;
        int FrameIndex = 0;
        for (int NoRound = 1; NoRound <= num; NoRound++)
        {
            int StartIndex = FrameIndex;        //start and end of this round of movement
            int EndIndex;
            Astra.JointType recordType = MySkeletonRenderer.recordJointType;
            while (FrameIndex < _jointGroup[recordType].Count - 1
                && _jointGroup[recordType][FrameIndex].time != -1)
            {
                //Debug.Log("Index: " + FrameIndex + " - " + _jointGroup[recordType][FrameIndex].time);
                FrameIndex++;
            }
            if (_jointGroup[recordType][FrameIndex].time == -1)
            {
                EndIndex = FrameIndex - 1;
            }
            else
            {
                EndIndex = FrameIndex;
            }
            FrameIndex++;

            int MoveOffset = (int)data[NoRound - 1, 0];
            Debug.Log("S:" + StartIndex + " E:" + EndIndex + " O:" + MoveOffset);
            Debug.Log("L: " + data[NoRound - 1, 1] + " R: " + data[NoRound - 1, 2] + " Len: " + data[NoRound - 1, 3]);
            //MoveOffset = 0;
            if (Mathf.Abs(MoveOffset) >= EndIndex - StartIndex || MoveOffset == 0)
            {
                //do nothing, invalid offset
            }
            else if (MoveOffset < 0)
            {
                //leftward  

                for (int i = 0; i < _jointGroup.Count; i++)
                {
                    for (int j = StartIndex - MoveOffset; j <= EndIndex; j++)
                    {
                        _jointGroup[NumberToType(i)][j + MoveOffset] = _jointGroup[NumberToType(i)][j];
                    }
                    for (int j = EndIndex + MoveOffset + 1; j <= EndIndex; j++)
                    {
                        _jointGroup[NumberToType(i)][j] = _jointGroup[NumberToType(i)][EndIndex + MoveOffset];
                    }
                }

            }
            else
            {
                //rightward

                for (int i = 0; i < _jointGroup.Count; i++)
                {
                    for (int j = EndIndex; j >= StartIndex + MoveOffset; j--)
                    {
                        _jointGroup[NumberToType(i)][j] = _jointGroup[NumberToType(i)][j - MoveOffset];
                    }
                    for (int j = StartIndex; j < StartIndex + MoveOffset; j++)
                    {
                        _jointGroup[NumberToType(i)][j] = _jointGroup[NumberToType(i)][StartIndex + MoveOffset];
                    }
                }

            }
        }

        #endregion

    }

    public static void addStats(Astra.JointType jointType, int t = 0, float x = 0, float y = 0, float z = 0, float vel = 0, float acc = 0)
    {
        if (!recordFinished(jointType))
        {
            //statsList.Add(new Data(t, x, y, z));
            _jointGroup[jointType].Add(new Data(t, x, y, z, vel, acc));

            if (jointType == MySkeletonRenderer.recordJointType)
            {
                if (vel > maxVel) { maxVel = vel; }
                if (acc > maxAcc) { maxAcc = acc; }
            }
        }
    }

    public static Data getStats(Astra.JointType jointType, int index)
    {
        if (index >= 0 && index < getDataSize(jointType))
        {
            //return statsList[index];
            return _jointGroup[jointType][index];
        }
        else if (getDataSize(jointType) == 0)
        {
            return new Data(0, 0, 0, 0, 0, 0);
        }
        else
        {
            return _jointGroup[jointType][getDataSize(jointType) - 1];
        }

    }

    public static void setNumOfFrames(int t)
    {
        framesToRecord = t;
    }

    public static int getNumOfFrames()
    {
        return _jointGroup[MySkeletonRenderer.recordJointType].Count;
    }

    public static bool recordFinished(Astra.JointType jointType)
    {
        //return statsList.Count >= framesToRecord;
        //Debug.Log("recordFinished - jointType: " + TypeToString(jointType));
        return _jointGroup[jointType].Count >= MAX_FRAME_TO_RECORD;
    }

    public static int getDataSize(Astra.JointType jointType)
    {
        //return statsList.Count;
        return _jointGroup[jointType].Count;
    }

    public static int getNumOfJoints()
    {
        return _jointGroup.Count;
    }

    public static void clear()
    {
        for (int i = 0; i < getNumOfJoints(); i++)
        {
            _jointGroup[NumberToType(i)].Clear();
        }
        maxAcc = 0;
        maxVel = 0;
    }

    #region Global Helper Methods

    public static Astra.JointType NumberToType(int number)
    {
        switch (number)
        {
            case 0:
                return Astra.JointType.BaseSpine;
            case 1:
                return Astra.JointType.Head;
            case 2:
                return Astra.JointType.LeftElbow;
            case 3:
                return Astra.JointType.LeftFoot;
            case 4:
                return Astra.JointType.LeftHand;
            case 5:
                return Astra.JointType.LeftHip;
            case 6:
                return Astra.JointType.LeftKnee;
            case 7:
                return Astra.JointType.LeftShoulder;
            case 8:
                return Astra.JointType.LeftWrist;
            case 9:
                return Astra.JointType.MidSpine;
            case 10:
                return Astra.JointType.Neck;
            case 11:
                return Astra.JointType.RightElbow;
            case 12:
                return Astra.JointType.RightFoot;
            case 13:
                return Astra.JointType.RightHand;
            case 14:
                return Astra.JointType.RightHip;
            case 15:
                return Astra.JointType.RightKnee;
            case 16:
                return Astra.JointType.RightShoulder;
            case 17:
                return Astra.JointType.RightWrist;
            case 18:
                return Astra.JointType.ShoulderSpine;
            default:
                return Astra.JointType.Unknown;
        }
    }

    public static int TypeToNumber(Astra.JointType type)
    {
        switch (type)
        {
            case Astra.JointType.BaseSpine:
                return 0;
            case Astra.JointType.Head:
                return 1;
            case Astra.JointType.LeftElbow:
                return 2;
            case Astra.JointType.LeftFoot:
                return 3;
            case Astra.JointType.LeftHand:
                return 4;
            case Astra.JointType.LeftHip:
                return 5;
            case Astra.JointType.LeftKnee:
                return 6;
            case Astra.JointType.LeftShoulder:
                return 7;
            case Astra.JointType.LeftWrist:
                return 8;
            case Astra.JointType.MidSpine:
                return 9;
            case Astra.JointType.Neck:
                return 10;
            case Astra.JointType.RightElbow:
                return 11;
            case Astra.JointType.RightFoot:
                return 12;
            case Astra.JointType.RightHand:
                return 13;
            case Astra.JointType.RightHip:
                return 14;
            case Astra.JointType.RightKnee:
                return 15;
            case Astra.JointType.RightShoulder:
                return 16;
            case Astra.JointType.RightWrist:
                return 17;
            case Astra.JointType.ShoulderSpine:
                return 18;
            default:
                return 0;
        }
    }

    public static Astra.JointType StringToType(string str)
    {
        switch (str)
        {
            case "BaseSpine":
                return Astra.JointType.BaseSpine;
            case "Head":
                return Astra.JointType.Head;
            case "LeftElbow":
                return Astra.JointType.LeftElbow;
            case "LeftFoot":
                return Astra.JointType.LeftFoot;
            case "LeftHand":
                return Astra.JointType.LeftHand;
            case "LeftHip":
                return Astra.JointType.LeftHip;
            case "LeftKnee":
                return Astra.JointType.LeftKnee;
            case "LeftShoulder":
                return Astra.JointType.LeftShoulder;
            case "LeftWrist":
                return Astra.JointType.LeftWrist;
            case "MidSpine":
                return Astra.JointType.MidSpine;
            case "Neck":
                return Astra.JointType.Neck;
            case "RightElbow":
                return Astra.JointType.RightElbow;
            case "RightFoot":
                return Astra.JointType.RightFoot;
            case "RightHand":
                return Astra.JointType.RightHand;
            case "RightHip":
                return Astra.JointType.RightHip;
            case "RightKnee":
                return Astra.JointType.RightKnee;
            case "RightShoulder":
                return Astra.JointType.RightShoulder;
            case "RightWrist":
                return Astra.JointType.RightWrist;
            case "ShoulderSpine":
                return Astra.JointType.ShoulderSpine;
            default:
                return Astra.JointType.Unknown;
        }
    }

    public static string TypeToString(Astra.JointType type)
    {
        switch (type)
        {
            case Astra.JointType.BaseSpine:
                return "BaseSpine";
            case Astra.JointType.Head:
                return "Head";
            case Astra.JointType.LeftElbow:
                return "LeftElbow";
            case Astra.JointType.LeftFoot:
                return "LeftFoot";
            case Astra.JointType.LeftHand:
                return "LeftHand";
            case Astra.JointType.LeftHip:
                return "LeftHip";
            case Astra.JointType.LeftKnee:
                return "LeftKnee";
            case Astra.JointType.LeftShoulder:
                return "LeftShoulder";
            case Astra.JointType.LeftWrist:
                return "LeftWrist";
            case Astra.JointType.MidSpine:
                return "MidSpine";
            case Astra.JointType.Neck:
                return "Neck";
            case Astra.JointType.RightElbow:
                return "RightElbow";
            case Astra.JointType.RightFoot:
                return "RightFoot";
            case Astra.JointType.RightHand:
                return "RightHand";
            case Astra.JointType.RightHip:
                return "RightHip";
            case Astra.JointType.RightKnee:
                return "RightKnee";
            case Astra.JointType.RightShoulder:
                return "RightShoulder";
            case Astra.JointType.RightWrist:
                return "RightWrist";
            case Astra.JointType.ShoulderSpine:
                return "ShoulderSpine";
            default:
                return "Unknown";
        }
    }


    #endregion

}
