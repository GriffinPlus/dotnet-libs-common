///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2019 Stephen Cleary
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Threading.Tasks;

// ReSharper disable StaticMemberInGenericType

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Allocates Ids for instances on demand.
	/// 0 is an invalid/unassigned ID.
	/// Ids may be non-unique in very long-running systems.
	/// This is similar to the ID system used by <see cref="Task"/> and <see cref="TaskScheduler"/>.
	/// </summary>
	/// <typeparam name="TTag">The type for which ids are generated.</typeparam>
	// ReSharper disable once UnusedTypeParameter
	static class IdManager<TTag>
	{
		/// <summary>
		/// The last id generated for this type.
		/// This is 0, if no ids have been generated.
		/// </summary>
		private static int sLastId;

		/// <summary>
		/// Returns the id, allocating it, if necessary.
		/// </summary>
		/// <param name="id">A reference to the field containing the id.</param>
		public static int GetId(ref int id)
		{
			// If the ID has already been assigned, just use it.
			if (id != 0) return id;

			// Determine the new ID without modifying "id", since other threads may also be determining the new ID at the same time.
			int newId;

			// The Increment is in a while loop to ensure we get a non-zero ID:
			//  If we are incrementing -1, then we want to skip over 0.
			//  If there are tons of ID allocations going on, we want to skip over 0 no matter how many times we get it.
			do
			{
				newId = Interlocked.Increment(ref sLastId);
			} while (newId == 0);

			// Update the ID unless another thread already updated it.
			Interlocked.CompareExchange(ref id, newId, 0);

			// Return the current ID, regardless of whether it's our new ID or a new ID from another thread.
			return id;
		}
	}

}
