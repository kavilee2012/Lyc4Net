using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

///--------------------
/// 建立时间：2013-10-31
/// 修改时间：
/// 作者：Lucifercx
/// -------------------
/// 
namespace HTTP_SEND_CENTER
{
    /// <summary>
    /// 阻塞队列,版本2
    /// 队列空时阻塞消费者
    /// 队列满时阻塞生产者
    /// </summary>
    /// <typeparam name="Type">队列的数据结构</typeparam>
    class BlockQueue<Type>
    {
        private Queue<Type> m_queue = new Queue<Type>();
        private Semaphore m_nBusy, m_nIdle;
        private object m_inLock = new object(), m_outLock = new object();
        /// <summary>
        /// 队列长度最大值
        /// </summary>
        public readonly int maxcount;

        /// <summary>
        /// 初始化队列
        /// </summary>
        /// <param name="maxCount">队列长度最大值</param>
        public BlockQueue(int maxCount)
        {
            m_nBusy = new Semaphore(0, maxCount);
            m_nIdle = new Semaphore(maxCount, maxCount);
            maxcount = maxCount;
        }

        /// <summary>
        /// 压入队列
        /// </summary>
        /// <param name="item">入队数据结构</param>
        public void Enqueue(Type item)
        {
            lock (m_inLock)
            {
                m_nIdle.WaitOne();
                m_queue.Enqueue(item);
                m_nBusy.Release();
            }
        }

        /// <summary>
        /// 出队列
        /// </summary>
        /// <returns>最早进入队列的数据结构</returns>
        public Type Dequeue()
        {
            lock (m_outLock)
            {
                m_nBusy.WaitOne();
                Type item = m_queue.Dequeue();
                m_nIdle.Release();
                return item;
            }
        }

        /// <summary>
        /// 当前队列数
        /// </summary>
        public int Count
        {
            get { return m_queue.Count; }
        }

        /// <summary>
        /// 释放队列
        /// </summary>
        ~BlockQueue()
        {
            m_inLock = null;
            m_outLock = null;
            m_nBusy.Close();
            m_nIdle.Close();
            m_queue.Clear();
            m_queue = null;
        }




        /*
         public BlockQueue<XMLSMSData> smsqueue = new BlockQueue<XMLSMSData>(20000);
         XMLSMSData 是信息体，自己定义一个就可以了
         XMLSMSData temp = ReadXML.ReadSms(strReq);
         queue.Enqueue(temp);
         这里是压队列，读XML的自己变化一下
         XMLSMSData rs = smsqueue.Dequeue();
         取队列
         /// <summary>
        /// XML数据流结构体(短信类)
        /// </summary>
        public class XMLSMSData
        {
            public string datatime;
            public string transactionId;
            public string spId;
            public string serviceId;
            public string serviceType;
            public string feeType;
            public string itemId;
            public string ua;
            public string key;
            public string value;

            public XMLSMSData()
            {
                datatime = "";
                transactionId = "";
                spId = "";
                serviceId = "";
                spId = "";
                serviceType = "";
                feeType = "";
                itemId = "";
                ua = "";
                key = "";
                value = "";
            }
        }
         * 
         * 
         * 我们一般是20条线程同时处理，缓冲是20000个数据，在高峰能跑每秒15条数据
         * 系统是3台服务器，1台应用服务器负责接收，1台数据处理，1台ORACLE
         * 我们使用作业，每天自动备份到数据库服务器，每月通过FTP下载备份数据
         * 一般大流量是CENTOS+ORACLE，小流量才用SQL SERVER
         * 不是混用，单位的业务多，为了保证每条业务线的独立性，会根据评估，看是独立用ORACLE的数据库，还是合用SQL SERVER（减少服务器，减少开支）
         * 数据库一般只做2类操作，INSERT和SELELCT，禁止使用UPDATE，DELETE只能手工使用
       
         
         */
    }
}
