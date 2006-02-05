// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.UnitTesting;
using System;
using System.IO;

namespace ICSharpCode.CodeCoverage
{
	/// <summary>
	/// Menu command selected after right clicking a test method in the text editor
	/// to run tests with code coverage.
	/// </summary>
	public class RunTestWithCodeCoverageCommand : AbstractRunTestCommand
	{
		static MessageViewCategory category;
		static NCoverRunner runner;
		
		public RunTestWithCodeCoverageCommand()
		{
			if (runner == null) {
				runner = NCoverRunnerSingleton.Runner;
				runner.NCoverExited += new NCoverExitEventHandler(NCoverExited);
				runner.OutputLineReceived += new LineReceivedEventHandler(OutputLineReceived);
			}
		}
		
		protected override void RunTests(IProject project, IClass fixture, IMember test)
		{
			if (IsBuildRunning) {
				throw new CodeCoverageException();
			}
			
			string ncoverFileName = GetNCoverFileName();
			if (ncoverFileName != null) {
				SetNCoverRunnerProperties(ncoverFileName, project, fixture, test);
				RunNCover();
			} else {
				using (CodeCoverageRunnerNotFoundForm form = new CodeCoverageRunnerNotFoundForm()) {
					form.ShowDialog();
				}
			}
		}
		
		bool IsBuildRunning {
			get {
				return runner.IsRunning;
			}
		}
		
		/// <summary>
		/// Gets the message view output window.
		/// </summary>
		MessageViewCategory Category {
			get {
				if (category == null) {
					category = new MessageViewCategory("Code Coverage");
					CompilerMessageView cmv = (CompilerMessageView)WorkbenchSingleton.Workbench.GetPad(typeof(CompilerMessageView)).PadContent;
					cmv.AddCategory(category);
				}
				return category;
			}
		}
		
		/// <summary>
		/// Writes a line of text to the output window.
		/// </summary>
		void CategoryWriteLine(string message)
		{
			Category.AppendText(String.Concat(message, Environment.NewLine));
		}
		
		/// <summary>
		/// Brings output pad to the front.
		/// </summary>
		void ShowOutputPad()
		{
			WorkbenchSingleton.Workbench.GetPad(typeof(CompilerMessageView)).BringPadToFront();
		}
		
		bool FileNameExists(string fileName)
		{
			return fileName.Length > 0 && File.Exists(fileName);
		}
		
		void SetNCoverRunnerProperties(string ncoverFileName, IProject project, IClass fixture, IMember test)
		{
			string ncoverOutputDirectory = GetNCoverOutputDirectory(project);
			
			UnitTestApplicationStartHelper helper = new UnitTestApplicationStartHelper();
			helper.Initialize(project, fixture, test);
			helper.XmlOutputFile = Path.Combine(ncoverOutputDirectory, "NUnit.Xml");
			
			runner.NCoverFileName = ncoverFileName;
			runner.ProfiledApplicationCommand = UnitTestApplicationStartHelper.UnitTestConsoleApplication;
			runner.ProfiledApplicationCommandLineArguments = helper.GetArguments();
			runner.WorkingDirectory = Path.GetDirectoryName(helper.Assemblies[0]);
			runner.CoverageResultsFileName = Path.Combine(ncoverOutputDirectory, "Coverage.Xml");
			runner.LogFileName = Path.Combine(ncoverOutputDirectory, "Coverage.log");
			runner.AssemblyList = GetAssemblyList(project);
		}
		
		void RunNCover()
		{
			CodeCoverageService.ClearResults();
			CodeCoverageService.CodeCoverageHighlighted = true;
			
			Category.ClearText();
			TaskService.ClearExceptCommentTasks();
			ShowOutputPad();
			
			// Remove existing coverage results file.
			if (File.Exists(runner.CoverageResultsFileName)) {
				File.Delete(runner.CoverageResultsFileName);
			}
			
			// Create NCover output directory.
			if (!Directory.Exists(Path.GetDirectoryName(runner.CoverageResultsFileName))) {
				Directory.CreateDirectory(Path.GetDirectoryName(runner.CoverageResultsFileName));
			}
			
			CategoryWriteLine(StringParser.Parse("Running NCover..."));
			CategoryWriteLine(runner.CommandLine);
			
			runner.Start();
		}
		
		/// <summary>
		/// Displays the output from NCover after it has exited.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">The NCover exit event arguments.</param>
		void NCoverExited(object sender, NCoverExitEventArgs e)
		{
			System.Diagnostics.Debug.Assert(e.Error.Length == 0);

			string ncoverOutputDirectory = Path.GetDirectoryName(runner.CoverageResultsFileName);
			string unitTestResultsFileName = Path.Combine(ncoverOutputDirectory, "NUnit.Xml");
			UnitTestApplicationStartHelper.DisplayResults(unitTestResultsFileName);
			DisplayCoverageResults(runner.CoverageResultsFileName);
			
			if (TaskService.SomethingWentWrong) {
				ShowErrorList();
			}
		}
		
		void OutputLineReceived(object sender, LineReceivedEventArgs e)
		{
			CategoryWriteLine(e.Line);
		}
		
		void DisplayCoverageResults(string fileName)
		{
			if (!File.Exists(fileName)) {
				Task task = new Task(String.Empty, String.Concat("No code coverage results file generated: ", fileName), 0, 0, TaskType.Error);
				WorkbenchSingleton.SafeThreadAsyncCall(typeof(TaskService), "Add", new object[] {task});
				return;
			}
			
			CodeCoverageResults results = new CodeCoverageResults(fileName);
			WorkbenchSingleton.SafeThreadAsyncCall(typeof(CodeCoverageService), "ShowResults", new object[] {results});
		}
		
		/// <summary>
		/// Returns the full path to the NCover console application if it
		/// exists.
		/// </summary>
		string GetNCoverFileName()
		{
			string ncoverFileName = CodeCoverageOptions.NCoverFileName;
			if (FileNameExists(ncoverFileName)) {
				return ncoverFileName;
			} else {
				ncoverFileName = GetDefaultNCoverFileName();
				if (FileNameExists(ncoverFileName)) {
					return ncoverFileName;
				}
			}
			return null;
		}
		
		/// <summary>
		/// Returns the default full path to the NCover console application.
		/// </summary>
		/// <returns></returns>
		string GetDefaultNCoverFileName()
		{
			string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			return Path.Combine(programFilesPath, @"NCover\NCover.Console.exe");
		}
		
		void ShowErrorList()
		{
			PadDescriptor padDescriptor = WorkbenchSingleton.Workbench.GetPad(typeof(ErrorListPad));
			if (padDescriptor != null) {
				WorkbenchSingleton.SafeThreadAsyncCall(padDescriptor, "BringPadToFront");
			}
		}
		
		/// <summary>
		/// Reads the list of assemblies to be profiled from the project's
		/// NCover settings.
		/// </summary>
		string GetAssemblyList(IProject project)
		{
			if (project == null) {
				return String.Empty;
			}
			
			string ncoverSettingsFileName = NCoverSettings.GetFileName(project);
			if (File.Exists(ncoverSettingsFileName)) {
				NCoverSettings settings = new NCoverSettings(ncoverSettingsFileName);
				return settings.AssemblyList;
			}
			return String.Empty;
		}
		
		string GetNCoverOutputDirectory(IProject project)
		{
			return Path.Combine(project.Directory, "NCover");
		}
	}
}
