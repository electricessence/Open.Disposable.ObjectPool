﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Disposable
{
	public sealed class QueueObjectPool<T> : TrimmableObjectPoolBase<T>
		where T : class
	{

		public QueueObjectPool(Func<T> factory, Action<T> recycler, int capacity = DEFAULT_CAPACITY)
			: base(factory, recycler, capacity)
		{
			Pool = new Queue<T>(capacity); // Very very slight speed improvment when capacity is set.
		}

		public QueueObjectPool(Func<T> factory, int capacity = DEFAULT_CAPACITY)
			: this(factory, null, capacity)
		{
			
		}

		Queue<T> Pool;

		public override int Count => Pool?.Count ?? 0;

		protected override bool GiveInternal(T item)
		{
			if (Count < MaxSize)
			{
				lock (Pool) Pool.Enqueue(item); // It's possible that the count could exceed MaxSize here, but the risk is negligble as a few over the limit won't hurt.
				return true;
			}

			return false;
		}

		protected override T TryTakeInternal()
		{
			var p = Pool;
			if (p!=null && p.Count != 0)
			{
				lock (p)
				{
					if (p.Count!=0)
						return p.Dequeue();
				}

			}

			return null;
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			Pool = null;
		}
	}

	public static class QueueObjectPool
	{
		public static QueueObjectPool<T> Create<T>(Func<T> factory, int capacity = Constants.DEFAULT_CAPACITY)
			where T : class
		{
			return new QueueObjectPool<T>(factory, capacity);
		}

		public static QueueObjectPool<T> Create<T>(int capacity = Constants.DEFAULT_CAPACITY)
			where T : class, new()
		{
			return Create(() => new T(), capacity);
		}
	}
}
