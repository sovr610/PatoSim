﻿using RiscVSim.Environment.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiscVSim.Environment.Decoder
{
    public class RvcDecoder
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private Architecture architecture;

        private bool is64;
        private bool is32;


        public RvcDecoder(Architecture architecture)
        {
            this.architecture = architecture;

            is64 = architecture == Architecture.Rv64I;
            is32 = architecture == Architecture.Rv32I;
        }

        public RvcPayload Decode (IEnumerable<byte> rvcCoding)
        {
            RvcPayload payload = null;

            if (rvcCoding == null)
            {
                throw new ArgumentNullException("rvcCoding");
            }

            // Start the parsing with the the opcode and the F3

            var firstByte = rvcCoding.First();
            var secondByte = rvcCoding.ElementAt(1);
            var opCode = firstByte & 0x03;
            var f3 = secondByte >> 5;

            Logger.Info("RVC = {0}, OpCode = {0:X}, F3 = {1:X}", BitConverter.ToString(rvcCoding.ToArray(), 0), opCode, f3);

            if (opCode==0x00)
            {
                payload = DecodeGroup00(rvcCoding, opCode, f3);
            }

            if (opCode == 0x01)
            {
                payload = DecodeGroup01(rvcCoding, opCode, f3);
            }

            if (opCode == 0x02)
            {
                payload = DecodeGroup10(rvcCoding, opCode, f3);
            }

            // Something went wrong.. raise an exception!
            if (payload == null)
            {
                var rvcError = String.Format("Could not decode coding {0} on {1}", BitConverter.ToString(rvcCoding.ToArray(), 0), architecture);
                Logger.Error(rvcError);
                throw new RvcFormatException(rvcError);

            }

            return payload;
        }


        /*
         *  The formats were designed to keep bits for the two register source specifiers in the same place in all
            instructions, while the destination register field can move. When the full 5-bit destination register
            specifier is present, it is in the same place as in the 32-bit RISC-V encoding. Where immediates
            are sign-extended, the sign-extension is always from bit 12. Immediate fields have been scrambled,
            as in the base specification, to reduce the number of immediate muxes required.

            For many RVC instructions, zero-valued immediates are disallowed and x0 is not a valid 5-bit
            register specifier. These restrictions free up encoding space for other instructions requiring fewer
            operand bits.
         * 
         * 
         *  Format  Meaning                      15 14 13 12 11 10 09 08 07 06 05 04 03 02 01 00
            CR      Register                     funct4      rd/rs1         rs2            op
            CI      Immediate                    funct3   i  rd/rs1         imm            op
            CSS     Stack-relative Store         funct3   i                 rs2            op
            CIW     Wide Immediate               funct3   i                       rd ′     op
            CL      Load                         funct3   i        rs1′     imm   rd ′     op
            CS      Store                        funct3   i        rs1′     imm   rs2′     op
            CA      Arithmetic                   funct6            rd′      f2    rs2 ′    op
                                                                   /rs1′ 
            CB      Branch                       funct3   offset   rs1 ′    offset         op
            CJ      Jump                         funct3   jump target                      op


            Table 16.1: Compressed 16-bit RVC instruction formats.
            RVC Register Number               000 001 010 011 100 101 110 111
            Integer Register Number           x8  x9  x10 x11 x12 x13 x14 x15
            Integer Register ABI Name         s0  s1  a0  a1  a2  a3  a4  a5
            Floating-Point Register Number    f8  f9  f10 f11 f12 f13 f14 f15
            Floating-Point Register ABI Name  fs0 fs1 fa0 fa1 fa2 fa3 fa4 fa5

            Table 16.2: Registers specified by the three-bit rs1 ′, rs2 ′, and rd ′ fields of the CIW, CL, CS, CA,
            and CB formats.
         * 
         */
        // ...........................

        #region Group decoder

        private RvcPayload DecodeGroup00(IEnumerable<byte> rvcCoding, int opcode, int f3)
        {
            RvcPayload payload = null;

            //// RV32I / RV64I ////

            // C.LW (010)
            var isLw = f3 == 0x02;
            if (isLw)
            {
                payload = DecodeCL(rvcCoding);
            }

            // C.SW (110)
            var isSw = f3 == 0x06;
            if (isSw)
            {
                payload = DecodeCS(rvcCoding);
            }

            var isAddi4Spn = f3 == 0x00;
            if (isAddi4Spn)
            {
                payload = DecodeCIW(rvcCoding);
            }

            //// RV32I only ////
            if (is32)
            {
                // C.FLW (011)

                // C.FSW (111)
            }

            //// RV64I only ////
            if (is64)
            {
                // C.LD (011)
                var isLd = f3 == 0x03;
                if (isLd)
                {
                    payload = DecodeCL(rvcCoding);
                }

                var isSd = f3 == 0x07;
                if (isSd)
                {
                    payload = DecodeCS(rvcCoding);
                }

                // C.FLD (001)
                // C.FSD (101)

                
            }

            //// RV128I only ////
            // Not supported...

            // C.LQ (001)
            // C.SQ (101)

            return payload;
        }

        private RvcPayload DecodeGroup01(IEnumerable<byte> rvcCoding, int opcode, int f3)
        {
            RvcPayload payload = null;

            //// RV32I / RV64I ////

            // C.J (101)
            var isJ = f3 == 0x05;
            if (isJ)
            {
                payload = DecodeCJ(rvcCoding);
            }

            // C.BEQZ
            var isBeqz = f3 == 0x06;
            if (isBeqz)
            {
                payload = DecodeCB_Branch(rvcCoding);
            }

            // C.BNEZ
            var isBnez = f3 == 0x07;
            if (isBnez)
            {
                payload = DecodeCB_Branch(rvcCoding);
            }

            // C.LI
            var isLi = f3 == 0x02;
            if (isLi)
            {
                payload = DecodeCI(rvcCoding);
            }

            // C.ADDI16SP
            // C.LUI
            var isLui = f3 == 0x03;
            if (isLui)
            {
                payload = DecodeCI(rvcCoding);
            }

            // C.ADDI
            var isAddi = f3 == 0x00;
            if (isAddi)
            {
                payload = DecodeCI(rvcCoding);
            }

            // Group of different commands started with F3 = 100
            var isf3Eq100 = f3 == 0x04;
            if (isf3Eq100)
            {
                // Bit 11 10 Command
                //      0  0 CB CSRLI
                //      0  1 CB C.SRAI
                //      1  0 CB C.ANDI
                //      1  1 CA Coding C.AND, C.OR ... C.SUBW

                var secondByte = rvcCoding.ElementAt(1);
                var b11b10 = secondByte & 0x0C;
                b11b10 >>= 2;

                if (b11b10 == 0x03)
                {
                    payload = DecodeCA(rvcCoding);
                }
                else
                {
                    payload = DecodeCB_IntegerRegister(rvcCoding);
                }
            }

            //// RV32I only ////
            if (is32)
            {
                // C.JAL (001)
                var isJal = f3 == 0x01;
                if (isJal)
                {
                    payload = DecodeCJ(rvcCoding);
                }


            }

            //// RV64I only ////
            if (is64)
            {
                // C.ADDIW
                var isAddiw = f3 == 0x01;
                if (isAddiw)
                {
                    payload = DecodeCI(rvcCoding);
                }
            }

            //// RV128I only ////
            // Not supported...

            return payload;
        }

        private RvcPayload DecodeGroup10(IEnumerable<byte> rvcCoding, int opcode, int f3)
        {
            RvcPayload payload = null;

            //// RV32I / RV64I ////

            // C.LWSP (010)
            var isLwSp = f3 == 0x02;
            if (isLwSp)
            {
                payload = DecodeCI(rvcCoding);
            }

            // C.SWSP (110)
            var isSwSp = f3 == 0x06;
            if (isSwSp)
            {
                payload = DecodeCSS(rvcCoding);
            }

            // C.FSDSP (101)  (according to the table RV32 and RVC64..)
            // not supported

            // C.JR   (100)
            // C.JALR (100)
            var isF3100 = f3 == 0x04;
            if (isF3100)
            {

                payload = DecodeCR(rvcCoding);
            }

            //// RV32I only ////
            if (is32)
            {
                // C.FLWSP (011)
                // C.FSWSP (111)
                // Not supported

                var isSlli = f3 == 0x00;
                if (isSlli)
                {
                    payload = DecodeCI(rvcCoding);
                }

            }

            //// RV64I only ////
            if (is64)
            {
                // C.LDSP (011)
                var isLdSp = f3 == 0x03;
                if (isLdSp)
                {
                    payload = DecodeCI(rvcCoding);
                }

                // C.SDSP (111)
                var isSdSp = f3 == 0x07;
                if (isSdSp)
                {
                    payload = DecodeCSS(rvcCoding);
                }

                // C.FLDSP (011)
                // Not supported

                var isSlli = f3 == 0x00;
                if (isSlli)
                {
                    payload = DecodeCI(rvcCoding);
                }
            }

            //// RV128I only ////
            
            // C.LQSP (001)
            // C.SQSP / RV128I (101)
            // Not supported...

            return payload;
        }

        #endregion

        #region Type decoder

        private RvcPayload DecodeCI(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            var immediate = 0;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            // Read the opcode
            var opCode = buffer & 0x3;

            // Read the Immediate coding Bit 2..6
            buffer >>= 2;
            immediate = buffer & 0x1F;

            // Read the rd register
            buffer >>= 5;
            var rd = buffer & 0x1F;

            // Read the imm. Bit 12
            buffer >>= 5;
            var imm12 = (buffer & 0x01) << 5;
            immediate |= imm12;

            // read F3
            buffer >>= 1;
            var f3 = buffer & 0x7;

            payload.LoadCI(opCode, immediate,rd,f3);
            return payload;
        }

        private RvcPayload DecodeCSS(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            var immediate = 0;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // Read RS2
            buffer >>= 2;
            var rs2 = buffer & 0x1F;

            // Immediates
            buffer >>= 5;
            immediate = buffer & 0x3F;

            // F3
            buffer >>= 6;
            var f3 = buffer & 0x7;

            payload.LoadCSS(opCode, rs2, immediate, f3);
            return payload;
        }

        private RvcPayload DecodeCL(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // Rd'
            buffer >>= 2;
            var rdc = buffer & 0x07;

            // Imm
            buffer >>= 3;
            immediate = buffer & 0x03;

            // Rs1'
            buffer >>= 2;
            var rs1c = buffer & 0x07;

            // Imme
            buffer >>= 3;
            var imm2 = buffer & 0x07;
            immediate = immediate | (imm2 << 2);

            // f3
            buffer >>= 3;
            var f3 = buffer & 0x07;

            payload.LoadCL(opCode, rdc, immediate, rs1c, f3);
            return payload;
        }

        private RvcPayload DecodeCS(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // rs2'
            buffer >>= 2;
            var rs2c = buffer & 0x07;

            // imm
            buffer >>= 3;
            immediate = buffer & 0x03;

            // rs1'
            buffer >>= 2;
            var rs1c = buffer & 0x07;

            // immm 2
            buffer >>= 3;
            var imm2 = buffer & 0x07;
            immediate = immediate | (imm2 << 2);

            // f3
            buffer >>= 3;
            var f3 = buffer & 0x07;

            payload.LoadCS(opCode, rs2c, immediate, rs1c, f3);
            return payload;
        }

        private RvcPayload DecodeCA(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            //var opCode = buffer & 0x03;

            //// RS2'
            //buffer >>= 2;
            //var rs2c = buffer & 0x07;

            //// Funct2
            //buffer >>= 3;
            //var f2 = buffer & 0x03;

            //// RD' / RS'
            //buffer >>= 2;
            //var rdcrs1c = buffer & 0x07;

            //// Funct 6
            //buffer >>= 3;
            //var f6 = buffer & 0x3F;

            //// Funct 3
            //buffer >>= 3;
            //var f3 = buffer & 0x07;

            //payload.LoadCA(opCode, rs2c, f2, rdcrs1c, f6, f3);

            var opCode = buffer & 0x03;

            // RS2'
            buffer >>= 2;
            var rs2c = buffer & 0x07;

            // CA Mode
            buffer >>= 3;
            var caMode = buffer & 0x03;

            // RD' / RS'
            buffer >>= 2;
            var rdcrs1c = buffer & 0x07;

            // Funct 6
            buffer >>= 3;
            var f6 = buffer & 0x3F;

            // Funct 3
            buffer >>= 3;
            var f3 = buffer & 0x07;

            // F2
            var f2 = f6 & 0x03;

            payload.LoadCA(opCode, rs2c, f2,caMode, rdcrs1c, f6, f3);
            return payload;
        }

        private RvcPayload DecodeCJ(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // imm
            buffer >>= 2;
            immediate = buffer & 0x3FF;

            // f3
            buffer >>= 11;
            var f3 = buffer & 0x07;

            payload.LoadCJ(opCode, immediate, f3);
            return payload;
        }

        private RvcPayload DecodeCR(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // rs2
            buffer >>= 2;
            var rs2 = buffer & 0x1F;

            // rs1
            buffer >>= 5;
            var rs1 = buffer & 0x1F;

            // funct 4
            buffer >>= 5;
            var f4 = buffer & 0x0F;

            // f3
            buffer >>= 1;
            var f3 = buffer & 0x07;

            payload.LoadCR(opCode, rs1, rs2, f4,f3);
            return payload;
        }

        private RvcPayload DecodeCB_Branch(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // Imm 1
            buffer >>= 2;
            immediate = buffer & 0x1F;

            // Rs1'
            buffer >>= 5;
            var rs1c = buffer & 0x7;

            // Imm 2
            buffer >>= 3;
            var imm2 = buffer & 0x07;
            immediate |= (imm2 << 5);

            // f3
            buffer >>= 3;
            var f3 = buffer & 0x07;

            payload.LoadCB_Branch(opCode, immediate, rs1c, f3);
            return payload;
        }

        private RvcPayload DecodeCB_IntegerRegister(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            // Op
            var opCode = buffer & 0x03;

            // Imm
            buffer >>= 2;
            immediate = buffer & 0x1F;

            // Rd', RS1'
            buffer >>= 5;
            var rdcrs1c = buffer & 0x07;

            // F2
            buffer >>= 3;
            var f2 = buffer & 0x03;

            // Imm2
            buffer >>= 2;
            var imm2 = (buffer & 0x01) << 5;
            immediate = immediate | imm2;

            // F3
            buffer >>= 1;
            var f3 = buffer & 0x07;

            payload.LoadCB_Integer(opCode, immediate, rdcrs1c, f2, f3);
            return payload;
        }

        private RvcPayload DecodeCIW(IEnumerable<byte> rvcCoding)
        {
            var payload = new RvcPayload(rvcCoding);
            int immediate;

            int buffer = rvcCoding.ElementAt(1);
            buffer <<= 8;
            buffer |= rvcCoding.First();

            var opCode = buffer & 0x03;

            // rd'
            buffer >>= 2;
            var rdc = buffer & 0x07;

            // immm
            buffer >>= 3;
            immediate = 0xFFFF;

            // f3
            buffer >>= 8;
            var f3 = buffer & 0x07;

            payload.LoadCIW(opCode, rdc, immediate, f3);
            return payload;
        }

        #endregion

    }
}
