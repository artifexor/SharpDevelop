﻿// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;

namespace ICSharpCode.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Default implementation of <see cref="ITypeParameter"/>.
	/// </summary>
	public class DefaultTypeParameter : AbstractType, ITypeParameter
	{
		IEntity parent;
		
		string name;
		int index;
		IList<ITypeReference> constraints;
		IList<IAttribute> attributes;
		VarianceModifier variance;
		BitVector16 flags;
		
		const ushort FlagReferenceTypeConstraint      = 0x0001;
		const ushort FlagValueTypeConstraint          = 0x0002;
		const ushort FlagDefaultConstructorConstraint = 0x0004;
		
		protected override void FreezeInternal()
		{
			constraints = FreezeList(constraints);
			attributes = FreezeList(attributes);
			base.FreezeInternal();
		}
		
		public DefaultTypeParameter(IMethod parentMethod, int index, string name)
		{
			if (parentMethod == null)
				throw new ArgumentNullException("parentMethod");
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Value must not be negative");
			if (name == null)
				throw new ArgumentNullException("name");
			this.parent = parentMethod;
			this.index = index;
			this.name = name;
		}
		
		public DefaultTypeParameter(ITypeDefinition parentClass, int index, string name)
		{
			if (parentClass == null)
				throw new ArgumentNullException("parentClass");
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Value must not be negative");
			if (name == null)
				throw new ArgumentNullException("name");
			this.parent = parentClass;
			this.index = index;
			this.name = name;
		}
		
		public override string Name {
			get { return name; }
		}
		
		public override bool? IsReferenceType {
			get {
				switch (flags.Data & (FlagReferenceTypeConstraint | FlagValueTypeConstraint)) {
					case FlagReferenceTypeConstraint:
						return true;
					case FlagValueTypeConstraint:
						return false;
					default:
						return null;
				}
			}
		}
		
		public override int GetHashCode()
		{
			int hashCode = parent.GetHashCode();
			unchecked {
				hashCode += 1000000033 * index.GetHashCode();
			}
			return hashCode;
		}
		
		public override bool Equals(IType other)
		{
			DefaultTypeParameter p = other as DefaultTypeParameter;
			if (p == null)
				return false;
			return parent.Equals(p.parent)
				&& index == p.index;
		}
		
		public int Index {
			get { return index; }
		}
		
		public IList<IAttribute> Attributes {
			get {
				if (attributes == null)
					attributes = new List<IAttribute>();
				return attributes;
			}
		}
		
		public IEntity Parent {
			get { return parent; }
		}
		
		public IMethod ParentMethod {
			get { return parent as IMethod; }
		}
		
		public ITypeDefinition ParentClass {
			get { return parent as ITypeDefinition; }
		}
		
		public IList<ITypeReference> Constraints {
			get {
				if (constraints == null)
					constraints = new List<ITypeReference>();
				return constraints;
			}
		}
		
		public bool HasDefaultConstructorConstraint {
			get { return flags[FlagDefaultConstructorConstraint]; }
			set {
				CheckBeforeMutation();
				flags[FlagDefaultConstructorConstraint] = value;
			}
		}
		
		public bool HasReferenceTypeConstraint {
			get { return flags[FlagReferenceTypeConstraint]; }
			set {
				CheckBeforeMutation();
				flags[FlagReferenceTypeConstraint] = value;
			}
		}
		
		public bool HasValueTypeConstraint {
			get { return flags[FlagValueTypeConstraint]; }
			set {
				CheckBeforeMutation();
				flags[FlagValueTypeConstraint] = value;
			}
		}
		
		public VarianceModifier Variance {
			get { return variance; }
			set {
				CheckBeforeMutation();
				variance = value;
			}
		}
		
		public virtual IType BoundTo {
			get { return null; }
		}
		
		public virtual ITypeParameter UnboundTypeParameter {
			get { return null; }
		}
	}
}
