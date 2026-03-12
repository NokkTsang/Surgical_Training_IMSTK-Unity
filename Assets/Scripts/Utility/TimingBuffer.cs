/*=========================================================================

   Library: iMSTK-Unity

   Copyright (c) Kitware, Inc. 

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0.txt

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

=========================================================================*/

namespace ImstkUnity
{
    /// <summary>
    /// Class to gather information about runtime behavior, mirrors the 
    /// pattern of the Unity Profilermarker, but gives access to it's data
    /// for in game display. Use Begin() and End() to mark a section of interest
    /// after End() the timing value is automatically pushed to the buffer
    /// </summary>
    public class TimingBuffer
    {
        private AccumulatingBuffer _buffer;
        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        public float AverageTimeMs
        {
            get { return _buffer.Average * 1000; }
        }

        public float AverageTimeSeconds
        {
            get { return _buffer.Average; }
        }

        public float AverageRateSeconds
        {
            get { return 1.0f / AverageTimeSeconds; }
        }

        public TimingBuffer(int size)
        {
            _buffer = new AccumulatingBuffer(size);
        }

        public void Begin()
        {
            _stopwatch.Restart();
        }

        public void End()
        {
            _stopwatch.Stop();
            _buffer.Push((float)_stopwatch.Elapsed.TotalSeconds);
        }

    }
}