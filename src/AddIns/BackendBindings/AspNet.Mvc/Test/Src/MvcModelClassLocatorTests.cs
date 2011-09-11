﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using AspNet.Mvc.Tests.Helpers;
using ICSharpCode.AspNet.Mvc;
using NUnit.Framework;

namespace AspNet.Mvc.Tests
{
	[TestFixture]
	public class MvcModelClassLocatorTests
	{
		MvcModelClassLocator locator;
		FakeMvcProject fakeProject;
		FakeMvcParserService fakeParserService;
		
		void CreateLocator()
		{
			fakeProject = new FakeMvcProject();
			fakeParserService = new FakeMvcParserService();
			locator = new MvcModelClassLocator(fakeParserService);
		}
		
		List<IMvcClass> GetModelClasses()
		{
			return locator.GetModelClasses(fakeProject).ToList();
		}
		
		FakeMvcClass AddModelClass(string className)
		{
			return fakeParserService.AddModelClassToProjectContent(className);
		}
		
		FakeMvcClass AddModelClassWithBaseClass(string baseClassName, string className)
		{
			return fakeParserService.AddModelClassWithBaseClassToProjectContent(baseClassName, className);
		}
		
		string GetFirstModelClassName()
		{
			return GetModelClasses().First().FullName;
		}
		
		int GetModelClassCount()
		{
			return GetModelClasses().Count;
		}
		
		void UseVisualBasicProject()
		{
			fakeProject.SetVisualBasicAsTemplateLanguage();
		}
		
		void UseCSharpProject()
		{
			fakeProject.SetCSharpAsTemplateLanguage();
		}
		
		[Test]
		public void GetModelClasses_OneModelClassInProject_ReturnModelClassWithExpectedName()
		{
			CreateLocator();
			AddModelClass("MyNamespace.MyClass");
			string modelClassName = GetFirstModelClassName();
				
			Assert.AreEqual("MyNamespace.MyClass", modelClassName);
		}
		
		[Test]
		public void GetModelClasses_OneModelClassInProject_ReturnOneModelClass()
		{
			CreateLocator();
			AddModelClass("MyNamespace.MyClass");
			int count = GetModelClassCount();
				
			Assert.AreEqual(1, count);
		}
		
		[Test]
		public void GetModelClasses_NoModelClassesInProject_GetsProjectContentFromParserService()
		{
			CreateLocator();
			GetModelClasses();
			
			Assert.AreEqual(fakeProject, fakeParserService.ProjectPassedToGetProjectContent);
		}
		
		[Test]
		public void GetModelClasses_ControllerClassInProject_ControllerClassNotReturnedInModelClasses()
		{
			CreateLocator();
			AddModelClassWithBaseClass("System.Web.Mvc.Controller", "ICSharpCode.FooController");
			GetModelClasses();
			int count = GetModelClassCount();
			
			Assert.AreEqual(0, count);
		}
		
		[Test]
		public void GetModelClasses_HttpApplicationDerivedClassInProject_ClassNotReturnedInModelClasses()
		{
			CreateLocator();
			AddModelClassWithBaseClass("System.Web.HttpApplication", "ICSharpCode.MvcApplication");
			GetModelClasses();
			int count = GetModelClassCount();
			
			Assert.AreEqual(0, count);
		}
		
		[Test]
		public void GetModelClasses_VisualBasicProjectAndMyApplicationClassInProject_ClassNotReturnedInModelClasses()
		{
			CreateLocator();
			UseVisualBasicProject();
			AddModelClass("VbApp.My.MyApplication");
			int count = GetModelClassCount();
			
			Assert.AreEqual(0, count);	
		}
		
		[Test]
		public void GetModelClasses_VisualBasicProjectAndMySettingsClassInProject_ClassNotReturnedInModelClasses()
		{
			CreateLocator();
			UseVisualBasicProject();
			AddModelClass("TestVisualBasicApp.My.MySettings");
			int count = GetModelClassCount();
			
			Assert.AreEqual(0, count);	
		}
		
		[Test]
		public void GetModelClasses_CSharpProjectAndMyApplicationClassInProject_ClassIsReturnedInModelClasses()
		{
			CreateLocator();
			UseCSharpProject();
			AddModelClass("TestApp.My.MySettings");
			string modelClassName = GetFirstModelClassName();
				
			Assert.AreEqual("TestApp.My.MySettings", modelClassName);
		}
	}
}