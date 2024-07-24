#region References

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for expressions
/// </summary>
public static class ExpressionExtensions
{
	#region Methods

	/// <summary>
	/// Creates an expression that represents a conditional AND operation that evaluates the second operand only if the first operand evaluates to true.
	/// </summary>
	/// <typeparam name="T"> The type used in the expression. </typeparam>
	/// <param name="left"> An Expression to set the Left property equal to. </param>
	/// <param name="right"> An Expression to set the Right property equal to. </param>
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
	/// Creates an expression that represents a conditional OR operation.
	/// </summary>
	/// <typeparam name="T"> The type used in the expression. </typeparam>
	/// <param name="left"> An Expression to set the Left property equal to. </param>
	/// <param name="right"> An Expression to set the Right property equal to. </param>
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

	/// <summary>
	/// Try to get the name of an expression where the expression must be a property.
	/// </summary>
	/// <typeparam name="T"> The type passed into the expression. </typeparam>
	/// <param name="expression"> The expression to process. </param>
	/// <param name="name"> The name of the expression. </param>
	/// <returns> True if the property name was found otherwise false. </returns>
	public static bool TryGetPropertyName<T>(this Expression<Func<T, object>> expression, out string name)
	{
		var exp = expression.Body;

		if (exp is UnaryExpression cast)
		{
			exp = cast.Operand;
		}

		if (exp is MemberExpression { Member.MemberType: MemberTypes.Property } member)
		{
			name = member.Member.Name;
			return true;
		}

		name = null;
		return false;
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