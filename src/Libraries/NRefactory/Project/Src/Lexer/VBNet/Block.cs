﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Siegfried Pammer" email="siegfriedpammer@gmail.com" />
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.NRefactory.Parser.VB
{	
	public enum Context
	{
		Global,
		TypeDeclaration,
		Member,
		IdentifierExpected,
		Body,
		Xml,
		Attribute,
		Importable,
		Query,
		Expression,
		Debug,
		Default
	}
	
	public class Block : ICloneable
	{
		public static readonly Block Default = new Block() {
			context = Context.Global,
			lastExpressionStart = Location.Empty
		};
		
		public Context context;
		public Location lastExpressionStart;
		public bool isClosed;
		
		public override string ToString()
		{
			return string.Format("[Block Context={0}, LastExpressionStart={1}, IsClosed={2}]", context, lastExpressionStart, isClosed);
		}
		
		public object Clone()
		{
			return new Block() {
				context = this.context,
				lastExpressionStart = this.lastExpressionStart,
				isClosed = this.isClosed
			};
		}
	}
}
