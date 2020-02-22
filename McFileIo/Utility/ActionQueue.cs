using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace McFileIo.Utility
{
    public class ActionQueue<T>
    {
        private readonly T _baseObj;
        private readonly ConcurrentQueue<(Action<T, object> Action, object Args, string Source)> _actionQueue 
            = new ConcurrentQueue<(Action<T, object>, object, string)>();

        public bool TrackElapsedTime = false;

        private Dictionary<string, long> _trackingTime = new Dictionary<string, long>();
        private Dictionary<string, int> _trackingCnt = new Dictionary<string, int>();

        public ActionQueue(T baseObject)
        {
            _baseObj = baseObject;
        }

        public void ClearTrackingData()
        {
            _trackingCnt.Clear();
            _trackingTime.Clear();
        }

        public void EnqueueClearTrackingData()
        {
            Enqueue((x, y) => ClearTrackingData());
        }

        public bool TryGetTrackingData(string source, out (int Count, long TotalTime) result)
        {
            if(_trackingCnt.TryGetValue(source, out var cnt))
            {
                result = (cnt, _trackingTime[source]);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public void Enqueue(Action<T, object> procedure, object args = null, string source = null)
        {
            _actionQueue.Enqueue((procedure, args, source));
        }

        public int Perform()
        {
            int cnt = 0;

            while (_actionQueue.TryDequeue(out var result))
            {
                result.Action(_baseObj, result.Args);
                ++cnt;
            }

            return cnt;
        }

        public int Perform(int maxCount)
        {
            int cnt = 0;

            while (_actionQueue.TryDequeue(out var result))
            {
                result.Action(_baseObj, result.Args);
                ++cnt;

                if (cnt >= maxCount)
                    break;
            }

            return cnt;
        }

        public int PerformTimelimited(long milliSeconds)
        {
            var sw = new Stopwatch();

            sw.Start();

            var cnt = 0;
            long currentTime = 0;
            long currentTime2 = 0;

            while (_actionQueue.TryDequeue(out var result))
            {
                currentTime = sw.ElapsedMilliseconds;

                result.Action(_baseObj, result.Args);
                ++cnt;

                currentTime2 = sw.ElapsedMilliseconds;

                if (TrackElapsedTime && result.Source != null)
                {
                    unchecked
                    {
                        if (_trackingCnt.TryGetValue(result.Source, out var localCnt))
                        {
                            _trackingCnt[result.Source] = localCnt + 1;
                            _trackingTime[result.Source] += currentTime2 - currentTime;
                        }
                        else
                        {
                            _trackingCnt[result.Source] = 1;
                            _trackingTime[result.Source] = currentTime2 - currentTime;
                        }
                    }
                }

                if (currentTime2 > milliSeconds)
                    break;
            }

            sw.Stop();

            return cnt;
        }
    }
}
