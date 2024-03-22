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
using Speedy.Threading;

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

		ReaderWriterLockValues = new SpeedyList<SelectionOption<int>>(dispatcher)
		{
			new(0, "Tiny"),
			new(1, "Slim")
		};

		TestLoopValues = new SpeedyList<SelectionOption<int>>(dispatcher)
		{
			new(100, "100 (tiny)"),
			new(1000, "1000 (small)"),
			new(10000, "10k (medium)"),
			new(100000, "100k (large)"),
			new(1000000, "1m (x-large)")
		};

		ThrottleDelayValues = new SpeedyList<SelectionOption<int>>(dispatcher)
		{
			new(0, "No Delay"),
			new(5, "5 ms"),
			new(50, "50 ms"),
			new(500, "500 ms"),
			new(1000, "1000 ms")
		};

		Limit = 100;
		NumberOfItems = 1000;
		NumberOfThreads = 32;
		SelectedReaderWriterLock = ReaderWriterLockValues[0];
		SelectedTestLoopValue = TestLoopValues[1];
		SelectedThrottleDelay = ThrottleDelayValues[1];
		Progress = 0;

		// Commands
		ClearCommand = new RelayCommand(_ => Clear());
		NumberOfThreadsCommand = new RelayCommand(ChangeNumberOfThreads);
		RandomizeCommand = new RelayCommand(_ => Randomize());
	}

	#endregion

	#region Properties

	public bool CancellationPending { get; set; }

	public ICommand ClearCommand { get; }

	public bool IsRunning { get; private set; }

	public SpeedyList<SelectionOption<int>> LeftList { get; }

	public int Limit { get; set; }

	public string ListFilterForLeft { get; set; }

	public string ListFilterForMiddle { get; set; }

	public string ListFilterForRight { get; set; }

	public bool LoopTest { get; set; }

	public string Message { get; private set; }

	public SpeedyList<SelectionOption<int>> MiddleList { get; }

	public int NumberOfItems { get; set; }

	public int NumberOfThreads { get; set; }

	public ICommand NumberOfThreadsCommand { get; }

	public int Progress { get; set; }

	public ICommand RandomizeCommand { get; }

	public SpeedyList<SelectionOption<int>> ReaderWriterLockValues { get; }

	public SpeedyList<SelectionOption<int>> RightList { get; }

	public TimeSpan RunElapsed { get; private set; }

	public RuntimeInformation RuntimeInformation { get; }

	public SelectionOption<int> SelectedReaderWriterLock { get; set; }

	public SelectionOption<int> SelectedTestLoopValue { get; set; }

	public SelectionOption<int> SelectedThrottleDelay { get; set; }

	public SpeedyList<SelectionOption<int>> TestLoopValues { get; }

	public SpeedyList<SelectionOption<int>> ThrottleDelayValues { get; }

	public bool UseLimit { get; set; }

	public bool UseOrder { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(ListFilterForLeft):
			{
				LeftList.FilterCheck = string.IsNullOrWhiteSpace(ListFilterForLeft)
					? null
					: x => x.Name?.Contains(ListFilterForLeft) ?? false;
				LeftList.RefreshFilter();
				break;
			}
			case nameof(ListFilterForMiddle):
			{
				MiddleList.FilterCheck = string.IsNullOrWhiteSpace(ListFilterForMiddle)
					? null
					: x => x.Name?.Contains(ListFilterForMiddle) ?? false;
				MiddleList.RefreshFilter();
				break;
			}
			case nameof(ListFilterForRight):
			{
				RightList.FilterCheck = string.IsNullOrWhiteSpace(ListFilterForRight)
					? null
					: x => x.Name?.Contains(ListFilterForRight) ?? false;
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

	private void Clear()
	{
		LeftList.Clear();
		MiddleList.Clear();
		RightList.Clear();
	}

	private IReaderWriterLock GetLockable()
	{
		return SelectedReaderWriterLock.Id switch
		{
			1 => new ReaderWriterLockSlimProxy(),
			_ => new ReaderWriterLockTiny()
		};
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
		Message += $"\tThrottle Delay: {SelectedThrottleDelay.Id} ms";
		Progress = 0;

		var watch = Stopwatch.StartNew();
		var maximum = SelectedTestLoopValue.Id;

		try
		{
			LeftList.UpdateLock(GetLockable());
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
					MaxDegreeOfParallelism = NumberOfThreads
				};

				Parallel.For(0, maximum, options, _ =>
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
								case ListAction.Insert:
								{
									destination.Insert(0, item);
									break;
								}
								case ListAction.Add:
								default:
								{
									destination.Add(item);
									break;
								}
							}
						}
					}

					Progress++;

					if (SelectedThrottleDelay.Id > 0)
					{
						Thread.Sleep(SelectedThrottleDelay.Id);
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
			Progress = maximum;
			IsRunning = false;
		}
	}

	#endregion

	#region Enumerations

	public enum ListAction
	{
		Add = 0,
		Insert = 1
	}

	#endregion
}