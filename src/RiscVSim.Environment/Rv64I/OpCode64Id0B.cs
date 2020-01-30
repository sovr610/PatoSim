﻿using RiscVSim.Environment.Decoder;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.Environment.Rv64I
{
    public class OpCode64Id0B : OpCodeCommand
    {
        public OpCode64Id0B(IMemory memory, IRegister register) : base(memory,register)
        {

        }

        public override int Opcode => 0x0B;

        public override bool Execute(Instruction instruction, InstructionPayload payload)
        {
            /*
             *  # RV64A
                amoadd.d    rd rs1 rs2      aqrl 31..29=0 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amoxor.d    rd rs1 rs2      aqrl 31..29=1 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amoor.d     rd rs1 rs2      aqrl 31..29=2 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amoand.d    rd rs1 rs2      aqrl 31..29=3 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amomin.d    rd rs1 rs2      aqrl 31..29=4 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amomax.d    rd rs1 rs2      aqrl 31..29=5 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amominu.d   rd rs1 rs2      aqrl 31..29=6 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amomaxu.d   rd rs1 rs2      aqrl 31..29=7 28..27=0 14..12=3 6..2=0x0B 1..0=3
                amoswap.d   rd rs1 rs2      aqrl 31..29=0 28..27=1 14..12=3 6..2=0x0B 1..0=3
                lr.d        rd rs1 24..20=0 aqrl 31..29=0 28..27=2 14..12=3 6..2=0x0B 1..0=3
                sc.d        rd rs1 rs2      aqrl 31..29=0 28..27=3 14..12=3 6..2=0x0B 1..0=3
             */

            return true;
        }
    }
}
