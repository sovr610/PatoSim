﻿using RiscVSim.Environment;
using RiscVSim.Environment.Decoder;
using RiscVSim.Environment.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiscVSim.OpCodes.RV64I
{
    public class OpCode64Id00 : OpCodeCommand
    {

        /*
         *  lb      rd rs1       imm12 14..12=0 6..2=0x00 1..0=3
            lh      rd rs1       imm12 14..12=1 6..2=0x00 1..0=3
            lw      rd rs1       imm12 14..12=2 6..2=0x00 1..0=3
            ld      rd rs1       imm12 14..12=3 6..2=0x00 1..0=3
            lbu     rd rs1       imm12 14..12=4 6..2=0x00 1..0=3
            lhu     rd rs1       imm12 14..12=5 6..2=0x00 1..0=3
            lwu     rd rs1       imm12 14..12=6 6..2=0x00 1..0=3
         * 
         * 
         */

        private const int lb = 0;
        private const int lh = 1;
        private const int lw = 2;
        private const int ld = 3;
        private const int lbu = 4;
        private const int lhu = 5;
        private const int lwu = 6;

        public OpCode64Id00 (IMemory memory, IRegister register) : base(memory, register)
        {

        }

        public override int Opcode => 0x00;

        public override bool Execute(Instruction instruction, InstructionPayload payload)
        {
            var rd = payload.Rd;
            var rs1 = payload.Rs1;
            var signedImmediate = payload.SignedImmediate;
            var rs1Value = Register.ReadUnsignedInt(rs1);

            Logger.Info("Opcode00 : rd = {rd}, rs1 = {rs1}", rd, rs1);

            IEnumerable<byte> buffer;
            byte[] result;

            var memoryAddress = MathHelper.Add(rs1Value, signedImmediate);
            switch (payload.Funct3)
            {
                case lh:
                    // LH loads a 16-bit value from memory, then sign-extends to 32 - bits before storing in rd.
                    buffer = Memory.GetHalfWord(memoryAddress);
                    result = MathHelper.PrepareLoad(buffer.ToArray(), 8, true);
                    break;

                case lhu:
                    // LHU loads a 16-bit value from memory but then zero extends to 32 - bits before storing in rd.
                    buffer = Memory.GetHalfWord(memoryAddress);
                    result = MathHelper.PrepareLoad(buffer.ToArray(), 8, true);
                    break;

                case lb:
                    buffer = Memory.GetByte(memoryAddress);
                    result = MathHelper.PrepareLoad(buffer.ToArray(), 8, true);
                    break;

                case lbu:
                    buffer = Memory.GetByte(memoryAddress);
                    result = MathHelper.PrepareLoad(buffer.ToArray(), 8, true);
                    break;

                case lw:
                    buffer = Memory.GetWord(memoryAddress);
                    result = MathHelper.PrepareLoad(buffer.ToArray(), 8, true);
                    break;

                case lwu:
                    buffer = Memory.GetWord(memoryAddress);
                    result = MathHelper.PrepareLoad(buffer.ToArray(), 8, false);
                    break;

                case ld:
                    result = Memory.GetDoubleWord(memoryAddress).ToArray();
                    break;

                default:
                    throw new OpCodeNotSupportedException(String.Format("OpCode = {0}, Funct3 = {1}", instruction.OpCode, payload.Funct3));
            }

            Register.WriteBlock(rd, result);

            return true;
        }
    }
}
