﻿using RiscVSim.Environment;
using RiscVSim.Environment.Decoder;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.OpCodes.Rv32I
{
    public class OpCode32Id0D : OpCodeCommand
    {

        public OpCode32Id0D(IMemory memory, IRegister register) : base(memory,register)
        {
            // see base();
        }

        public override int Opcode => 0x0D;

        public override bool Execute(Instruction instruction, InstructionPayload payload)
        {
            // LUI (load upper immediate) is used to build 32-bit constants and uses the U-type format. 
            // LUI places the U - immediate value in the top 20 bits of the destination register rd, filling in the lowest 12 bits with zeros.

            uint workingBuffer;
            int rd = payload.Rd;
            var unsingedImmedaite = payload.UnsignedImmediate;

            Logger.Info("Opcode 0D : rd = {rd}, immediate = {imm}", rd, unsingedImmedaite);

            workingBuffer = unsingedImmedaite;
            workingBuffer <<= 12;

            Register.WriteUnsignedInt(rd, workingBuffer);

            return true;
        }
    }
}
