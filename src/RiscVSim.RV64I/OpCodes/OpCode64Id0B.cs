﻿using RiscVSim.Environment;
using RiscVSim.Environment.Decoder;
using RiscVSim.Environment.Exception;
using RiscVSim.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.OpCodes.RV64I
{
    public class OpCode64Id0B : OpCodeCommand
    {
        private AtomicInstruction atomic;

        public OpCode64Id0B(IMemory memory, IRegister register) : base(memory,register)
        {
            atomic = new AtomicInstruction(memory, register);
        }

        public override int Opcode => 0x0B;

        public override bool Execute(Instruction instruction, InstructionPayload payload)
        {
            var rs1 = payload.Rs1;
            var rs2 = payload.Rs2;
            var rd = payload.Rd;
            var f3 = payload.Rd;

            // F7 pattern
            var f7 = payload.Funct7;
            var f5 = f7 >> 2;           // bits 31 ... 27
            var aq = (f7 & 0x02) >> 1;  // acquire bit 26
            var rl = f7 & 0x01;         // release bit 25

            Logger.Info("OpCode 0B : rd = {rd}, rs1 = {rs1}, rs2 = {rs2}, funct3 = {f3}, aq = {aq}, rl = {rl}, f5 = {f5}", rd, rs1, rs2, f3, aq, rl, f5);

            // f3 = 2 => RV32I  W operations
            // f3 = 3 => RV64I  D operations

            if (f3 == 2)
            {
                atomic.ExecuteW(rd, rs1, rs2, rl, aq, f5);
            }
            else if (f3 == 3)
            {
                atomic.ExecuteD(rd, rs1, rs2, rl, aq, f5);
            }
            else
            {
                string message = String.Format("Invalid coding (fe={0:X} detected for the F3 coding in the A-Extension", f3);
                Logger.Error(message);
                throw new RiscVSimException(message);
            }
            

            return true;
        }
    }
}
