﻿using RiscVSim.Environment.Decoder;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.Environment.Opcode
{
    public class OpCode05 : OpCodeCommand
    {


        public OpCode05(IMemory memory, Register register) : base(memory,register)
        {

        }

        public override int Opcode => 05;

        public override void Execute(Instruction instruction, InstructionPayload payload)
        {
            // AUIPC (add upper immediate to pc) is used to build pc-relative addresses and uses the U-type format.AUIPC forms a 32 - bit offset from the 20 - bit U - immediate,
            // filling in the lowest 12 bits with zeros, adds this offset to the address of the AUIPC instruction, then places the result in register rd.

            uint workingBuffer;
            int rd = payload.Rd;
            var unsingedImmedaite = payload.UnsignedImmediate;
            var address = Register.ReadUnsignedInt(Register.ProgramCounter);

            workingBuffer = unsingedImmedaite;
            workingBuffer <<= 12;

            workingBuffer += address;

            Register.WriteUnsignedInt(rd, workingBuffer);
        }
    }
}