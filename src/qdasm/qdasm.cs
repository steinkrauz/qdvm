/* Author: Steinkrauz <steinkrauz@yahoo.com>
 *
 * This program is free software. It comes without any warranty, to
 * the extent permitted by applicable law. You can redistribute it
 * and/or modify it under the terms of the Do What The Fuck You Want
 * To Public License, Version 2, as published by Sam Hocevar. See
 * http://sam.zoy.org/wtfpl/COPYING for more details. */  

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace qdasm {
    public class qdasm {
        public static List<byte> CodeDepot;
        public static Dictionary<String, int> LabelAddr;
        public static List<LabelUse> LabelUses;
        static void Main(string[] args) {
            CodeDepot = new List<byte>();
            LabelAddr = new Dictionary<string, int>();
            LabelUses = new List<LabelUse>();
            TextReader tr = new StreamReader(args[args.Length-1]);
            String Buf; int i = 1;
            AsmParser2 ps = new AsmParser2();
            while ((Buf = tr.ReadLine()) != null) {
                try {
                    ps.Parse(Buf);
                } catch(Exception ex) {
                    Console.WriteLine("{1} at line {0}", i,ex.Message);
                }
                i++;
            }
            tr.Close();
            foreach (LabelUse lu in LabelUses) {
                if (!LabelAddr.ContainsKey(lu.Name)) {
                    System.Console.WriteLine("Undefined label {0}", lu.Name);
                    return;
                }
                int addr = LabelAddr[lu.Name];
                CodeDepot[lu.offset] = (byte)(addr &0xFFFFFF);
                CodeDepot[lu.offset + 1] = (byte)((addr >> 8) & 0xFFFFFF);
            }
            if (args.Length > 1 && args[0].Equals("-p")) {
                foreach (byte bb in CodeDepot)
                    Console.Write("{0},", bb);
            } else {
                try {
                    BinaryWriter bw = new BinaryWriter(File.Open(args[args.Length - 1].Replace("qda", "qdm"), FileMode.Create));
                    //mark the type of module
                    //0 -- absolute, 1 -- relative (to be implemented)
                    bw.Write((byte)0);
                    //next field is the size of code
                    bw.Write(CodeDepot.Count);
                    //now we are to write the code
                    bw.Write(CodeDepot.ToArray());
                    bw.Close();
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
    public struct LabelUse {
        public String Name;
        public int offset;
    }

    enum ParseState { Neutral, Skip, Label, Cmd, Arg, Str, Emit, Syscmd, Sysarg, Exec, Stop}
    class AsmParser {
        ParseState state;
        Dictionary<String, byte> opc;
        public AsmParser() {
            state = ParseState.Neutral;
            opc = new Dictionary<string, byte>();

            opc.Add("nop",0);
            opc.Add("ldi",1);
            opc.Add("ldb",2);
            opc.Add("lsi",3);
            opc.Add("lsb",4);
            opc.Add("ldv",5);
            opc.Add("stv",6);
            opc.Add("add",10);
            opc.Add("sub",11);
            opc.Add("mul",12);
            opc.Add("mod",14);
            opc.Add("jeq",20);
            opc.Add("jne",21);
            opc.Add("jls",22);
            opc.Add("jle",23);
            opc.Add("jgr",24);
            opc.Add("jge",25);
            opc.Add("jmp",26);
            opc.Add("pop",30);
            opc.Add("dup",31);
            opc.Add("swp",32);
            opc.Add("scl",100);
            opc.Add("ret",110);


        }
        public void Parse(String Line){
            Line += "\n";
            char[] chrs = Line.ToCharArray();
            String Cmd = "", Arg = "";
            int i = 0;
            state = ParseState.Neutral;
            bool ErrFlag = false;
            while(i<Line.Length){
                if (chrs[i] == ';') while (chrs[i] != '\n') i++; 
                switch (state) {
                    case ParseState.Neutral: if (Char.IsWhiteSpace(chrs[i])) { i++; break; }
                        if (chrs[i] == '.' && i == 0) { state = ParseState.Label; i++; Cmd = ""; break; }
                        if (chrs[i] == '#' && i == 0) { state = ParseState.Syscmd; i++; Cmd = ""; break; }
                        if (Char.IsLetter(chrs[i])) { state = ParseState.Cmd; Cmd = ""; break; }
                        System.Console.Write("Error at pos. {0} ", i); ErrFlag = true;
                        state = ParseState.Stop;
                        break;
                    case ParseState.Skip: state = ParseState.Stop; break;
                    case ParseState.Label: if (chrs[i] == ':') {
                            qdasm.LabelAddr.Add(Cmd, qdasm.CodeDepot.Count);
                            state = ParseState.Skip; break;
                        }
                        if (Char.IsLetter(chrs[i])) {
                            Cmd += chrs[i++];
                            break;
                        }
                        System.Console.Write("Error at pos. {0} ", i); ErrFlag = true;
                        state = ParseState.Stop;
                        break;
                    case ParseState.Cmd: if (Char.IsWhiteSpace(chrs[i])) { state = ParseState.Arg; break; }
                        if (Char.IsLetter(chrs[i])) {
                            Cmd += chrs[i++];
                            if (Cmd.Length > 3) {
                                ErrFlag = true;
                                System.Console.Write("Opcode too long at pos. {0} ", i); ErrFlag = true;
                                state = ParseState.Stop;
                            }
                            break;
                        }
                        System.Console.Write("Error at pos. {0} ", i); ErrFlag = true;
                        state = ParseState.Stop;
                        break;
                    case ParseState.Arg: if (chrs[i] == '\n') { state = ParseState.Emit; break; }
                        if (Char.IsLetterOrDigit(chrs[i])||chrs[i]=='.') {
                            Arg += chrs[i];
                        }
                        i++;
                        break;
                    case ParseState.Emit:
                        if (!opc.ContainsKey(Cmd)) {
                            System.Console.Write("Opcode unknown: {0} ", Cmd); ErrFlag = true;
                            state = ParseState.Stop;
                            break;
                        }
                        qdasm.CodeDepot.Add(opc[Cmd]);
                        if (Arg.Length == 0) { state = ParseState.Stop; break; }
                        if (Arg.StartsWith(".")) {
                            Arg = Arg.Substring(1);
                            LabelUse OneUse;
                            OneUse.Name = Arg;
                            OneUse.offset = qdasm.CodeDepot.Count;
                            qdasm.LabelUses.Add(OneUse);
                            qdasm.CodeDepot.Add(255);
                            qdasm.CodeDepot.Add(255);
                            state = ParseState.Stop; break;
                        }
                        byte data;
                        try {
                            data = Convert.ToByte(Arg);
                        } catch (Exception ex) {
                            System.Console.Write("{0}: {1}",Arg,ex.Message);
                            ErrFlag = true;

                            state = ParseState.Stop; break;
                        }
                        qdasm.CodeDepot.Add(data);
                        state = ParseState.Stop;
                        break;
                    case ParseState.Stop: return;
                    case ParseState.Syscmd:if (Char.IsWhiteSpace(chrs[i])) { state = ParseState.Sysarg; break; }
                        if (Char.IsLetter(chrs[i])) {
                            Cmd += chrs[i++];
                        }
                        break;
                    case ParseState.Sysarg:if (chrs[i] == '\n') { state = ParseState.Exec; break; }
                          Arg += chrs[i];
                        i++;
                        break;
                    case ParseState.Exec:
                        if (Cmd.Equals("include")) {
                            try {
                                TextReader tr = new StreamReader(Arg);
                                String Buf; int line = 1;
                                while ((Buf = tr.ReadLine()) != null) {
                                    try {
                                        Parse(Buf);
                                    } catch {
                                        Console.WriteLine(" at line {0} in {1}", line,Arg);
                                    }
                                    line++;
                                }
                                tr.Close();
                            } catch (Exception ex) {
                                System.Console.Write("{0}: {1}", Arg, ex.Message);
                                ErrFlag = true;

                                state = ParseState.Stop; break;
                            }
                            break;
                        }
                        System.Console.Write("Unknown directive: {0}", Cmd);
                        ErrFlag = true;
                        state = ParseState.Stop; 
                        break;
                }
            }
            if (ErrFlag) throw new Exception();
        }

    }
}
