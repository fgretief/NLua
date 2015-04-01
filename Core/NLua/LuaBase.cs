/*
 * This file is part of NLua.
 * 
 * Copyright (c) 2014 Vinicius Jarina (viniciusjarina@gmail.com)
 * Copyright (C) 2003-2005 Fabio Mascarenhas de Queiroz.
 * Copyright (C) 2012 Megax <http://megax.yeahunter.hu/>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections;
using System.Dynamic;
using System.Collections.Generic;
using System.Globalization;

namespace NLua
{
	/// <summary>
	/// Base class to provide consistent disposal flow across lua objects. Uses code provided by Yves Duhoux and suggestions by Hans Schmeidenbacher and Qingrui Li 
	/// </summary>
	public abstract partial class LuaBase : IDisposable
	{
		private bool _Disposed;
		[CLSCompliantAttribute(false)]
		protected int
			_Reference;
		[CLSCompliantAttribute(false)]
		protected Lua
			_Interpreter;

		~LuaBase ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public virtual void Dispose (bool disposeManagedResources)
		{
			if (!_Disposed) {
				if (disposeManagedResources) {
					if (_Reference != 0)
						_Interpreter.DisposeInternal (_Reference);
				}

				_Interpreter = null;
				_Disposed = true;
			}
		}

		public override bool Equals (object o)
		{
			if (o is LuaBase) {
				var l = (LuaBase)o;
				return _Interpreter.CompareRef (l._Reference, _Reference);
			} else
				return false;
		}

		public override int GetHashCode ()
		{
			return _Reference;
		}
	}

#if !NET35
    public partial class LuaBase : DynamicObject
    {
    }

    public class DynamicArray : DynamicObject, IEnumerable<object>, IEnumerable<string>
    {
        private readonly object[] _array;

        internal DynamicArray(object[] array)
        {
            _array = array ?? new object[0];
        }

        public object this[int index]
        { 
            get { return _array[index]; }
        }
       
        public int Length
        {
            get { return _array.Length; }
        }

        public override string ToString()
        {
            if (_array.Length == 1)
                return _array[0].ToString();

            return _array.ToString();
        }

        public static implicit operator object[](DynamicArray d)
        {
            return d._array;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            int index;
            if (!int.TryParse(binder.Name, out index))
                return false;
            if (index >= _array.Length)
                return false;

            result = _array[index];
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;

            int index;
            if (!int.TryParse(indexes[0].ToString(), out index))
                return false;
            if (index >= _array.Length)
                return false;

            result = _array[index];
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;

            if (binder.Type == typeof(object[]))
            {
                result = _array;
                return true;
            }

            if (_array.Length != 1)
                return false;

            if (_array[0] is LuaTable || _array[0] is LuaFunction)
            {
                result = _array[0];
                return true;
            }

            if (_array[0].GetType() != binder.Type)
                return false;

            result = Convert.ChangeType(_array[0], binder.Type);
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                yield return i.ToString(CultureInfo.InvariantCulture);
            }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                yield return _array[i].ToString();
            }
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }
    }
#endif
}