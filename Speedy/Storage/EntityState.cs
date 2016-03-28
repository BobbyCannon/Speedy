#region References

#endregion

namespace Speedy.Storage
{
	internal class EntityState
	{
		#region Properties

		public Entity Entity { get; set; }
		public Entity OldEntity { get; set; }
		public EntityStateType State { get; set; }

		#endregion
	}
}