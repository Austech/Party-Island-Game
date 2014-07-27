﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// Encodable States are States that can be turned into arrays of data, and formed from arrays of data
    /// </summary>
    public interface IStateEncodable
    {
        byte[] Encode();
        void Decode(byte[] data);
    }
}
