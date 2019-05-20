using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class connect : MonoBehaviour
{
    public class Cmd
    {
        public const byte M2SQueryLink = 0xA9;//查询链接（握手）
        public const byte M2SDeliverRequest = 0xA3;//发送出货命令

        public const byte M2SEnterTestModelRequest = 0xC1;//发送进入测试命令
        public const byte M2SExitTestModelRequest = 0xC2;//发送退出测试命令

        public const byte M2SAllClearRequest = 0xC3;//发送一键清除故障
        public const byte M2SReplenishRequest = 0xC4;//发送补货

        public const byte M2SDeliverCupRequest = 0xC5;//发送出杯

        public const byte S2MQueryAck = 0xB9;//查询的应答 
        public const byte S2MDeliverAck = 0x08;//出货命令的应答
        public const byte S2MDeliverComplete = 0x06;//货道出货成功
        public const byte S2MotorError = 0xB1;//转盘电机故障或者该货道缺货  故障  

        public const byte S2DeliverCup = 0xB6;//出杯子
    }
    public class WriteData
    {
        public byte Cmd;//当前的Cmd
        public byte[] Data;//请求的数据类型 
    }
    private static byte[] result = new byte[1024];
    void Start ()
    {
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
        try 
        { 
            clientSocket.Connect(new IPEndPoint(ip, 8885)); //配置服务器IP与端口 
            Debug.LogError("连接服务器！");
        } 
        catch 
        {
            Debug.LogError("连接服务器失败，请按回车键退出！"); 
            return; 
        }
        try
        {
            StartCoroutine(Sendor());
            //通过clientSocket接收数据 
            int receiveNumber = clientSocket.Receive(result);
            Debug.LogError("receiveNumber:"+ receiveNumber);
            byte[] get = new byte[receiveNumber];
            Array.Copy(result, get, receiveNumber);
            Read(get);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            return;
        }
            
    }

    private IEnumerator Sendor()
    {
        yield return new WaitForSeconds(6f);

    }


    private  byte[] _writeBuf = new byte[128];
    private  byte _cmdIndex = 0;

    public  void Send(Socket socket, WriteData data, bool reliable = false)
    {
        if (data == null) return;
        int idx = 0;
        byte len = (byte)(3 + (data.Data == null ? 0 : data.Data.Length));
        _writeBuf[idx++] = 0xAA;

        _writeBuf[idx++] = len;
        _writeBuf[idx++] = data.Cmd;
        _writeBuf[idx++] = _cmdIndex;
        byte check = (byte)(len ^ data.Cmd ^ _cmdIndex);
        if (data.Data != null)
        {
            for (int i = 0; i < data.Data.Length; i++)
            {
                _writeBuf[idx++] = data.Data[i];
                check ^= data.Data[i];
            }
        }
        _writeBuf[idx++] = check;
        _writeBuf[idx++] = 0x55;

        socket.Send(_writeBuf);
    
        _cmdIndex++;

        byte cmd = _writeBuf[2];
        if (cmd == Cmd.M2SDeliverRequest)
        {
            Debug.LogError("应答的握手指令");
        }

    }

    //接收数据
    public  void Read(byte[] bytes)
    {
        Debug.LogError("接收服务器消息:" + bytes.Length);
        byte cmd = bytes[2];
        Debug.LogError("cmd:" + cmd);

        if (cmd == Cmd.M2SQueryLink)//应答的握手指令
        {
            Debug.LogError("应答的握手指令");
        }

    }

    public  void SayHello(Socket socket)
    {
        WriteData data = new WriteData() { Cmd = Cmd.M2SQueryLink, Data = null };
        Send(socket, data);
    }
}
