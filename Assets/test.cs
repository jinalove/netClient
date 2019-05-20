using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class test : MonoBehaviour {

    // Use this for initialization
    byte[] bytes = new byte[1024];
    private static byte[] result = new byte[1024];
    private List<byte>  buffer = new List<byte>();

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
          //  return;
        }
        try
        {
            //  StartCoroutine(Sendor());

            // int receiveNumber = clientSocket.Receive(result);
            // Debug.LogError("receiveNumber:" + receiveNumber);
            //byte[] get = new byte[receiveNumber];
            //Array.Copy(result, get, receiveNumber);

            StartCoroutine(Recycle(clientSocket));
            StartCoroutine(Handle());


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            return;
        }
    }

    private IEnumerator Recycle(Socket clientSocket)
    {
        while (true)
        {
            if (clientSocket.Available > 0)
            {
                byte[] collect = new byte[clientSocket.Available];
                clientSocket.Receive(collect);
                buffer.AddRange(collect);
                yield return new WaitForSeconds(0.2f);
                continue;//不足4字节读包长
            }
            yield return new WaitForSeconds(0.2f);

        }

    }

    private IEnumerator Handle()
    {
        while (true)
        {
            if (buffer.Count > 0)
            {
                Debug.Log("有数据进来了!!!!!!");
                int index = 0;
               
                if (buffer[index++] == 0xAA)
                {
                    if (buffer.Count < (4 + 1))
                    {
                        yield return new WaitForSeconds(0.2f);
                        Debug.LogError("包长不足:" + buffer.Count);
                        continue;//不足4字节读包长
                    }

                    byte[] lenArray = new byte[4];

                    for (int i = 0; i < 4; i++)
                    {
                        lenArray[i] = buffer[index++];
                    }
                    int _cmdIndex = index;

                    int length = BitConverter.ToInt32(lenArray, 0);
                    Debug.Log("数据长度:"+ length);
                    index += length;   //index 等于final位

                    if (buffer.Count <= index)
                    {
                        yield return new WaitForSeconds(0.2f);
                        Debug.LogError("数据长度:" + buffer.Count);
                        continue;//buffer.Count不足数据长度
                    }
                    Debug.LogError("buffer.Count:" + buffer.Count);

                    byte Cmd = buffer[_cmdIndex++];
                    byte CmdIndex = buffer[_cmdIndex++];
                    Debug.LogError("Cmd:" + Cmd);
                    Debug.LogError("CmdIndex:" + CmdIndex);

                    byte check = (byte)(Cmd ^ CmdIndex);
                    for (int i = 0; i < 4; i++)
                    {
                        check ^= lenArray[i];
                    }

                    int dataStartIndex = _cmdIndex;   //dataStartIndex 等于正式数据位
                    int datalength = length - 3;        //在协议里面规定: length 等于 (3 + data.Data.Length)
                    byte[] data = new byte[datalength];

                    for (int i = 0; i < datalength; i++)
                    {
                        data[i] = buffer[dataStartIndex + i];
                        check ^= data[i];
                    }
                    Debug.LogError("check:"+ check);

                    if (buffer[index] == 0x55 && check == buffer[index - 1])
                    {
                        // head + len+ cmd + length
                        int num = 1 + 4 + 1 + length;
                        buffer.RemoveRange(0, num);
                        Debug.Log("final buffer.Count:" + buffer.Count);
                        Debug.LogError("数据校验合格----------------------------------------");
                        continue;
                    }
                    else
                    {
                        Debug.LogError("数据校验失败");
                        buffer.RemoveAt(0);
                        continue;
                    }
                }
                else
                {
                    Debug.LogError("过滤数据!!:"+ buffer[0]);
                    buffer.RemoveAt(0);
                    continue;
                }
            }

           // Debug.Log(" WaitForSeconds(0.2f)");
            yield return new WaitForSeconds(0.2f);
        }

    }

    private IEnumerator Sendor(Socket clientSocket =null)
    {
        yield return new WaitForSeconds(0f);
        //  PersonHandler person = new PersonHandler { cmd = 2, age = 18, name = "OK" };
        Person person = new Person { cmd = 2, age = 18, name = "OK" };
        bytes = SerializeHelper.SerializeToBinary(person);

        MemoryStream buff = new MemoryStream();
        byte[] pLen = BitConverter.GetBytes(bytes.Length); 

        buff.Write(pLen, 0, 4);//写包头
        buff.Write(bytes, 0, bytes.Length);

        //Debug.LogError("bytes.Length:"+bytes.Length);

        //clientSocket.Send(buff.ToArray());
        Read(buff.ToArray());


    }

    //接收数据
    public void Read(byte[] bytes)
    {


        Debug.LogError("DeserializeWithBinary:"+ bytes.Length);

        // BaseHandler handler = SerializeHelper.DeserializeWithBinary<BaseHandler>(bytes);
        //foreach (var item in bytes)
        //{
        //    Debug.LogError(item);
        //}
        CmdBase handler = SerializeHelper.DeserializeWithBinary<CmdBase>(bytes);

        //if (handler != null)
        //{
        //    handler.Deserialize(bytes);
        //}
        //else
        //{
        //    Debug.LogError("接收数据为空");
        //}
        //Debug.LogError(handler.cmd);
        Debug.LogError("接收数据");
        if (handler != null)
        {
            Debug.LogError(handler.cmd);
        }
        else
        {
            Debug.LogError("接收数据为空");
        }
    }
}


[Serializable]
public class PersonHandler:BaseHandler
{
    public int age;
    public string name;

    public override void Deserialize(byte[] bytes)
    {
        PersonHandler person = new PersonHandler();
        person = SerializeHelper.DeserializeWithBinary<PersonHandler>(bytes);
        Debug.LogError(person.name);
        Debug.LogError(person.age);
    }
}

[Serializable]
public class CoffeeHandler : BaseHandler
{
    public int age;
    public string name;

    public override void Deserialize(byte[] bytes)
    {
        PersonHandler person = new PersonHandler();
        person = SerializeHelper.DeserializeWithBinary<PersonHandler>(bytes);
        Debug.LogError(person.name);
        Debug.LogError(person.age);
    }
}

[Serializable]
public class Person : CmdBase
{
    public int age;
    public string name;

}
[Serializable]
public class CmdBase
{
    public int cmd;
}


[Serializable]
public class BaseHandler
{
    public int cmd;
    public virtual void Deserialize(byte[] bytes)
    {

    }

}