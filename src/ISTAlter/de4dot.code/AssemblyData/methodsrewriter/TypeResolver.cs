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

using System.Reflection;
using dnlib.DotNet;

namespace ISTAlter.de4dot.code.AssemblyData.methodsrewriter {
	class TypeResolver {
		public Type type;
		Dictionary<ITypeDefOrRef, TypeInstanceResolver> typeRefToInstance = new Dictionary<ITypeDefOrRef, TypeInstanceResolver>(TypeEqualityComparer.Instance);

		public TypeResolver(Type type) => this.type = type;

		TypeInstanceResolver GetTypeInstance(ITypeDefOrRef typeRef) {
			if (!typeRefToInstance.TryGetValue(typeRef, out var instance))
				typeRefToInstance[typeRef] = instance = new TypeInstanceResolver(type, typeRef);
			return instance;
		}

		public FieldInfo Resolve(IField fieldRef) => GetTypeInstance(fieldRef.DeclaringType).Resolve(fieldRef);
		public MethodBase Resolve(IMethod methodRef) => GetTypeInstance(methodRef.DeclaringType).Resolve(methodRef);
	}
}