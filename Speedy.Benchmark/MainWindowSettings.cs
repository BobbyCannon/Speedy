#region References

using System.IO;
using System.Reflection;
using System.Text;
using Speedy.Serialization;

#endregion

namespace Speedy.Benchmark
{
	public class MainWindowSettings : Bindable
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

		#endregion
	}
}