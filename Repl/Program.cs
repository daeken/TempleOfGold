using System;
using System.IO;
using Runtime;

namespace Repl {
	class Program {
		static void Main(string[] args) {
			var ec = new ExecutionContext();
			Console.WriteLine(ec.Eval(File.ReadAllText("test.js")));
		}
	}
}