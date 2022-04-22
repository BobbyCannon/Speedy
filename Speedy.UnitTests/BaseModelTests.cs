#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.UnitTests
{
	public abstract class BaseModelTests<T> : BaseTests where T : new()
	{
		#region Methods

		protected T GetModelWithNonDefaultValues()
		{
			var response = new T();
			response.UpdateWithNonDefaultValues();
			return response;
		}

		protected void ValidateModel(T model, params string[] exclusions)
		{
			ValidateUnwrap(model);
			ValidateUpdateWith(model, false);
			// This ensure the exclusion validation is executed, no property will be name with "EmptyGuid" name
			ValidateUpdateWith(model, false, "00000000-0000-0000-0000-000000000000");
			ValidateUpdateWith(model, true);
			model.ValidateAllValuesAreNotDefault(exclusions);
		}

		private void ValidateUnwrap(T model)
		{
			if (model is IUnwrappable unwrappable)
			{
				var actual = unwrappable.Unwrap();
				TestHelper.AreEqual(model, actual);
			}

			if (model is Entity entity)
			{
				var actual = entity.Unwrap();
				TestHelper.AreEqual(model, actual);
			}
		}

		private void ValidateUpdateWith(T model, bool excludeVirtuals, params string[] exclusions)
		{
			if (model is not IUpdatable update)
			{
				return;
			}

			var actual = new T() as IUpdatable;
			var membersToIgnore = new List<string>();

			Assert.IsNotNull(actual);

			if (excludeVirtuals)
			{
				actual.UpdateWith(update, true, exclusions);
				membersToIgnore.AddRange(typeof(T).GetVirtualPropertyNames());
			}
			else
			{
				actual.UpdateWith(update, exclusions);
			}

			TestHelper.AreEqual(update, actual, membersToIgnore.ToArray());
		}

		#endregion
	}
}