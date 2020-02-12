﻿using RiscVSim.Environment.Decoder;
using RiscVSim.Environment.Exception;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiscVSim.Environment.Rv32I
{
    public class RvcComposer : IRvcComposer
    {
        public RvcComposer()
        {

        }
        public Instruction ComposeInstruction(RvcPayload payload)
        {
            int? opCode = null;

            // Q00
            // 000 C.ADDI4SPN OpCode 04
            // 001 C.FLD Not supported
            // 010 C.LW OpCode 00
            // 011 C.FLW Not Supported
            // 101 C.FSD Not Supported
            // 110 C.SW OpCode 08
            // 111 C.FSW Not Supported
            if (payload.Op == 0x00)
            {
                switch (payload.Funct3)
                {
                    // C.ADDI4SPN
                    case 0:
                        opCode = 04;
                        break;

                    // C.LW
                    case 2:
                        opCode = 00;
                        break;

                    // C.SW
                    case 6:
                        opCode = 08;
                        break;

                    default:
                        string message = string.Format("RVC Opcode {0:X} and F3 {1:X} is not supported",payload.Op,payload.Funct3);
                        throw new OpCodeNotSupportedException(message);
                }
            }


            // Q01
            // 000 C.NOP / C.ADDI OpCode 04
            // 001 C.JAL
            // 010 C.LI
            // 011 C.ADDI16SP (RD=2)
            // 011 C.LUI (RD != 2)
            // 100 x 00 C.SRLI
            // 100 x 01 C.SRAI
            // 100 x 10 C.ANDI
            // 100 x 11 C.SUB, C.XOR, C.OR, C.AND
            // 101 C.J
            // 110 C.BEQZ
            // 111 C.BNEZ
            if (payload.Op == 01)
            {
                switch (payload.Funct3)
                {
                    // C.NOP / C.ADDI
                    case 0:
                        opCode = 00;
                        break;

                    // C.JAL
                    case 1:
                        opCode = 0x1B;
                        break;

                    // C.LI
                    case 2:
                        opCode = 0x04;
                        break;

                    // C.LUI
                    // C.ADDI16SP
                    case 3:
                        opCode = 0x04;
                        break;

                    // C.SRLI, C.SRAI, ...
                    case 4:
                        opCode = 0x04;
                        break;

                    // C.J
                    case 5:
                        opCode = 0x1B;
                        break;

                    // C.BEQZ
                    case 6:
                        opCode = 0x18;
                        break;

                    // C.BNEZ
                    case 7:
                        opCode = 0x18;
                        break;

                    default:
                        string message = string.Format("RVC Opcode {0:X} and F3 {1:X} is not supported", payload.Op, payload.Funct3);
                        throw new OpCodeNotSupportedException(message);
                }
            }

            // Q02
            // 000 C.SLLI
            // 001 C.FLDSP
            // 010 C.LWSP
            // 011 C.FLWSP
            // 100 0 C.JR / C.MV
            // 100 1 C.EBREAK / C.JALR / C.ADD
            // 101 C.FSDSP
            // 110 C.SWSP
            // 111 C.FSWSP

            if (payload.Op == 3 )
            {
                switch (payload.Funct3)
                {
                    // C.SLLI
                    case 0:
                        opCode = 0x04;
                        break;

                    // C.LWSP
                    case 2:
                        opCode = 0x00;
                        break;

                    // C.JR (Jalr)
                    // C.MV (ADD rd,x0,rs2)
                    // C.EBREAK 
                    // C.JALR (Jalr)
                    // C.ADD
                    case 4:
                        var isJr = (payload.Funct4 == 0x08) && (payload.Rs2 == 0);
                        var isMv = (payload.Funct4 == 0x08) && (payload.Rd != 0) && (payload.Rs2 != 0);
                        var isEBreak = (payload.Funct4 == 0x09) && (payload.Rd == 0) && (payload.Rs2 == 0);
                        var isJalr = (payload.Funct4 == 0x09) && (payload.Rs1 != 0) && (payload.Rs2 == 0);
                        var isAdd = (payload.Funct4 == 0x09) && (payload.Rs1 != 0) && (payload.Rs2 != 0);

                        if (isJr)
                        {
                            opCode = 0x19;
                        }

                        if (isMv)
                        {
                            opCode = 0x0C;
                        }

                        if (isEBreak)
                        {
                            opCode = 0x1C;
                        }

                        if (isJalr)
                        {
                            opCode = 0x19;
                        }

                        if (isAdd)
                        {
                            opCode = 0x0C;
                        }


                        if (!opCode.HasValue)
                        {
                            throw new RvcFormatException("Invalid coding detected for Q02, F3 100");
                        }
                        break;

                    // C.SWSP
                    case 6:
                        opCode = 0x08;
                        break;

                    default:
                        string message = string.Format("RVC Opcode {0:X} and F3 {1:X} is not supported", payload.Op, payload.Funct3);
                        throw new OpCodeNotSupportedException(message);
                }
            }

            if (!opCode.HasValue)
            {
                throw new RiscVSimException("Invalid RVC Opcode detected");
            }

            
            var instruction = new Instruction(payload.Type, opCode.Value, 2);
            return instruction;
        }

        public InstructionPayload ComposePayload(Instruction ins, RvcPayload payload)
        {
            //InstructionPayload p = new InstructionPayload(ins, null);

            //return p;



            return null;
        }
    }
}
