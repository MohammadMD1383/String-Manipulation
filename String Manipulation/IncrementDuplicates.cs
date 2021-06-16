using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using String_Manipulation.Utility;
using Task = System.Threading.Tasks.Task;

namespace String_Manipulation
{
	internal sealed class IncrementDuplicates
	{
		private const int CommandId = 0x0300;

		private static readonly Guid CommandSet = new Guid("d76396e3-c7ac-4e9a-bfd5-41bee2650ae4");

		private readonly AsyncPackage _package;

		private IncrementDuplicates(AsyncPackage package, OleMenuCommandService commandService)
		{
			_package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandId = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(Execute, menuCommandId);
			commandService.AddCommand(menuItem);
		}

		public static IncrementDuplicates Instance { get; private set; }

		private IAsyncServiceProvider ServiceProvider => _package;

		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new IncrementDuplicates(package, commandService);
		}


		private async void Execute(object sender, EventArgs e)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			var dte = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
			if (dte?.ActiveDocument == null) return;

			var selection = (TextSelection) dte.ActiveDocument.Selection;
			var text = selection.Text;

			var duplicates = new Dictionary<decimal, decimal>();
			text = Regex.Replace(text, @"-?(\d+\.\d+|\d+)", match =>
			{
				var number = match.Value.ToDecimal();

				if (!duplicates.ContainsKey(number))
				{
					duplicates[number] = match.Value.GetSmallestDecimal();
					return match.Value;
				}

				var incRate = duplicates[number];
				duplicates[number] += incRate.GetSmallestDecimal();
				number += incRate;
				return number.ToString(CultureInfo.InvariantCulture);
			});

			selection.Insert(text, 0);
		}
	}
}