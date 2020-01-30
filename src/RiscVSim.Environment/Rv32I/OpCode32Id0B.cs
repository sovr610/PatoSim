﻿using RiscVSim.Environment.Decoder;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.Environment.Rv32I
{
    public class OpCode32Id0B : OpCodeCommand
    {
        public OpCode32Id0B(IMemory memory, IRegister register) : base(memory,register)
        {

        }

        public override int Opcode => 0x0B;

        public override bool Execute(Instruction instruction, InstructionPayload payload)
        {
            /*
             *  # RV32A
                amoadd.w    rd rs1 rs2      aqrl 31..29=0 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amoxor.w    rd rs1 rs2      aqrl 31..29=1 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amoor.w     rd rs1 rs2      aqrl 31..29=2 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amoand.w    rd rs1 rs2      aqrl 31..29=3 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amomin.w    rd rs1 rs2      aqrl 31..29=4 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amomax.w    rd rs1 rs2      aqrl 31..29=5 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amominu.w   rd rs1 rs2      aqrl 31..29=6 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amomaxu.w   rd rs1 rs2      aqrl 31..29=7 28..27=0 14..12=2 6..2=0x0B 1..0=3
                amoswap.w   rd rs1 rs2      aqrl 31..29=0 28..27=1 14..12=2 6..2=0x0B 1..0=3
                lr.w        rd rs1 24..20=0 aqrl 31..29=0 28..27=2 14..12=2 6..2=0x0B 1..0=3
                sc.w        rd rs1 rs2      aqrl 31..29=0 28..27=3 14..12=2 6..2=0x0B 1..0=3
             */

            return true;
        }
    }
}
