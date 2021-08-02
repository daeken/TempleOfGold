using NUnit.Framework;
using Runtime;

namespace Test262 {
	public class SimpleTests {
		[Test]
		public void Test1() {
			var ec = new ExecutionContext();
			Assert.AreEqual(ec.Eval(@"
				5 + 6
			"), 11);
		}
	}
}