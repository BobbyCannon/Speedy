using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speedy.Samples.Entities
{
	public class Setting : Entity<string>
	{
		public override string Id { get; set; }
	}
}
