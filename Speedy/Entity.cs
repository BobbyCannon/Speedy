﻿#region References

using System;

#endregion

namespace Speedy
{
	public abstract class Entity
	{
		#region Properties

		public DateTime CreatedOn { get; set; }

		public int Id { get; set; }

		#endregion
	}
}