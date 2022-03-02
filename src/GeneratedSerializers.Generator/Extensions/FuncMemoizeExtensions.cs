// ******************************************************************
// Copyright � 2015-2018 nventive inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// ******************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if !HAS_NO_CONCURRENT_DICT
using System.Collections.Concurrent;
#endif

namespace GeneratedSerializers
{
	public static class FuncMemoizeExtensions
	{
		/// <summary>
		/// Memoizer with one parameter, used to perform a lazy-cached evaluation. (see http://en.wikipedia.org/wiki/Memoization)
		/// </summary>
		/// <typeparam name="T">The return type to memoize</typeparam>
		/// <param name="func">the function to evaluate</param>
		/// <returns>A memoized value</returns>
		public static Func<TKey, TResult> AsLockedMemoized<TKey, TResult>(this Func<TKey, TResult> func)
		{
#if XAMARIN
			// On Xamarin.iOS, the SynchronizedDictionary type costs a lot in terms of 
			// generic delegates, where trampolines are used a lot. As simple lock will not create that
			// much contention for now.

			var values = new Dictionary<TKey, TResult>();

			return (key) =>
			{
				lock (values)
				{
					return values.FindOrCreate(key, () => func(key));
				}
			};

#elif !HAS_NO_CONCURRENT_DICT
			var values = new ConcurrentDictionary<TKey, TResult>();

			return (key) => values.GetOrAdd(key, func);
#else
			var values = new SynchronizedDictionary<TKey, TResult>();

			return (key) =>
			{
				TResult value = default(TResult);

				values.Lock.Write(
					v => v.TryGetValue(key, out value),
					v => value = values[key] = func(key)
				);

				return value;
			};
#endif
		}
	}
}
