﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Linq;
using ICSharpCode.PackageManagement.EnvDTE;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;

namespace PackageManagement.Tests.EnvDTE
{
	[TestFixture]
	public class ProjectPropertyTests
	{
		Properties properties;
		TestableDTEProject project;
		TestableProject msbuildProject;
		
		void CreateProperties()
		{
			project = new TestableDTEProject();
			msbuildProject = project.TestableProject;
			var factory = new ProjectPropertyFactory(project);
			properties = new Properties(factory);
		}
		
		[Test]
		public void Value_GetPostBuildEvent_ReturnsProjectsPostBuildEvent()
		{
			CreateProperties();
			msbuildProject.SetProperty("PostBuildEvent", "Test");
			var postBuildEventProperty = properties.Item("PostBuildEvent").Value;
			
			Assert.AreEqual("Test", postBuildEventProperty);
		}
		
		[Test]
		public void Value_GetPostBuildEvent_ReturnsUnevaluatedPostBuildEvent()
		{
			CreateProperties();
			msbuildProject.SetProperty("PostBuildEvent", "$(SolutionDir)", false);
			var postBuildEventProperty = properties.Item("PostBuildEvent").Value;
			
			Assert.AreEqual("$(SolutionDir)", postBuildEventProperty);
		}
		
		[Test]
		public void Value_GetNullProperty_ReturnsEmptyString()
		{
			CreateProperties();
			var property = properties.Item("TestTestTest").Value;
			
			Assert.AreEqual(String.Empty, property);
		}
		
		[Test]
		public void Value_SetPostBuildEvent_UpdatesProjectsPostBuildEvent()
		{
			CreateProperties();
			properties.Item("PostBuildEvent").Value = "Test";
			
			string postBuildEventProperty = msbuildProject.GetEvaluatedProperty("PostBuildEvent");
			
			Assert.AreEqual("Test", postBuildEventProperty);
		}
		
		[Test]
		public void Value_SetPostBuildEvent_DoesNotEscapeText()
		{
			CreateProperties();
			properties.Item("PostBuildEvent").Value = "$(SolutionDir)";
			
			string postBuildEventProperty = msbuildProject.GetUnevalatedProperty("PostBuildEvent");
			
			Assert.AreEqual("$(SolutionDir)", postBuildEventProperty);
		}
		
		[Test]
		public void Value_SetPostBuildEvent_MSBuildProjectIsSaved()
		{
			CreateProperties();
			properties.Item("PostBuildEvent").Value = "test";
			
			bool saved = msbuildProject.IsSaved;
			
			Assert.IsTrue(saved);
		}
		
		[Test]
		public void Value_GetTargetFrameworkMoniker_ReturnsNet40ClientProfile()
		{
			CreateProperties();
			msbuildProject.SetProperty("TargetFrameworkVersion", "4.0");
			msbuildProject.SetProperty("TargetFrameworkProfile", "Client");
			
			string targetFrameworkMoniker = properties.Item("TargetFrameworkMoniker").Value as string;
			
			string expectedTargetFrameworkMoniker = ".NETFramework,Version=v4.0,Profile=Client";
			
			Assert.AreEqual(expectedTargetFrameworkMoniker, targetFrameworkMoniker);
		}
		
		[Test]
		public void Value_GetTargetFrameworkMonikerUsingIncorrectCaseAndFrameworkIdentifierIsSilverlight_ReturnsNet35Silverlight()
		{
			CreateProperties();
			msbuildProject.SetProperty("TargetFrameworkIdentifier", "Silverlight");
			msbuildProject.SetProperty("TargetFrameworkVersion", "3.5");
			msbuildProject.SetProperty("TargetFrameworkProfile", "Full");
			
			string targetFrameworkMoniker = properties.Item("targetframeworkmoniker").Value as string;
			
			string expectedTargetFrameworkMoniker = "Silverlight,Version=v3.5,Profile=Full";
			
			Assert.AreEqual(expectedTargetFrameworkMoniker, targetFrameworkMoniker);
		}
		
		[Test]
		public void GetEnumerator_TargetFrameworkVersionSetTo40_TargetFrameworkVersionPropertyReturned()
		{
			CreateProperties();
			msbuildProject.SetProperty("TargetFrameworkVersion", "4.0");
			
			Property targetFrameworkVersionProperty = PropertiesHelper.FindProperty(project.Properties, "TargetFrameworkVersion");
			string targetFrameworkVersion = targetFrameworkVersionProperty.Value as string;
			
			Assert.AreEqual("4.0", targetFrameworkVersion);
		}
		
		[Test]
		public void Value_GetFullPathProperty_ReturnsProjectDirectory()
		{
			CreateProperties();
			msbuildProject.FileName = @"d:\projects\MyProject\MyProject.csproj";
			
			Property fullPathProperty = project.Properties.Item("FullPath");
			string fullPath = fullPathProperty.Value as string;
			
			string expectedFullPath = @"d:\projects\MyProject";
			Assert.AreEqual(expectedFullPath, fullPath);
		}
		
		[Test]
		public void Value_GetFullPathPropertyWithUpperCaseCharacters_ReturnsProjectDirectory()
		{
			CreateProperties();
			msbuildProject.FileName = @"d:\projects\MyProject\MyProject.csproj";
			
			Property fullPathProperty = project.Properties.Item("FULLPATH");
			string fullPath = fullPathProperty.Value as string;
			
			string expectedFullPath = @"d:\projects\MyProject";
			Assert.AreEqual(expectedFullPath, fullPath);
		}
		
		[Test]
		public void Value_GetOutputFileNameProperty_ReturnsOutputAssemblyFileNameWithoutPath()
		{
			CreateProperties();
			msbuildProject.AssemblyName = "MyProject";
			msbuildProject.SetProperty("OutputType", "Exe");
			
			string fileName = (string)project.Properties.Item("OutputFileName").Value;
			
			Assert.AreEqual(@"MyProject.exe", fileName);
		}
		
		[Test]
		public void Properties_GetOutputFileNamePropertyInLowerCase_ReturnsOutputAssemblyFileNameWithoutPath()
		{
			CreateProperties();
			msbuildProject.AssemblyName = "MyProject";
			msbuildProject.SetProperty("OutputType", "Library");
			
			string fileName = (string)project.Properties.Item("outputfilename").Value;
			
			Assert.AreEqual(@"MyProject.dll", fileName);
		}
		
		[Test]
		public void Properties_GetOutputFileNamePropertyWhenOutputTypeIsMissing_ReturnsOutputAssemblyFileNameWithExeFileExtension()
		{
			CreateProperties();
			msbuildProject.AssemblyName = "MyProject";
			
			string fileName = (string)project.Properties.Item("outputfilename").Value;
			
			Assert.AreEqual(@"MyProject.exe", fileName);
		}
		
		[Test]
		public void Properties_GetOutputFileNamePropertyWhenOutputTypeValueIsInLowerCase_ReturnsOutputAssemblyFileNameWithoutPath()
		{
			CreateProperties();
			msbuildProject.AssemblyName = "MyProject";
			msbuildProject.SetProperty("OutputType", "winexe");
			
			string fileName = (string)project.Properties.Item("OutputFileName").Value;
			
			Assert.AreEqual(@"MyProject.exe", fileName);
		}
		
		[Test]
		public void Properties_GetOutputFileNamePropertyWhenOutputTypeValueIsInvalid_ReturnsOutputAssemblyFileNameWithExeFileExtension()
		{
			CreateProperties();
			msbuildProject.AssemblyName = "MyProject";
			msbuildProject.SetProperty("OutputType", "invalid");
			
			string fileName = (string)project.Properties.Item("OutputFileName").Value;
			
			Assert.AreEqual(@"MyProject.exe", fileName);
		}
	}
}
