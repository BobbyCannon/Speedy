#region References

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for expressions
/// </summary>
public static class ExpressionExtensions
{
	#region Methods

	/// <summary>
	/// Creates a expression that represents a conditional AND operation that evaluates the second operand only if the first operand evaluates to true.
	/// </summary>
	/// <typeparam name="T"> The type used in the expression. </typeparam>
	/// <param name="left"> A Expression to set the Left property equal to. </param>
	/// <param name="right"> A Expression to set the Right property equal to. </param>
	/// <returns> The updated expression. </returns>
	public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
	{
		var parameter = Expression.Parameter(typeof(T));
		var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
		var vLeft = leftVisitor.Visit(left.Body);
		var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
		var vRight = rightVisitor.Visit(right.Body);
		return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(vLeft, vRight), parameter);
	}

	/// <summary>
	/// Creates a expression that represents a conditional OR operation.
	/// </summary>
	/// <typeparam name="T"> The type used in the expression. </typeparam>
	/// <param name="left"> A Expression to set the Left property equal to. </param>
	/// <param name="right"> A Expression to set the Right property equal to. </param>
	/// <returns> The updated expression. </returns>
	public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
	{
		var parameter = Expression.Parameter(typeof(T));
		var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
		var vLeft = leftVisitor.Visit(left.Body);
		var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
		var vRight = rightVisitor.Visit(right.Body);
		return Expression.Lambda<Func<T, bool>>(Expression.Or(vLeft, vRight), parameter);
	}

	/// <summary>
	/// Specifies additional related data to be further included based on a related type that was just included.
	/// </summary>
	/// <typeparam name="T"> The type of entity being queried. </typeparam>
	/// <typeparam name="TPreviousProperty"> The type of the entity that was just included. </typeparam>
	/// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
	/// <param name="source"> The source query. </param>
	/// <param name="include"> A lambda expression representing the navigation property to be included (<c> t =&gt; t.Property1 </c>). </param>
	/// <returns> A new query with the related data included. </returns>
	public static IIncludableQueryable<T, TProperty> ThenInclude<T, TPreviousProperty, TProperty>(this IIncludableQueryable<T, ICollection<TPreviousProperty>> source, Expression<Func<TPreviousProperty, TProperty>> include) where T : class
	{
		return source.ThenInclude(include);
	}

	#endregion

	#region Classes

	private class ReplaceExpressionVisitor : ExpressionVisitor
	{
		#region Fields

		private readonly Expression _newValue;
		private readonly Expression _oldValue;

		#endregion

		#region Constructors

		public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
		{
			_oldValue = oldValue;
			_newValue = newValue;
		}

		#endregion

		#region Methods

		public override Expression Visit(Expression node)
		{
			return node == _oldValue ? _newValue : base.Visit(node);
		}

		#endregion
	}

	#endregion
}