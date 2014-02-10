﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PackExplorer
{
    public struct Entry
    {
        public long offset;
        public long size;
        public string name;
        public Entry(long offset,long size,string name)
        {
            this.offset = offset;
            this.size = size;
            this.name = name;
        }
    }
    public class AnalyseManager
    {
        static byte[] Idstring = Encoding.ASCII.GetBytes("GPDA");
        public static List<Element> AnalysePackGPDA(Element e)
        {
            BinaryReader br = new BinaryReader(e.Data); //file stream
            List<Element> ret = new List<Element>();
            List<Entry> entries = new List<Entry>();
            //check type
            if (!CheckPackGPDA(e))
            {
                throw new Exception("Not a GPDA pack");
            }
            //File size
            if (e.Size != br.ReadInt64())
            {
                throw new Exception("Format not match (Length at 0x4)");
            }

            //Entries count
            int count = br.ReadInt32();

            //Read entry
            for (int i = 0; i < count; i++)
            {
                long e_offset = br.ReadInt64();
                int e_size = br.ReadInt32();
                int e_name_offset = br.ReadInt32();

                //read name
                long pos = br.BaseStream.Position;
                br.BaseStream.Seek(e_name_offset, SeekOrigin.Begin);
                int name_length = br.ReadInt32();
                string e_name = new string(br.ReadChars(name_length));

                entries.Add(new Entry(e_offset, e_size, e_name));
                br.BaseStream.Seek(pos, SeekOrigin.Begin);
                //Debug.WriteLine(entries[i].Name);
            }

            foreach (Entry en in entries)
            {
                byte[] buf = new byte[en.size];

                br.BaseStream.Seek(e.Offset, SeekOrigin.Begin);
                br.Read(buf, 0, (int)e.Size);

                MemoryStream ms = new MemoryStream(buf);
                ret.Add(new Element(ms, en.offset, en.size, en.name));

            }
            return ret;
        }
        /// <summary>
        /// GPDA Header checker method
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool CheckPackGPDA(Element e)
        {
            byte[] chk = e.Get4Bytes();
            for (int i = 0; i < 3; i++)
            {
                if (chk[i] != Idstring[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}