using System;
using System.Collections.Generic;
using System.Text;

namespace Continuum.Core.Extension
{
	public static class StackExtensions
	{
		public static void RemoveItem<T>(this Stack<T> stack, T item)
		{
			Stack<T> removalStack = new Stack<T>();
			
			while (stack.Count > 0)
			{
				if (stack.Peek().Equals(item))
				{
					stack.Pop();
					break;
				}

				removalStack.Push(stack.Pop());
			}

			while (removalStack.Count > 0)
			{
				stack.Push(removalStack.Pop());
			}
		}

		public static T PushItem<T>(this Stack<T> stack, T item)
		{
			stack.Push(item);
			return item;
		}
	}
}
