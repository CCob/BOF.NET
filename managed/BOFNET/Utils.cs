using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BOFNET {

    // Base on code from here: https://stackoverflow.com/questions/8620885/c-sharp-binary-reader-in-big-endian
    public static class Utils {
        // Note this MODIFIES THE GIVEN ARRAY then returns a reference to the modified array.
        public static byte[] Reverse(this byte[] b) {
            Array.Reverse(b);
            return b;
        }

        public static UInt16 ReadUInt16BE(this BinaryReader binRdr) {
            return BitConverter.ToUInt16(binRdr.ReadBytes(sizeof(UInt16)).Reverse(), 0);
        }

        public static Int16 ReadInt16BE(this BinaryReader binRdr) {
            return BitConverter.ToInt16(binRdr.ReadBytes(sizeof(Int16)).Reverse(), 0);
        }

        public static UInt32 ReadUInt32BE(this BinaryReader binRdr) {
            return BitConverter.ToUInt32(binRdr.ReadBytes(sizeof(UInt32)).Reverse(), 0);
        }

        public static Int32 ReadInt32BE(this BinaryReader binRdr) {
            return BitConverter.ToInt32(binRdr.ReadBytes(sizeof(Int32)).Reverse(), 0);
        }
    }
 }
