using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BOFNET {
    /// <summary>
    /// Unpacks a byte[] than has been packed with aggressor script's bof_pack function, note that z is not supported
    /// right now, only Z which should work in most cases.
    /// </summary>
    public class Unpack {

        public List<object> Values { get; private set; } = new List<object>();

        public Unpack(string format, byte[] data) {            

            BinaryReader br = new BinaryReader(new MemoryStream(data), Encoding.Unicode);
            
            foreach(char code in format) {

                switch (code) {
                    case 'Z':
                        int lenRaw = br.ReadInt32BE();
                        StringBuilder str = new StringBuilder();
                        char ch;

                        while ((ch = br.ReadChar()) != 0) {
                            str.Append(ch);
                        }

                        Values.Add(str.ToString());
                        break;
                                                                                   
                    case 'i':
                        Values.Add(br.ReadInt32BE());
                        break;

                    case 's':
                        Values.Add(br.ReadInt16());
                        break;

                    case 'b':
                        Values.Add(br.ReadBytes(br.ReadInt32BE()));
                        break;

                    default:
                        throw new ArgumentException($"Invalid type {code} encountered in format string. Currently only type Z,i,s,b are supported");
                }
            }
        }
    }
}
