using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using String_Manipulation.Utility;
using Task = System.Threading.Tasks.Task;

namespace String_Manipulation
{
	internal sealed class Decrement
	{
		private const int CommandId = 0x0200;

		private static readonly Guid CommandSet = new Guid("d76396e3-c7ac-4e9a-bfd5-41bee2650ae4");

		private readonly AsyncPackage _package;

		private Decrement(AsyncPackage package, OleMenuCommandService commandService)
		{
			_package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandId = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(Execute, menuCommandId);
			commandService.AddCommand(menuItem);
		}

		public static Decrement Instance { get; private set; }

		private IAsyncServiceProvider ServiceProvider => _package;

		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
			Instance = new Decrement(package, commandService);
		}

		private async void Execute(object sender, EventArgs e)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			var dte = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
			if (dte?.ActiveDocument == null) return;

			var selection = (TextSelection) dte.ActiveDocument.Selection;
			var text = selection.Text;

			text = Regex.Replace(text, @"-?(\d+\.\d+|\d+)", match =>
			{
				var number = match.Value.ToDecimal();
				var exponent = -match.Value.GetDecimalsCount();
				var decrement = Math.Pow(10, exponent).ToDecimal();
				number -= decrement;
				return number.ToString(CultureInfo.InvariantCulture);
			});

			selection.Insert(text, 0);
		}
	}
}