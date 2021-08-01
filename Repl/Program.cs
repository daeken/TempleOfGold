using System;
using System.IO;
using Runtime;

namespace Repl {
	class Program {
		static void Main(string[] args) {
			var ec = new ExecutionContext();
			while(true) {
				Console.Write("> ");
				var inp = Console.ReadLine();
				if(inp == null) break;
				var val = (string) (ec.Eval(inp)?.ToString());
				if(val != null)
					Console.WriteLine(val);
			}
		}
	}
}