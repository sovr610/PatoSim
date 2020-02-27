﻿using RiscVSim.Environment.Decoder;
using RiscVSim.Environment.Exception;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.Environment.Rv32I
{
    /// <summary>
    /// Implements the JALR (Jump and Link Register) opcode
    /// </summary>
    public class OpCode32Id19 : OpCodeCommand
    {

        public OpCode32Id19 (IMemory memory, IRegister register) : base(memory, register)
        {
         }

        public override int Opcode => 0x19;

        public override bool Execute(Instruction instruction, InstructionPayload payload)
        {
            var rd = payload.Rd;
            var rs1 = payload.Rs1;

            // Get the conditions for the Push and Pop operations
            var rdLink = (rd == 1) || (rd == 5);
            var rs1Link = (rs1 == 1) || (rs1 == 5);

            Logger.Info("Opcode 19 : rd = {rd}, rs1 = {rs1}, Immediate = {imm}", rd, rs1, payload.SignedImmediate);

            // A simple jump
            if (!rdLink && !rs1Link)
            {
                SimpleJump(instruction,payload);
            }
            else
            {
                uint address = 0;
                if (rdLink && rs1Link)
                {
                    if (rd == rs1)
                    {
                        // Push operation

                        // Assumption: rd is a valid link register and I have to update it!
                        // TODO: Review this
                        address = CalculateAddress(payload);                      

                    }
                    else
                    {
                        // pop then push

                        // pop then push
                        address = CalculateAddress(payload);
                        Register.WriteUnsignedInt(rd, address);
                    }
                }
                else
                {
                    // pop
                    if (!rdLink && rs1Link)
                    {
                        address = CalculateAddress(payload);
                        Register.WriteUnsignedInt(rs1, address);
                    }

                    // push
                    if (rdLink && !rs1Link)
                    {
                        // Assumption:  rs1 is not a link register (x1 or x5) and therfore we just do the push operation and jump
                        // TODO: Review this!
                        address = CalculateAddress(payload);


                        var currentAddress = Register.ReadUnsignedInt(Register.ProgramCounter);
                        var instructionLength = Convert.ToUInt32(instruction.InstructionLength);
                        var returnAddress = currentAddress + instructionLength;
                        Register.WriteUnsignedInt(payload.Rd, returnAddress);
                    }
                }

                // Jump
                if (address != 0)
                {
                    Register.WriteUnsignedInt(Register.ProgramCounter, address);
                }
                else
                {
                    throw new EncodingException("Unknown code path detected in RVI32 JALR");
                }


            }

            return false;
        }

        private void SimpleJump(Instruction instruction, InstructionPayload payload)
        {
            // The indirect jump instruction JALR (jump and link register) uses the I-type encoding. The target
            // address is obtained by adding the sign - extended 12 - bit I - immediate to the register rs1, then setting
            // the least - significant bit of the result to zero. The address of the instruction following the jump
            // (pc+4) is written to register rd. Register x0 can be used as the destination if the result is not
            // required.
            var pcIndex = Register.ProgramCounter;
            var address = CalculateAddress(payload);

            if (payload.Rd != 0)
            {
                var currentAddress = Register.ReadUnsignedInt(pcIndex);
                var instructionLength = Convert.ToUInt32(instruction.InstructionLength);
                Register.WriteUnsignedInt(payload.Rd, currentAddress + instructionLength);
            }

            // Jump!
            Register.WriteUnsignedInt(Register.ProgramCounter, address);
        }

        private uint CalculateAddress(InstructionPayload payload)
        {
            uint address;
            var immediate = payload.SignedImmediate;
            int rd = payload.Rd;
            var rs1 = payload.Rs1;
            var rs1Value = Register.ReadUnsignedInt(rs1);

            // Make the address a multiplier by 2 !
            //address = rs1Value + immediate;
            address = MathHelper.Add(rs1Value, immediate);
            var rest = address % 2;
            if (rest == 1)
            {
                address -= 1;
            }

            return address;
        }
    }
}
