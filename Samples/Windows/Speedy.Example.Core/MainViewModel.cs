#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Speedy.Application;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy.Example.Core;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(RuntimeInformation runtimeInformation, IDispatcher dispatcher) : base(dispatcher)
	{
		RuntimeInformation = runtimeInformation;
		LeftList = new SpeedyList<SelectionOption<int>>(GetDispatcher());
		MiddleList = new SpeedyList<SelectionOption<int>>(GetDispatcher());
		RightList = new SpeedyList<SelectionOption<int>>(GetDispatcher());

		ThrottleDelayValues = new SpeedyList<SelectionOption<int>>(dispatcher)
		{
			new SelectionOption<int>(0, "No Delay"),
			new SelectionOption<int>(5, "5 ms"),
			new SelectionOption<int>(50, "50 ms"),
			new SelectionOption<int>(500, "500 ms"),
			new SelectionOption<int>(1000, "1000 ms"),
		};

		Limit = 100;
		NumberOfItems = 1000;
		NumberOfThreads = 32;
		ThrottleDelay = ThrottleDelayValues[1];
		Progress = 0;
		Maximum = 100000;

		// Commands
		NumberOfThreadsCommand = new RelayCommand(ChangeNumberOfThreads);
		RandomizeCommand = new RelayCommand(_ => Randomize());
	}

	#endregion

	#region Properties

	public bool CancellationPending { get; set; }

	public bool IsRunning { get; private set; }

	public SpeedyList<SelectionOption<int>> LeftList { get; }

	public int Limit { get; set; }

	public string ListFilterForLeft { get; set; }

	public string ListFilterForMiddle { get; set; }

	public string ListFilterForRight { get; set; }

	public bool LoopTest { get; set; }

	public int Maximum { get; set; }

	public string Message { get; private set; }

	public SpeedyList<SelectionOption<int>> MiddleList { get; }

	public int NumberOfItems { get; set; }

	public int NumberOfThreads { get; set; }

	public ICommand NumberOfThreadsCommand { get; }

	public int Progress { get; set; }

	public ICommand RandomizeCommand { get; }

	public SpeedyList<SelectionOption<int>> RightList { get; }

	public RuntimeInformation RuntimeInformation { get; }

	public SelectionOption<int> ThrottleDelay { get; set; }

	public bool UseLimit { get; set; }

	public bool UseOrder { get; set; }
	
	public SpeedyList<SelectionOption<int>> ThrottleDelayValues { get; }
	
	public TimeSpan RunElapsed { get; private set; }

	#endregion

	#region Methods

	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(ListFilterForLeft):
			{
				LeftList.IncludeInFilter = string.IsNullOrWhiteSpace(ListFilterForLeft)
					? null
					: x => x.ToString().Contains(ListFilterForLeft);
				LeftList.RefreshFilter();
				break;
			}
			case nameof(ListFilterForMiddle):
			{
				MiddleList.IncludeInFilter = string.IsNullOrWhiteSpace(ListFilterForMiddle)
					? null
					: x => x.ToString().Contains(ListFilterForMiddle);
				MiddleList.RefreshFilter();
				break;
			}
			case nameof(ListFilterForRight):
			{
				RightList.IncludeInFilter = string.IsNullOrWhiteSpace(ListFilterForRight)
					? null
					: x => x.ToString().Contains(ListFilterForRight);
				RightList.RefreshFilter();
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	private void ChangeNumberOfThreads(object obj)
	{
		switch (obj?.ToString())
		{
			case "-":
			{
				NumberOfThreads = Math.Max(1, NumberOfThreads -= 1);
				break;
			}
			case "+":
			default:
			{
				NumberOfThreads = Math.Min(32, NumberOfThreads += 1);
				break;
			}
		}
	}

	private void Randomize()
	{
		if (IsRunning)
		{
			CancellationPending = true;
			return;
		}

		Task.Run(Run)
			.ContinueWith(_ =>
			{
				if (LoopTest && !CancellationPending)
				{
					Randomize();
				}
			});
	}

	private void Run()
	{
		IsRunning = true;
		CancellationPending = false;
		Message = "Starting...\r\n";
		Message += $"\tThrottle Delay: {ThrottleDelay.Id} ms";
		Progress = 0;

		var watch = Stopwatch.StartNew();

		try
		{
			LeftList.Clear();
			LeftList.Limit = UseLimit ? Limit : int.MaxValue;
			LeftList.OrderBy = UseOrder ? new[] { new OrderBy<SelectionOption<int>>(x => x.Id) } : null;
			MiddleList.Clear();
			MiddleList.Limit = UseLimit ? Limit : int.MaxValue;
			MiddleList.OrderBy = UseOrder ? new[] { new OrderBy<SelectionOption<int>>(x => x.Id) } : null;
			RightList.Clear();
			RightList.Limit = UseLimit ? Limit : int.MaxValue;
			RightList.OrderBy = UseOrder ? new[] { new OrderBy<SelectionOption<int>>(x => x.Id) } : null;

			var total = NumberOfItems;
			var minimum = Math.Max(10, (int) (total * 0.1));

			LeftList.Load(Enumerable.Range(1, total).Select(x => new SelectionOption<int>(x, x.ToString())));
			MiddleList.Load(Enumerable.Range(total + 1, total).Select(x => new SelectionOption<int>(x, x.ToString())));
			RightList.Load(Enumerable.Range((total * 2) + 1, total).Select(x => new SelectionOption<int>(x, x.ToString())));

			var actions = EnumExtensions.GetEnumValues<ListAction>();
			var sources = new[] { LeftList, MiddleList, RightList };
			var destinations = new Dictionary<SpeedyList<SelectionOption<int>>, SpeedyList<SelectionOption<int>>[]>
			{
				{ LeftList, new[] { MiddleList, RightList } },
				{ MiddleList, new[] { LeftList, RightList } },
				{ RightList, new[] { LeftList, MiddleList } }
			};
			
			try
			{
				var options = new ParallelOptions
				{
					MaxDegreeOfParallelism = 32
				};

				Parallel.For(0, Maximum, options, _ =>
				{
					if (CancellationPending)
					{
						return;
					}

					var source = RandomGenerator.GetItem(sources);
					var destination = RandomGenerator.GetItem(destinations[source]);
					var action = RandomGenerator.GetItem(actions);

					if (source.Count > minimum)
					{
						var index = RandomGenerator.NextInteger(0, source.Count);
						if (source.TryGetAndRemoveAt(index, out var item))
						{
							switch (action)
							{
								case ListAction.Add:
								{
									destination.Add(item);
									break;
								}
								case ListAction.Insert:
								{
									destination.Insert(0, item);
									break;
								}
							}
						}
					}

					Progress++;

					if (ThrottleDelay.Id > 0)
					{
						Thread.Sleep(ThrottleDelay.Id);
					}
				});
			}
			catch (Exception ex)
			{
				Message = ex.ToDetailedString();
			}
		}
		finally
		{
			RunElapsed = watch.Elapsed;
			Message += $"\r\nDone {RunElapsed}";
			Progress = Maximum;
			IsRunning = false;
		}
	}

	public enum ListAction
	{
		Add = 0,
		Insert = 1
	}

	#endregion
}