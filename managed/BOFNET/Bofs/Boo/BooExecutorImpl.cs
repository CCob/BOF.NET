#if (NET45)
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Extensions;
using Boo.Lang.Parser;
#endif
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;

namespace BOFNET.Bofs.Boo {

    public class BooExecutorImpl : MarshalByRefObject, BooExecutor {

        TextWriter console;
        readonly string printMacro = @"
import Boo.Lang.Compiler
import Boo.Lang.Compiler.Ast

class BooBeaconConsole:
	[Property(Console)]
	public static _booBeaconConsole as System.IO.TextWriter

macro beaconPrint:	
	if len(beaconPrint.Arguments) < 2:
		mie = [| BooBeaconConsole.Console.WriteLine() |]
		mie.Arguments = beaconPrint.Arguments
		return ExpressionStatement(mie)
	
	block = Block()
	
	last = beaconPrint.Arguments[-1]
	for arg in beaconPrint.Arguments:
		if arg is last: break
		block.Add([| BooBeaconConsole.Console.Write($arg) |].withLexicalInfoFrom(arg))
		block.Add([| BooBeaconConsole.Console.Write(' ') |])
	block.Add([| BooBeaconConsole.Console.WriteLine($last) |].withLexicalInfoFrom(last))
	
	return block	
";
        public string Execute(string code) {

#if (NET45)

            string appDomain = AppDomain.CurrentDomain.FriendlyName;
            BooCompiler compiler = new BooCompiler();
            compiler.Parameters.Input.Add(new StringInput("PrintMacro.boo", printMacro));
            compiler.Parameters.Input.Add(new StringInput("code.boo", code));
            compiler.Parameters.Pipeline = new CompileToFile();
            compiler.Parameters.Ducky = true;
            compiler.Parameters.OutputType = CompilerOutputType.Auto;
            compiler.Parameters.References.Add(typeof(BooCompiler).Assembly);
            compiler.Parameters.References.Add(typeof(BooParser).Assembly);
            compiler.Parameters.References.Add(typeof(AssertMacro).Assembly);

            CompilerContext context = compiler.Run();

            //Note that the following code might throw an error if the Boo script had bugs.
            //Poke context.Errors to make sure.
            if (context.GeneratedAssembly != null) {

                Type booBeaconConsole = context.GeneratedAssembly.GetType("BooBeaconConsole");
                PropertyInfo pi = booBeaconConsole.GetProperty("Console", BindingFlags.Public | BindingFlags.Static);
                pi.SetValue(null, console, null);

                MethodInfo main = context.GeneratedAssembly.EntryPoint;
                return  (string)main.Invoke(null, new object[] { new string[] { } });

            } else {
                string compilerErrors = "";
                foreach (CompilerError error in context.Errors)
                    compilerErrors += error;

                throw new ArgumentException(compilerErrors, "code");
            } 
#else
            return "Boo execution not supported on .NET 3.5 or below";
#endif
        }

        public void Init(ObjectHandle consoleFacade) {
            console = consoleFacade.Unwrap() as TextWriter;
        }

        public void LoadAssembly(byte[] data) {
            Assembly.Load(data);
        }
    }
}
