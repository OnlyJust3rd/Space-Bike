//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using System.IO.Ports;

//public class ArduinoController : MonoBehaviour
//{
//    SerialPort data_stream = new SerialPort("COM8", 9600);
//    public string recievedstring;
//    public Image test_data;

//    public string[] datas;

//    private void Start()
//    {
//        print(data_stream.IsOpen);
//        data_stream.Open();
//        print(data_stream.IsOpen);
//    }

//    private void Update()
//    {
//        recievedstring = data_stream.ReadLine();

//        print(recievedstring);
//        if(recievedstring != null && recievedstring != "") HandleData(recievedstring);
//        //float d = float.Parse(recievedstring);
//        //FindObjectOfType<PlayerController>().RechargeBattery(d);
//    }

//    private void HandleData(string s)
//    {
//        PlayerController p = FindObjectOfType<PlayerController>();

//        string[] data = s.Split(',');
//        if (data.Length < 5) return;
//        print($"speed:{data[0]} left:{data[1]} shoot:{data[2]} missile:{data[3]} right:{data[4]}");

//        // Battery
//        try
//        {
//            p.RechargeBattery(float.Parse(data[0]));
//        }
//        catch (System.FormatException e)
//        {
//            return;
//        }

//        float input = 0;
//        if (int.Parse(data[3]) == 0) input = 1;
//        if (int.Parse(data[2]) == 0) input = -1;

//        bool missile = false, shoot = false;
//        if (int.Parse(data[1]) == 0) shoot = true;
//        if (int.Parse(data[4]) == 0) missile = true;

//        p.InputManager(input, missile, shoot);
        
//    }

//    private void OnDestroy()
//    {
//        if (!Application.isPlaying)
//        {
//            data_stream.Close();
//        }
//    }
//}
