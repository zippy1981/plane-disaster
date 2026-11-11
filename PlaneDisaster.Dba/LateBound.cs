/*
 * Copyright 2026 Justin Dearing
 *
 * This file is part of PlaneDisaster.NET.
 *
 * PlaneDisaster.NET is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; version 2 of the License.
 *
 * PlaneDisaster.NET is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with PlaneDisaster.NET; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PlaneDisaster.Dba
{
	/// <summary>
	/// Late bound COM calls, so the assembly needs no interop assemblies for ADOX or DAO and keeps
	/// working whichever bitness and version of them is installed.
	/// </summary>
	internal static class LateBound
	{
		/// <summary>Creates the COM object registered as <paramref name="progId"/>.</summary>
		/// <exception cref="InvalidOperationException">Nothing is registered with that ProgID.</exception>
		public static object Create(string progId)
		{
			Type type = Type.GetTypeFromProgID(progId);
			if (type == null) {
				throw new InvalidOperationException(string.Format("{0} is not registered for this process's bitness.", progId));
			}
			return Activator.CreateInstance(type);
		}


		/// <summary>Reads a property, or an indexed property when <paramref name="args"/> are given.</summary>
		public static object Get(object target, string name, params object[] args)
		{
			return Invoke(target, name, BindingFlags.GetProperty, args);
		}


		/// <summary>Sets a property to a value.</summary>
		public static void Set(object target, string name, object value)
		{
			Invoke(target, name, BindingFlags.SetProperty, new object[] { value });
		}


		/// <summary>Sets a property to an object reference (VB's Set).</summary>
		public static void SetRef(object target, string name, object value)
		{
			Invoke(target, name, BindingFlags.PutRefDispProperty, new object[] { value });
		}


		/// <summary>Calls a method.</summary>
		public static object Call(object target, string name, params object[] args)
		{
			return Invoke(target, name, BindingFlags.InvokeMethod, args);
		}


		/// <summary>Releases COM objects, skipping nulls.</summary>
		public static void Release(params object[] targets)
		{
			foreach (object target in targets) {
				if (target != null && Marshal.IsComObject(target)) {
					Marshal.ReleaseComObject(target);
				}
			}
		}


		private static object Invoke(object target, string name, BindingFlags flags, object[] args)
		{
			try {
				return target.GetType().InvokeMember(name, flags, null, target, args);
			} catch (TargetInvocationException ex) {
				// Surface the COM error (for example "Could not find file") rather than the reflection wrapper.
				if (ex.InnerException != null) {
					throw ex.InnerException;
				}
				throw;
			}
		}
	}
}
