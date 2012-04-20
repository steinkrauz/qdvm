using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace qdasm {
    public class AsmParser2 {
        enum SpSt { ws, symb1, symbN, str, comm };
        Dictionary<String, byte> opc;
        Dictionary<String, byte> defs;
        List<string> res;
        public AsmParser2() {
            opc = new Dictionary<string, byte>();
            res = new List<string>();
            defs = new Dictionary<string, byte>();

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
            opc.Add("inc",15);
            opc.Add("dec",16);
            opc.Add("jeq",20);
            opc.Add("jne",21);
            opc.Add("jls",22);
            opc.Add("jle",23);
            opc.Add("jgr",24);
            opc.Add("jge",25);
            opc.Add("jmp",26);
            opc.Add("jz", 27);
            opc.Add("jnz",28);
            opc.Add("pop",30);
            opc.Add("dup",31);
            opc.Add("swp",32);
            opc.Add("rnd",33);
            opc.Add("clk", 34);
            opc.Add("hlt", 35);
            opc.Add("scl",100);
            opc.Add("cal",101);
            opc.Add("ret",110);
            


        }

        byte[] Int2Bytes(int idata) {
             byte[] ib = new byte[4];
             for (int i = 0; i <= 3; i++)
                 ib[i] = (byte)((idata >> (i * 8)) & 0x000000FF);
             return ib;
        }

        string[] split(String str) {
            res.Clear();
            char[] ch = str.ToCharArray();
            string Buf = "";
            int i = 0;
            SpSt state = SpSt.ws;
            while (i < str.Length) {
                switch (state) {
                    case SpSt.ws:
                        if (Char.IsWhiteSpace(ch[i])) { i++; break; }
                        state = SpSt.symb1;
                        break;
                    case SpSt.comm:
                        if (Buf.Length > 0) { res.Add(Buf); Buf = ""; }
                        i = str.Length;
                        break;
                    case SpSt.symb1:
                        Buf = "";
                        if (ch[i] == ';') { state = SpSt.comm; break; }
                        if (ch[i] == '"') { state = SpSt.str; i++; break; }
                        if (Char.IsLetterOrDigit(ch[i]) || Char.IsPunctuation(ch[i])) { Buf += ch[i++]; state = SpSt.symbN; break; }
                        throw new Exception(String.Format("Illegal char {0} at {1}", ch[i], i));
                    case SpSt.symbN:
                        if (Char.IsLetterOrDigit(ch[i]) || Char.IsPunctuation(ch[i])) { Buf += ch[i++]; break; }
                        if (ch[i] == ';') { state = SpSt.comm; break; }
                        if (Char.IsWhiteSpace(ch[i])) { res.Add(Buf); Buf = ""; state = SpSt.ws; break; }
                        throw new Exception(String.Format("Illegal char {0} at {1}", ch[i], i));
                    case SpSt.str:
                        if (ch[i] == '"') {
                            res.Add(Buf); Buf = "";
                            state = SpSt.ws; i++; break;
                        }
                        Buf += ch[i++];
                        break;
                }
            }
            if (Buf.Length > 0) res.Add(Buf);

            return res.ToArray();
        }


        public void Parse(string Line) {
            /*
             * opc - OPcode Code  (awful, isn't it?)
             * ops - OPcode String
             */
            string[] ops = split(Line);
            if (ops.Length == 0) return;
            /*foreach (string so in ops)
                Console.Write("[{0}]", so);
            Console.WriteLine();*/
            if (ops[0].StartsWith(".")){
                if (!ops[0].EndsWith(":")) throw new Exception("Malformed label");
                qdasm.LabelAddr.Add(ops[0].Substring(1,ops[0].Length-2), qdasm.CodeDepot.Count);
                return;
            }
            if (ops[0].StartsWith("#")) {
                ParseDirectives(ops);
                return;
            }
            ParseOpcode(ops);
        }

        private void ParseDirectives(string[] ops) {
            switch (ops[0]) {
                case "#defnum":
                    byte data;
                    try {
                        data = Convert.ToByte(ops[2]);
                        defs.Add(ops[1], data);
                    } catch (Exception ex) {
                        throw new Exception(String.Format("{0}: {1}", ops[2], ex.Message));
                    }
                    break;
                case "#include":
                    try {
                        TextReader tr = new StreamReader(ops[1]);
                        String Buf; int line = 1;
                        while ((Buf = tr.ReadLine()) != null) {
                            try {
                                Parse(Buf);
                            } catch {
                                Console.WriteLine(" at line {0} in {1}", line, ops[1]);
                            }
                            line++;
                        }
                        tr.Close();
                    } catch (Exception ex) {
                        System.Console.Write("{0}: {1}", ops[1], ex.Message);
                    }
                    break;
                default: throw new Exception(String.Format("Directive {0} is unknown", ops[0]));
            }
        }

        private void ParseOpcode(string[] ops) {
            switch (ops[0]) {
                //one-word opcodes
                case "nop":
                case "add":
                case "sub":
                case "div":
                case "mod":
                case "inc":
                case "dec":
                case "ret":
                case "pop":
                case "dup":
                case "rnd":
                case "swp":
                case "clk":
                case "hlt":
                    qdasm.CodeDepot.Add(opc[ops[0]]);
                    if (ops.Length > 1) throw new Exception("Extra data for a single-word opcode");
                    break;
                //op with byte-value
                case "ldb":
                case "lsb":
                case "ldv":
                case "stv":
                case "scl":
                    qdasm.CodeDepot.Add(opc[ops[0]]);
                    if (ops.Length < 2) throw new Exception("Opcode argument missing");

                    byte data;
                    if (Char.IsDigit(ops[1][0])) {
                        try {
                            data = Convert.ToByte(ops[1]);
                        } catch (Exception ex) {
                            throw new Exception(String.Format("{0}: {1}", ops[1], ex.Message));
                        }
                    } else {
                        if (defs.ContainsKey(ops[1])) {
                            data = defs[ops[1]];
                        } else {
                            throw new Exception(String.Format("Unknown definition: {0}", ops[1]));
                        }
                    }
                    qdasm.CodeDepot.Add(data);
                    break;
                //op with int-value
                case "lsi":
                case "ldi":
                case "sti":
                    qdasm.CodeDepot.Add(opc[ops[0]]);
                    if (ops.Length < 2) throw new Exception("Opcode argument missing");
                    int ival = -1;
                    if (Char.IsDigit(ops[1][0])) {
                        try {
                            ival = Convert.ToInt32(ops[1]);
                        } catch (Exception ex) {
                            throw new Exception(String.Format("{0}: {1}", ops[1], ex.Message));
                        }
                    }
                    if (Char.IsLetter(ops[1][0])) {
                        if (defs.ContainsKey(ops[1])) {
                            ival = (int)defs[ops[1]];
                        } else {
                            throw new Exception(String.Format("Unknown definition: {0}", ops[1]));
                        }
                    }
                    if (ops[1][0] == '.') {
                        ops[1] = ops[1].Substring(1);
                        LabelUse OneUse;
                        OneUse.Name = ops[1];
                        OneUse.offset = qdasm.CodeDepot.Count;
                        qdasm.LabelUses.Add(OneUse);
                        qdasm.CodeDepot.Add(255);
                        qdasm.CodeDepot.Add(255);
                        //fill space up to Int32
                        qdasm.CodeDepot.Add(0);
                        qdasm.CodeDepot.Add(0);
                        break;
                    }
                    qdasm.CodeDepot.AddRange(Int2Bytes(ival));
                    break;

                //jumps
                case "jne":
                case "jeq":
                case "jls":
                case "jle":
                case "jgr":
                case "jge":
                case "jz":
                case "jnz":
                case "jmp":
                case "cal":
                    qdasm.CodeDepot.Add(opc[ops[0]]);
                    if (ops.Length < 2) throw new Exception("Opcode argument missing");
                    if (ops[1].StartsWith(".")) {
                        ops[1] = ops[1].Substring(1);
                        LabelUse OneUse;
                        OneUse.Name = ops[1];
                        OneUse.offset = qdasm.CodeDepot.Count;
                        qdasm.LabelUses.Add(OneUse);
                        qdasm.CodeDepot.Add(255);
                        qdasm.CodeDepot.Add(255);
                    } else {
                        throw new Exception("You must use a label for any jump.");
                    }
                    break;
                    //storage
                case "db": 
                    try {
                        data = Convert.ToByte(ops[1]);
                    } catch (Exception ex) {
                        throw new Exception(String.Format("{0}: {1}", ops[1], ex.Message));
                    }
                    qdasm.CodeDepot.Add(data);
                    break;
                case "di":
                    int idata;
                    try {
                        idata = Convert.ToInt32(ops[1]);
                    } catch (Exception ex) {
                        throw new Exception(String.Format("{0}: {1}", ops[1], ex.Message));
                    }
                    qdasm.CodeDepot.AddRange(Int2Bytes(idata)); break;
                case "ds":
                    byte[] bytes = Encoding.Default.GetBytes(ops[1]);
                    qdasm.CodeDepot.AddRange(bytes);
                    qdasm.CodeDepot.Add(0);
                    break;
                default:
                    throw new Exception("Invalid opcode encountered");
            }
        }
    }

}
