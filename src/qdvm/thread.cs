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

namespace qdvm {

    public enum QDThrState { NEW, RUN, WAIT, FROZEN, DEAD };

    public class QDVMthread {
        public QDThrState state;
        public String id;
        public int ip;
        public Stack<int> st;
        public Stack<int> rs;
        public int[] vars;
        public byte[] code;
        public QDVMthread() {
            id = "";
            st = new Stack<int>();
            rs = new Stack<int>();
            vars = new int[QDVMdef.LOC_VAR_COUNT];
            code = null;
            state = QDThrState.NEW;
            ip = 0;
        }
        public void Kick() { state = QDThrState.WAIT; }
        public void Load(string Name) {
            string DiskName = String.Format("{0}{1}{2}", StkQDVM.BaseDir, Path.DirectorySeparatorChar, Name);
            try {
                BinaryReader br = new BinaryReader(File.Open(DiskName, FileMode.Open));
                byte type = br.ReadByte();
                switch (type) {
                    case 0: LoadAbsCode(br); break;
                    default: throw new Exception("Unsupported file format");
                }
                br.Close();
            } catch (Exception ex) {
                Console.Out.WriteLine(ex.Message);
            }
        }

        private void LoadAbsCode(BinaryReader br) {
            int size = br.ReadInt32();
            code = br.ReadBytes(size);
        }

    }


    public delegate T Func<T>(QDVMthread thr);
}
