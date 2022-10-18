#region References

using System;
using Foundation;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

public static class PlatformExtensions
{
	#region Methods

	public static DateTime ToDateTime(this NSDate date)
	{
		return (DateTime) date;
	}

	#endregion
}