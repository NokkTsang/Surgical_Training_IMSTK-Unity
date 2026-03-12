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
    /// Class to maintain a simple ring buffer for accumulating data, the buffer 
    /// maintains the sum of the data as well for fast average calculation.
    /// The size can be determined at initialization.
    /// Used to keep track of timing and performance data outside of the profiler
    /// </summary>
    public class AccumulatingBuffer
    {
        private float[] _data;
        private int _length;
        private int _current = 0;
        private float _sum = 0;

        public int Length
        {
            get { return _length; }
        }

        public float Average
        {
            get { return _sum / _length; }
        }

        public AccumulatingBuffer(int size)
        {
            _data = new float[size];
            for (int i = 0; i < _length; ++i)
            {
                _data[i] = 0;
            }
            _length = size;
        }

        public void Push(float val)
        {
            _sum -= _data[_current];
            _data[_current] = val;
            _sum += _data[_current];
            _current = (_current + 1) % _length;
        }
    }
}