
#if (NET40 || NET45 || NET472)
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Extensions;
using Boo.Lang.Parser;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BOFNET {

    // References:
    // https://stackoverflow.com/questions/8620885/c-sharp-binary-reader-in-big-endian
    // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings

    public static class Utils {

        private static Random random = new Random();

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

        public static string RandomString(int length) {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";            
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static Assembly CompileBooAssembly(string script, List<Assembly> references, string assemblyPath) {

#if(NET40 || NET45 || NET472)

            //We need to supply custom CompilerParameters to turn of adding default references.
            //There is a bug in EmitAssembly that forces a permission check of destination assembly folder
            //not to be performed and always resort to the AppDomain folder as the output location instead  
            BooCompiler compiler = new BooCompiler(new CompilerParameters(false));
            compiler.Parameters.Input.Add(new StringInput(RandomString(10), script));
            compiler.Parameters.OutputType = CompilerOutputType.Library;
            compiler.Parameters.Debug = false;
            compiler.Parameters.Checked = false;
            compiler.Parameters.GenerateInMemory = true;            
            compiler.Parameters.Pipeline = assemblyPath != null ? new CompileToFile() : new CompileToMemory();
            compiler.Parameters.References.Add(compiler.Parameters.LoadAssembly("System", true));
            compiler.Parameters.References.Add(compiler.Parameters.LoadAssembly("System.Runtime", true));

            if(assemblyPath != null)
                compiler.Parameters.OutputAssembly = assemblyPath;

            foreach (Assembly reference in references) {
                compiler.Parameters.References.Add(reference);
            }

            CompilerContext context = compiler.Run();
                      
            if (context.GeneratedAssembly != null) {
                
                
                return context.GeneratedAssembly;                         
            } else {
                string compilerErrors = "";
                foreach (CompilerError error in context.Errors)
                    compilerErrors += error;

                throw new ArgumentException(compilerErrors, "script");
            }
#else
            throw new NotImplementedException("Not supported under .NET 3.5 or below");
#endif
        }
    }
 }
