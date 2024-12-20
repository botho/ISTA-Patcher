/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using dnlib.DotNet;

namespace ISTAlter.de4dot.code {
	// "global" data and methods that is shared between all deobfuscators that deobfuscate
	// assemblies at the same time.
	public class DeobfuscatorContext : IDeobfuscatorContext {
		Dictionary<string, object> dataDict = new Dictionary<string, object>(StringComparer.Ordinal);

		public void Clear() => dataDict.Clear();
		public void SetData(string name, object data) => dataDict[name] = data;

		public object GetData(string name) {
			dataDict.TryGetValue(name, out object value);
			return value;
		}

		public void ClearData(string name) => dataDict.Remove(name);

		static ITypeDefOrRef GetNonGenericTypeRef(ITypeDefOrRef typeRef) {
			var ts = typeRef as TypeSpec;
			if (ts == null)
				return typeRef;
			var gis = ts.TryGetGenericInstSig();
			if (gis == null || gis.GenericType == null)
				return typeRef;
			return gis.GenericType.TypeDefOrRef;
		}

		public TypeDef ResolveType(ITypeDefOrRef type) {
			if (type == null)
				return null;
			type = GetNonGenericTypeRef(type);

			if (type is TypeDef typeDef)
				return typeDef;

			if (type is TypeRef tr)
				return tr.Resolve();

			return null;
		}

		public MethodDef ResolveMethod(IMethod method) {
			if (method == null)
				return null;

			if (method is MethodDef md)
				return md;

			var mr = method as MemberRef;
			if (mr == null || !mr.IsMethodRef)
				return null;

			var type = ResolveType(mr.DeclaringType);
			if (type == null)
				return null;

			return type.Resolve(mr) as MethodDef;
		}

		public FieldDef ResolveField(IField field) {
			if (field == null)
				return null;

			if (field is FieldDef fd)
				return fd;

			var mr = field as MemberRef;
			if (mr == null || !mr.IsFieldRef)
				return null;

			var type = ResolveType(mr.DeclaringType);
			if (type == null)
				return null;

			return type.Resolve(mr) as FieldDef;
		}
	}
}
