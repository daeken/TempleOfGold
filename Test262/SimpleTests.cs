using NUnit.Framework;
using Runtime;

namespace Test262 {
	public class SimpleTests {
		[Test]
		public void Test1() {
			var ec = new ExecutionContext();
			ec.Eval(@"
				var foo = 5;
				var bar = 4 + 1;
			");
			Assert.AreEqual(ec["foo"], ec["bar"]);
		}
	}
}