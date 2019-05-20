//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor.MemoryProfiler;
//using UnityEngine;

//public class Send : MonoBehaviour{

/**
//* 封包
//* **/
//   private void send(Message vo)
//   {

//       if (vo == null) return;
//       MemoryStream buff = new MemoryStream();
//       byte[] voByte = Message.encode(vo);
//       byte[] pLen = ByteArrayUtil.writeInt(voByte.Length);
//       buff.Write(pLen, 0, 4);//写包头
//       buff.Write(voByte, 0, voByte.Length);
//       _socket.Send(buff.ToArray());
//   }
//   //开一个子线程接受数据,Socket的粘包拆包
//   class SocketThread
//   {
//       public Connection conn;

//       private int len = 0;
//       public void run()
//       {
//           while (true)
//           {
//               if (conn.state != Connection.CONNECTED) continue;
//               if (len == 0)
//               {
//                   if (conn._socket.Available < 4) continue;//不足4字节读包长
//                   byte[] lenB = new byte[4];
//                   conn._socket.Receive(lenB, 4, SocketFlags.None);
//                   len = ByteArrayUtil.readInt(lenB);//读出包长
//               }
//               if (conn._socket.Available < len) continue;//不足一个包

//               byte[] voB = new byte[len];
//               conn._socket.Receive(voB, len, SocketFlags.None);//把包读出来

//               len = 0;

//               MemoryStream ms = new MemoryStream(voB);
//               Message vo = Message.decode(ms);//解码包
//               ms.Close();
//               Debug.Log("receive:" + vo.getClassName());
//               conn.resultConnectionHandler(vo);
//           }
//       }
//   }