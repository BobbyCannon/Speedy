#region References

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace Speedy.Storage
{
	internal class ShouldSerializeContractResolver : DefaultContractResolver
	{
		#region Fields

		public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

		#endregion

		#region Methods

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			var propertyInfo = (PropertyInfo)member;
			var shouldSerialize = !propertyInfo.GetAccessors()[0].IsVirtual;

			property.ShouldSerialize = instance => shouldSerialize;

			return property;
		}

		#endregion
	}
}