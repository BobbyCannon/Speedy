#region References

using Microsoft.EntityFrameworkCore;

#endregion

namespace Speedy.EntityFrameworkCore
{
    public interface IEntityTypeConfiguration
    {
        #region Methods

        void Configure(ModelBuilder instance);

        #endregion
    }
}