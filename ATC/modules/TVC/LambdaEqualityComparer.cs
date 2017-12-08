using System;
using System.Collections.Generic;

namespace ATC.modules.TVC
{
	class LambdaEqualityComparer<T> : IEqualityComparer<T>
	{
		private readonly Func<T, T, bool> _func;

		public LambdaEqualityComparer(Func<T, T, bool> lambda)
		{
			_func = lambda;
		}

		public bool Equals(T x, T y) => _func(x, y);

		public int GetHashCode(T obj) => base.GetHashCode();
	}
}
