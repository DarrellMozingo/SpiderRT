using System.Linq;
using NUnit.Framework;

namespace SpiderRT.UnitTests
{
	public class HelperExtensionsTests
	{
		[Test]
		public void Should_loop_over_each_item_of_the_enumeration_executing_the_given_action()
		{
			var total = 0;

			Enumerable.Range(1, 2).ForEach(x => total += x);

			Assert.That(total, Is.EqualTo(3));
		}
	}
}