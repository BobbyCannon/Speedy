#region References

using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace Speedy.UnitTests.Collections;

public abstract class BaseCollectionTests : SpeedyUnitTest
{
	#region Methods

	protected void TestParallelOperations(IList<int> list)
	{
		var options = new ParallelOptions
		{
			MaxDegreeOfParallelism = 32
		};

		var total = 0;
		var removals = 0;

		Parallel.For(0, 1000, options, i =>
		{
			for (var j = 0; j < 100; ++j)
			{
				list.Add((i * 100) + j);
				ThreadSafe.Increment(ref total);

				if ((total > 0) && ((total % 10) == 0))
				{
					list.RemoveAt(RandomGenerator.NextInteger(0, list.Count - 1));
					ThreadSafe.Increment(ref removals);
				}

				if ((total > 0)
					&& ((RandomGenerator.NextInteger(0, 100) % 3) == 0))
				{
					var index = RandomGenerator.NextInteger(0, list.Count);
					var _ = list[index];
					list[index] = index;
				}
			}
		});

		AreEqual(100000, total);
		AreEqual(total - removals, list.Count);
	}

	#endregion
}