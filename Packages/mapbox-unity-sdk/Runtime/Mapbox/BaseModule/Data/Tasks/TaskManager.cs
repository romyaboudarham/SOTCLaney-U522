using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapbox.BaseModule.Data.Tiles;
using Mapbox.BaseModule.Utilities;
using UnityEngine;

namespace Mapbox.BaseModule.Data.Tasks
{
	public class TaskManager
	{
		public Action<TaskWrapper> TaskStarted = (t) => { };
		public Action<CanonicalTileId> TaskCancelled = (t) => { };
		public int ActiveTaskLimit = 10;
		
		protected HashSet<TaskWrapper> _runningTasks;
		protected Dictionary<int, TaskWrapper> _allTasks;
		protected Queue<int>[] _taskQueueList;
		protected Dictionary<CanonicalTileId, HashSet<int>> _tasksByTile = new Dictionary<CanonicalTileId, HashSet<int>>();
		private bool _isDestroying = false;
		private CancellationTokenSource _globalCancellationTokenSource;

		private static object _lock = new object();

		public TaskManager()
		{
			_runningTasks = new HashSet<TaskWrapper>();
			_globalCancellationTokenSource = new CancellationTokenSource();
			_taskQueueList = new Queue<int>[5]
			{
				new Queue<int>(),
				new Queue<int>(),
				new Queue<int>(),
				new Queue<int>(),
				new Queue<int>()
			};
			_allTasks = new Dictionary<int, TaskWrapper>();

			_tasksByTile = new Dictionary<CanonicalTileId, HashSet<int>>();
			//_taskPriorityQueue = new PriorityQueue<TaskWrapper, int>();
		}

		public void Initialize()
		{
			//TODO remove runnable here?
			Runnable.Run(UpdateTaskManager());
		}

		private bool TaskQueueAny()
		{
			foreach (var queue in _taskQueueList)
			{
				if (queue.Count != 0)
					return true;
			}

			return false;
		}

		private int TaskQueuePeek()
		{
			foreach (var queue in _taskQueueList)
			{
				if (queue.Count != 0)
				{
					return queue.Peek();
				}
			}

			return -1;
		}

		private int TaskQueueDequeue()
		{
			foreach (var queue in _taskQueueList)
			{
				if (queue.Count != 0)
				{
					return queue.Dequeue();
				}
			}

			return -1;
		}

		public IEnumerator UpdateTaskManager()
		{
			while (!_isDestroying)
			{
				while (TaskQueueAny() && _runningTasks.Count <= ActiveTaskLimit)
				{
					var firstPeek = TaskQueuePeek();
					// if (_allTasks.ContainsKey(firstPeek) &&
					// 	_allTasks[firstPeek].EnqueueFrame > Time.frameCount - 15 && Application.isPlaying)
					// {
					// 	yield return null;
					// }
					// else
					{
						var wrapperId = TaskQueueDequeue();
						TaskWrapper wrapper;
						if (!_allTasks.ContainsKey(wrapperId))
						{
							continue;
						}
						
						wrapper = _allTasks[wrapperId];

						//Debug.Log("running " + wrapper.Info);
						if (wrapper is MeshGenTaskWrapper meshWrapper)
						{
							HandleMeshGenTask(meshWrapper);
						}
						else
						{
							HandleTask(wrapper);
						}
						
					}
				}

				yield return null;
			}
		}

		private void HandleTask(TaskWrapper wrapper)
		{
			_allTasks.Remove(wrapper.Id);
			_tasksByTile[wrapper.OwnerTileId].Remove(wrapper.Id);
			if (_tasksByTile[wrapper.OwnerTileId].Count == 0)
			{
				_tasksByTile.Remove(wrapper.OwnerTileId);
			}
						
			TaskStarting(wrapper);
			var task = Task.Run(wrapper.Action, _globalCancellationTokenSource.Token);
			_runningTasks.Add(wrapper);
			task.ContinueWith((t) =>
			{
				if (t.IsFaulted)
				{
					Debug.Log(t.Exception?.Message);
					Debug.Break();
				}
				TaskFinished(wrapper);
				ContinueWrapper(t, wrapper);
			}, TaskScheduler.FromCurrentSynchronizationContext());
			TaskStarted(wrapper);
		}

		private void HandleMeshGenTask(MeshGenTaskWrapper wrapper)
		{
			_allTasks.Remove(wrapper.Id);
			_tasksByTile[wrapper.OwnerTileId].Remove(wrapper.Id);
			if (_tasksByTile[wrapper.OwnerTileId].Count == 0)
			{
				_tasksByTile.Remove(wrapper.OwnerTileId);
			}
						
			TaskStarting(wrapper);
			var task = Task.Run(wrapper.MeshGen);
			_runningTasks.Add(wrapper);
			task.ContinueWith((t) =>
			{
				if (t.IsFaulted)
				{
					// Debug.Log(t.Exception?.Message);
					// Debug.Log(wrapper.TileId);
					// Debug.Break();
					//return error
				}
				
				TaskFinished(wrapper);
				_runningTasks.Remove(wrapper);
				if (wrapper.ContinueMeshWith != null)
				{
					//Debug.Log(taskWrapper.FinishedFrame - taskWrapper.StartingFrame + " task timer");
					if (!t.IsFaulted)
					{
						
						wrapper.ContinueMeshWith(t.Result);
					}
					else
					{
						t.Result.ResultType = TaskResultType.MeshGenerationFailure;
						wrapper.ContinueMeshWith(null);
					}
				}
			}, TaskScheduler.FromCurrentSynchronizationContext());
			TaskStarted(wrapper);
		}

		protected virtual void TaskStarting(TaskWrapper task)
		{
			task.StartingTime = Time.realtimeSinceStartup;
		}

		protected virtual void TaskFinished(TaskWrapper task)
		{
			task.FinishedTime = Time.realtimeSinceStartup;
		}

		private void ContinueWrapper(Task task, TaskWrapper taskWrapper)
		{
			_runningTasks.Remove(taskWrapper);
			//taskWrapper.Finished(taskWrapper);
			if (taskWrapper.ContinueWith != null)
			{
				//Debug.Log(taskWrapper.FinishedFrame - taskWrapper.StartingFrame + " task timer");
				taskWrapper.ContinueWith(task);
			}
		}

		public virtual void AddTask(TaskWrapper taskWrapper, int priorityLevel = 3)
		{
			//Debug.Log(taskWrapper.Info);
			lock (_lock)
			{
				if (taskWrapper != null)
				{
					if (!_allTasks.ContainsKey(taskWrapper.Id))
					{
						taskWrapper.EnqueueFrame = Time.frameCount;
						_allTasks.Add(taskWrapper.Id, taskWrapper);

						if (!_tasksByTile.ContainsKey(taskWrapper.OwnerTileId))
						{
							_tasksByTile.Add(taskWrapper.OwnerTileId, new HashSet<int>());
						}
						_tasksByTile[taskWrapper.OwnerTileId].Add(taskWrapper.Id);
						//_taskQueue.Enqueue(taskWrapper.Id);
						_taskQueueList[priorityLevel].Enqueue(taskWrapper.Id);
					}
					else
					{
						_allTasks.Remove(taskWrapper.Id);
						if (_tasksByTile.ContainsKey(taskWrapper.OwnerTileId))
						{
							_tasksByTile[taskWrapper.OwnerTileId].Remove(taskWrapper.Id);
							if (_tasksByTile[taskWrapper.OwnerTileId].Count == 0)
							{
								_tasksByTile.Remove(taskWrapper.OwnerTileId);
							}
						}
						else
						{
							Debug.Log(taskWrapper.TileId);
						}

						taskWrapper.EnqueueFrame = Time.frameCount;
						_allTasks.Add(taskWrapper.Id, taskWrapper);

						if (!_tasksByTile.ContainsKey(taskWrapper.OwnerTileId))
						{
							_tasksByTile.Add(taskWrapper.OwnerTileId, new HashSet<int>());
						}
						_tasksByTile[taskWrapper.OwnerTileId].Add(taskWrapper.Id);
						//_taskQueue.Enqueue(taskWrapper.Id);
						_taskQueueList[priorityLevel].Enqueue(taskWrapper.Id);
					}
				}
			}

			//_taskPriorityQueue.Enqueue(taskWrapper, priority);
		}

		public virtual void CancelTile(CanonicalTileId cancelledTileId)
		{
			if (_tasksByTile.ContainsKey(cancelledTileId))
			{
				foreach (var taskId in _tasksByTile[cancelledTileId])
				{
					if (_allTasks.ContainsKey(taskId))
					{
						var task = _allTasks[taskId];
						TaskCancelled(cancelledTileId);
						_allTasks.Remove(taskId);
						task.OnCancelled();
					}
				}

				_tasksByTile.Remove(cancelledTileId);
			}
		}

		public virtual void CancelTask(TaskWrapper task)
		{
			_allTasks.Remove(task.Id);
			task.OnCancelled();
		}

		public void CancelTask(int taskKey)
		{
			if (_allTasks.TryGetValue(taskKey, out var task))
			{
				CancelTask(task);
			}
		}

		public void OnDestroy()
		{
			_globalCancellationTokenSource.Cancel();
			_isDestroying = true;
			_allTasks.Clear();
			_allTasks = null;
			_tasksByTile.Clear();
			_tasksByTile = null;
			_taskQueueList = null;
			TaskStarted = null;
			TaskCancelled = null;
		}
	}
}