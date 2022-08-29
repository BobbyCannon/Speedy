#region References

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Sync;
using Speedy.Website.Models;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class CodeGeneratorTests
	{
		#region Methods

		[TestMethod]
		public void CopyOverloadForICloneableToClipboard()
		{
			var type = typeof(SyncEngineState);
			var deepCloneUsesShallowClone = true;
			var builder = new StringBuilder();
			var properties = type
				//.GetCachedProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
				.GetCachedProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite)
				.OrderBy(x => x.Name)
				.ToList();

			builder.AppendLine($"/// <inheritdoc />\r\npublic override {type.Name} DeepClone(int levels = -1)\r\n{{");

			if (deepCloneUsesShallowClone)
			{
				builder.AppendLine("\treturn ShallowClone();");
			}
			else
			{
				builder.AppendLine("\t// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******\r\n");

				builder.AppendLine($"\r\n\treturn new {type.Name}\r\n\t{{");

				foreach (var p in properties)
				{
					builder.AppendLine($"\t\t{p.Name} = {p.Name},");
				}

				builder.Remove(builder.Length - 3, 1);
				builder.AppendLine("\t};");
			}

			builder.AppendLine("}");
			builder.AppendLine();
			builder.AppendLine($"/// <inheritdoc />\r\npublic override {type.Name} ShallowClone()\r\n{{");
			builder.AppendLine("\t// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******\r\n");
			builder.AppendLine($"\treturn new {type.Name}\r\n\t{{");
			foreach (var p in properties)
			{
				builder.AppendLine($"\t\t{p.Name} = {p.Name},");
			}

			builder.Remove(builder.Length - 3, 1);
			builder.AppendLine("\t};");
			builder.AppendLine("}");

			builder.ToString().CopyToClipboard();
			Console.Write(builder.ToString());
		}

		[TestMethod]
		public void CopyOverloadForUpdateWithToClipboard()
		{
			var type = typeof(SyncEngineState);
			var builder = new StringBuilder();
			builder.AppendLine($"/// <inheritdoc />\r\npublic override void UpdateWith(object update, params string[] exclusions)\r\n{{\r\n\tswitch (update)\r\n\t{{\r\n\t\tcase {type.FullName} options:\r\n\t\t{{\r\n\t\t\tUpdateWith(options, exclusions);\r\n\t\t\treturn;\r\n\t\t}}\r\n\t\tdefault:\r\n\t\t{{\r\n\t\t\tbase.UpdateWith(update, exclusions);\r\n\t\t\treturn;\r\n\t\t}}\r\n\t}}\r\n}}");
			builder.ToString().CopyToClipboard();
			Console.Write(builder.ToString());
		}

		[TestMethod]
		public void GenerateUpdateWith()
		{
			var type = typeof(PartialUpdateOptions);
			var builder = new StringBuilder();
			var properties = type
				.GetCachedProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
				//.GetCachedProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite)
				.OrderBy(x => x.Name)
				.ToList();

			builder.AppendLine($@"/// <summary>
/// Update the {type.Name} with an update.
/// </summary>
/// <param name=""update""> The update to be applied. </param>
/// <param name=""exclusions""> An optional set of properties to exclude. </param>
public void UpdateWith({type.Name} update, params string[] exclusions)
{{");
			builder.AppendLine("\t// If the update is null then there is nothing to do.");
			builder.AppendLine("\tif (update == null)\r\n\t{\r\n\t\treturn;\r\n\t}\r\n");
			builder.AppendLine("\t// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******");
			builder.AppendLine();
			builder.AppendLine("\tif (exclusions.Length <= 0)");
			builder.AppendLine("\t{");

			foreach (var p in properties)
			{
				builder.AppendLine($"\t\t{p.Name} = update.{p.Name};");
			}

			builder.AppendLine("\t}");
			builder.AppendLine("\telse");
			builder.AppendLine("\t{");

			foreach (var p in properties)
			{
				builder.AppendLine($"\t\tthis.IfThen(_ => !exclusions.Contains(nameof({p.Name})), x => x.{p.Name} = update.{p.Name});");
			}

			builder.AppendLine("\t}\r\n");
			builder.AppendLine("\t//base.UpdateWith(update, exclusions);");
			builder.AppendLine("}\r\n");
			builder.AppendLine($"/// <inheritdoc />\r\npublic override void UpdateWith(object update, params string[] exclusions)\r\n{{\r\n\tswitch (update)\r\n\t{{\r\n\t\tcase {type.Name} options:\r\n\t\t{{\r\n\t\t\tUpdateWith(options, exclusions);\r\n\t\t\treturn;\r\n\t\t}}\r\n\t\tdefault:\r\n\t\t{{\r\n\t\t\tbase.UpdateWith(update, exclusions);\r\n\t\t\treturn;\r\n\t\t}}\r\n\t}}\r\n}}");

			builder.ToString().CopyToClipboard();
			Console.Write(builder.ToString());
		}

		#endregion
	}
}