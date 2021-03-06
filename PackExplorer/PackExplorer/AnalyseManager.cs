﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
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
    /// <summary>
    /// Extracting Algorithm Mehtods
    /// </summary>
    public class AnalyseManager
    {
        static byte[] Idstring = Encoding.ASCII.GetBytes("GPDA");
        static byte[] Gzip = { 0x1f, 0x8b };
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
            //Jump the head
            br.ReadInt32();

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

            foreach (Entry suben in entries)
            {
                byte[] buf = new byte[suben.size];

                br.BaseStream.Seek(suben.offset, SeekOrigin.Begin);
                br.Read(buf, 0, (int)suben.size);

                MemoryStream ms = new MemoryStream(buf);
                ret.Add(new Element(ms, suben.offset, suben.size, suben.name));

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
            byte[] chk = e.GetHeadBytes(4);
            for (int i = 0; i < 3; i++)
            {
                if (chk[i] != Idstring[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static Element UncompressGzip(Element e)
        {
            byte[] buf=new byte[0x100];
            GZipStream gz = new GZipStream(e.Data, CompressionMode.Decompress);
            MemoryStream ms = new MemoryStream();
            int length;
            while ((length=gz.Read(buf,0,buf.Length))!=0)
            {
                ms.Write(buf, 0, length);
            }
            return new Element(ms, 0, ms.Length, e.Name);
        }
        public static bool CheckGzip(Element e)
        {
            byte[] chk = e.GetHeadBytes(2);
            if (chk[0] == Gzip[0] && chk[1] == Gzip[1])
                return true;
            else
                return false;
        }
    }
}
