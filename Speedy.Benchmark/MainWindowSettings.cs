#region References

using System.IO;
using System.Reflection;
using System.Text;
using Speedy.Serialization;
using Speedy.Storage;

#endregion

namespace Speedy.Benchmark
{
	public class MainWindowSettings : IUpdatable
	{
		#region Constructors

		static MainWindowSettings()
		{
			var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			FilePath = $"{directory}\\Settings.json";
		}

		#endregion

		#region Properties

		public static string FilePath { get; }

		public int Height { get; set; }

		public int Left { get; set; }

		public bool Loaded { get; set; }

		public int Top { get; set; }

		public int Width { get; set; }

		#endregion

		#region Methods

		public static MainWindowSettings Load()
		{
			try
			{
				var json = File.ReadAllText(FilePath, Encoding.UTF8);
				var settings = json.FromJson<MainWindowSettings>();
				settings.Loaded = true;
				return settings;
			}
			catch
			{
				return new MainWindowSettings { Loaded = false };
			}
		}

		public void Save()
		{
			try
			{
				var json = this.ToJson();
				File.WriteAllText(FilePath, json, Encoding.UTF8);
			}
			catch
			{
				// ignore this
			}
		}

		public void UpdateWith(MainWindowSettings update, params string[] exclusions)
		{
			if (update == null)
			{
				return;
			}

			Left = update.Left;
			Top = update.Top;
			Height = update.Height;
			Width = update.Width;
		}

		public void UpdateWith(object update, params string[] exclusions)
		{
			UpdateWith(update as MainWindowSettings, exclusions);
		}

		#endregion
	}
}