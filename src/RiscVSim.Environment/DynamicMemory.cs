﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiscVSim.Environment
{
    internal class DynamicMemory : IMemory
    {
        private Architecture architecture;

        private Dictionary<uint, byte> memoryDict;

        internal DynamicMemory(Architecture architecture)
        {
            memoryDict = new Dictionary<uint, byte>();
            this.architecture = architecture;
        }

        #region IMemory implementation

        public Architecture Architecture
        {
            get
            {
                return architecture;
            }
        }

        public IEnumerable<byte> Old_Fetch(uint address)
        {
            var instruction = ReadBlock(address, 4);
            return instruction;
        }

        public IEnumerable<byte> GetWord(uint address)
        {
            var word = ReadBlock(address, 2);
            return word;
        }

        public IEnumerable<byte> GetDoubleWord(uint address)
        {
            var word = ReadBlock(address, 4);
            return word;
        }

        public IEnumerable<byte> Read(uint baseAddress, int count)
        {
            var buffer = ReadBlock(baseAddress, count);
            return buffer;
        }

        public void Write(uint baseAddress, IEnumerable<byte> content)
        {
            WriteBlock(baseAddress, content);
        }

        #endregion


        #region Internal implementation

        private IEnumerable<byte> ReadBlock(uint baseAddress, int count)
        {
            var buffer = new byte[count];

            for (int offset = 0; offset < count; offset++)
            {
                uint curAddress = Convert.ToUInt32(baseAddress + offset);
                buffer[offset] = ReadByte(curAddress);
            }

            return buffer;
        }

        private void WriteBlock (uint baseAddress, IEnumerable<byte> content)
        {
            int length = content.Count();

            for (int offset = 0; offset < length; offset++)
            {
                var curAddress = Convert.ToUInt32(baseAddress + offset);
                WriteByte(curAddress, content.ElementAt(offset));
            }
        }

        private void WriteByte(uint address, byte value)
        {
            // If the the key (the address) already exists, update the value or create a new one otherwise.

            if (memoryDict.ContainsKey(address))
            {
                memoryDict[address] = value;
            }
            else
            {
                memoryDict.Add(address, value);
            }
        }

        private byte ReadByte(uint address)
        {
            // If the memory is "blank" create a new one with zero as default value
            if (!memoryDict.ContainsKey(address))
            {
                memoryDict.Add(address, 0);
            }

            return memoryDict[address];
        }






        #endregion
    }
}