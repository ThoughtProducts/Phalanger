/*

 Copyright (c) 2004-2006 Tomas Matousek.
  
 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using PHP.Core.Reflection;

namespace PHP.Core
{
	/// <summary>
	/// Summary description for PhpString.
	/// </summary>
	public sealed class PhpString : IPhpVariable, IPhpObjectGraphNode
	{
		public const string PhpTypeName = PhpVariable.TypeNameString;

		/// <summary>
		/// Copy-on-write aware string builder.
		/// </summary>
		private CowStringBuilder cow;

		#region Nested Class: CowStringBuilder

		private class CowStringBuilder
		{
			public StringBuilder Builder;
			public int RefCount = 1;

			public CowStringBuilder(string str)
			{
				Builder = new StringBuilder(str);
			}

			public CowStringBuilder(string str, int capacity)
			{
				Builder = new StringBuilder(str, capacity);
			}

			public CowStringBuilder(string str1, string str2)
			{
				if (str1 == null)
				{
					Builder = new StringBuilder(str2);
				}
				else if (str2 == null)
				{
					Builder = new StringBuilder(str1);
				}
				else
				{
					Builder = new StringBuilder(str1, str1.Length + str2.Length);
					Builder.Append(str2);
				}
			}
		}

		#endregion

		#region Construction

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="phps"></param>
		private PhpString(PhpString phps)
		{
			this.cow = phps.cow;
			this.cow.RefCount++;
		}

		public PhpString(string str)
		{
			this.cow = new CowStringBuilder(str);
		}

		public PhpString(string str1, string str2)
		{
			this.cow = new CowStringBuilder(str1, str2);
		}

		#endregion

		#region IPhpVariable Members

		public bool IsEmpty()
		{
			return cow.Builder.Length == 0 || (cow.Builder.Length == 1 && cow.Builder[0] == '0');
		}

		public bool IsScalar()
		{
			return true;
		}

		public string GetTypeName()
		{
			return PhpTypeName;
		}

		#endregion

		#region IPhpConvertible Members

		public PHP.Core.PhpTypeCode GetTypeCode()
		{
			return PhpTypeCode.PhpString;
		}

		public double ToDouble()
		{
			return Convert.StringToDouble(cow.Builder.ToString());
		}

		public int ToInteger()
		{
			return Convert.StringToInteger(cow.Builder.ToString());
		}

		public long ToLongInteger()
		{
			return Convert.StringToLongInteger(cow.Builder.ToString());
		}

		public bool ToBoolean()
		{
			int length = cow.Builder.Length;
			return length != 0 && (length != 1 || cow.Builder[0] != 0);
		}

		public PhpBytes ToPhpBytes()
		{
			return new PhpBytes(cow.Builder.ToString());
		}

		public Convert.NumberInfo ToNumber(out int intValue, out long longValue, out double doubleValue)
		{
			return Convert.StringToNumber(cow.Builder.ToString(), out intValue, out longValue, out doubleValue);
		}

		public override string ToString()
		{
			return cow.Builder.ToString();
		}

		string IPhpConvertible.ToString()
		{
			return ToString();
		}

        string IPhpConvertible.ToString(DTypeDesc caller)
        {
            return ToString();
        }

		/// <summary>
		/// Converts instance to its string representation according to PHP conversion algorithm.
		/// </summary>
		/// <param name="success">Indicates whether conversion was successful.</param>
		/// <param name="throwOnError">Throw out 'Notice' when conversion wasn't successful?</param>
		/// <returns>The converted value.</returns>
		string IPhpConvertible.ToString(bool throwOnError, out bool success)
		{
			success = true;
			return ToString();
		}

		#endregion

		#region IPhpPrintable Members

		public void Print(System.IO.TextWriter output)
		{
			PhpVariable.Print(output, cow.Builder.ToString());
		}

		public void Dump(System.IO.TextWriter output)
		{
			PhpVariable.Dump(output, cow.Builder.ToString());
		}

		public void Export(System.IO.TextWriter output)
		{
			PhpVariable.Export(output, cow.Builder.ToString());
		}

		#endregion

		#region IPhpCloneable Members

		public object DeepCopy()
		{
			return new PhpString(this);
		}

		public object Copy(PHP.Core.CopyReason reason)
		{
			return new PhpString(this);
		}

		public PhpString Copy()
		{
			return new PhpString(this);
		}

		#endregion

		#region IPhpComparable Members

		public int CompareTo(object obj)
		{
			return CompareTo(obj, PhpComparer.Default);
		}

		public int CompareTo(object obj, IComparer/*!*/ comparer)
		{
			Debug.Assert(comparer != null);
            return comparer.Compare(cow.Builder.ToString(), obj);
		}

		#endregion

		#region Read Operations

		public int Length
		{
			get
			{
				return cow.Builder.Length;
			}
		}

		internal char GetCharUnchecked(int index)
		{
			Debug.Assert(index >= 0 && index < cow.Builder.Length);
			return cow.Builder[index];
		}

		#endregion

		#region Write Operations

		public PhpString/*!*/ Append(string str)
		{
			if (cow.RefCount > 1)
			{
				cow.RefCount--;
				cow = new CowStringBuilder(cow.Builder.ToString(), str);
			}
			else
			{
				cow.Builder.Append(str);
			}

			return this;
		}

		public PhpString/*!*/ Append(char c)
		{
			if (cow.RefCount > 1)
			{
				cow.RefCount--;
				cow = new CowStringBuilder(cow.Builder.ToString(), cow.Builder.Length + 1);
			}

			cow.Builder.Append(c);

			return this;
		}

		public PhpString/*!*/ Append(char c, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if (cow.RefCount > 1)
			{
				cow.RefCount--;
				cow = new CowStringBuilder(cow.Builder.ToString(), cow.Builder.Length + count);
			}

			cow.Builder.Append(c, count);

			return this;
		}

		public PhpString/*!*/ Prepend(string str)
		{
			if (cow.RefCount > 1)
			{
				cow.RefCount--;
				cow = new CowStringBuilder(str, cow.Builder.ToString());
			}
			else
			{
				cow.Builder.Insert(0, str);
			}

			return this;
		}

		internal void SetCharUnchecked(int index, char value)
		{
			Debug.Assert(index >= 0 && index < cow.Builder.Length);

			if (cow.RefCount > 1)
			{
				cow.RefCount--;
				cow = new CowStringBuilder(cow.Builder.ToString());
			}

			cow.Builder[index] = value;
		}

		#endregion

		#region IPhpObjectGraphNode Members

		/// <summary>
		/// Walks the object graph rooted in this node.
		/// </summary>
		/// <param name="callback">The callback method.</param>
		/// <param name="context">Current <see cref="ScriptContext"/>.</param>
		public void Walk(PHP.Core.PhpWalkCallback callback, ScriptContext context)
		{ }

		#endregion
	}

	#region Array Proxy

	/// <summary>
	/// Created by ensuring operators (i.e. when a chain is written) if the ensured value is a non-empty string.
	/// Holds a string container (<see cref="PhpString"/> or <see cref="PhpBytes"/>).
	/// The operator immediately following the ensuring operator either raises an error if it is an ensuring operator
	/// or modifies byte/character in the string if it is a <see cref="PhpArray.SetArrayItem"/> operator.
	/// </summary>
	internal sealed class PhpArrayString : PhpArray // TODO: Bytes/String
	{
		public override bool IsProxy { get { return true; } }
		
		internal PhpString String { get { return (PhpString)obj; } }
		internal PhpBytes Bytes { get { return (PhpBytes)obj; } }
		internal object Object { get { return obj; } }

		readonly private object obj/*!*/;

		internal PhpArrayString(object obj)
		{
			Debug.Assert(obj is PhpString || obj is PhpBytes);
			this.obj = obj;
		}
		
		#region Operators

		public override object GetArrayItem(object key, bool quiet)
		{
			Debug.Fail("N/A: written chains only");
			throw null;
		}
		
		public override void SetArrayItem(object key, object value)
		{
			PhpString str = obj as PhpString;

			int index;
			if (Operators.CheckStringIndexRange(index = Convert.ObjectToInteger(key), Int32.MaxValue, false))
			{	
				if (str != null)
					Operators.SetStringItem(str, index, value);
				else
					Operators.SetBytesItem((PhpBytes)obj, index, value);
			}		
		}

		public override PhpReference/*!*/ GetArrayItemRef()
		{
			PhpException.VariableMisusedAsArray(obj, true);
			return new PhpReference();
		}

		public override PhpReference/*!*/ GetArrayItemRef(object key)
		{
			PhpException.VariableMisusedAsArray(obj, true);
			return new PhpReference();
		}

		public override PhpReference/*!*/ GetArrayItemRef(int key)
		{
			PhpException.VariableMisusedAsArray(obj, true);
			return new PhpReference();
		}

		public override PhpReference/*!*/ GetArrayItemRef(string key)
		{
			PhpException.VariableMisusedAsArray(obj, true);
			return new PhpReference();
		}

		public override void SetArrayItem(object value)
		{
			PhpException.VariableMisusedAsArray(obj, false);
		}

		public override void SetArrayItemRef(object key, PhpReference value)
		{
			PhpException.VariableMisusedAsArray(obj, true);
		}

		public override void SetArrayItem(int key, object value)
		{
			PhpException.VariableMisusedAsArray(obj, true);
		}

		public override void SetArrayItem(string key, object value)
		{
			PhpException.VariableMisusedAsArray(obj, true);
		}

		public override void SetArrayItemExact(string key, object value, int hashcode)
		{
			PhpException.VariableMisusedAsArray(obj, true);
		}

		public override void SetArrayItemRef(int key, PhpReference value)
		{
			PhpException.VariableMisusedAsArray(obj, true);
		}

		public override void SetArrayItemRef(string/*!*/ key, PhpReference value)
		{
			PhpException.VariableMisusedAsArray(obj, true);
		}		

		public override PhpArray EnsureItemIsArray(object key)
		{
			// error (postponed error, which cannot be reported by the previous operator):  
			PhpException.VariableMisusedAsArray(obj, false);
			return null;
		}

		public override PhpArray EnsureItemIsArray()
		{
			PhpException.VariableMisusedAsArray(obj, false);
			return null;
		}

		public override DObject EnsureItemIsObject(object key, ScriptContext/*!*/ context)
		{
			// error (postponed error, which cannot be reported by the previous operator):  
			PhpException.VariableMisusedAsObject(obj, false);
			return null;
		}

		public override DObject EnsureItemIsObject(ScriptContext/*!*/ context)
		{
			PhpException.VariableMisusedAsObject(obj, false);
			return null;
		}
		
		#endregion
	}

	#endregion
}