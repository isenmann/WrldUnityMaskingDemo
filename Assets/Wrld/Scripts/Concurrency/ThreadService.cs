using System;
using System.Runtime.InteropServices;
using AOT;
using System.Threading;
using System.Collections.Generic;

namespace Wrld.Concurrency
{
    internal class ThreadService
    {
        private static ThreadService ms_instance;
        private Dictionary<int, Thread> m_threads = new Dictionary<int,  Thread>();
        private int m_nextThreadID;
        private const int InvalidThreadID = -1;

        internal delegate IntPtr ThreadStartDelegate(IntPtr startData);

        internal delegate int CreateThreadDelegate(ThreadStartDelegate runFunc, IntPtr startData);

        internal delegate void JoinThreadDelegate(int threadHandle);

        internal ThreadService()
        {
            ms_instance = this;
        }

        [MonoPInvokeCallback(typeof(CreateThreadDelegate))]
        static internal int CreateThread(ThreadStartDelegate runFunc, IntPtr startData)
        {
            return ms_instance.CreateThreadInternal(runFunc, startData);
        }

        private int CreateThreadInternal(ThreadStartDelegate runFunc, IntPtr startData)
        {
            int threadID;
            Thread thread;
            
            lock (m_threads)
            {
                threadID = GenerateThreadID();
                thread = new Thread(new ParameterizedThreadStart(start => runFunc((IntPtr)start)));
                m_threads[threadID] = thread;
            }

            thread.Start(startData);

            return threadID;
        }

        [MonoPInvokeCallback(typeof(JoinThreadDelegate))]
        static internal void JoinThread(int threadID)
        {
            ms_instance.JoinThreadInternal(threadID);
        }

        private void JoinThreadInternal(int threadID)
        {
            Thread thread;

            lock (m_threads)
            {
                thread = m_threads[threadID];
                m_threads.Remove(threadID);
            }

            thread.Join();
        }

        private int GenerateThreadID()
        {
            int threadID;

            do
            {
                threadID = m_nextThreadID++;
            }
            while(m_threads.ContainsKey(threadID) || threadID == InvalidThreadID);

            return threadID;
        }
    }
}

