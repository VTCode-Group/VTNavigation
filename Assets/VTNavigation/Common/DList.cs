using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTNavigation.Common
{
	public class ListNode
	{
		public int Index;
		public ListNode Prev;
		public ListNode Next;
	}

	public class DList
	{
		public ListNode Head;
		public ListNode Tail;
		public int Count;
		public DList()
		{
			Head = null;
			Tail = null;
			Count = 0;
		}

		public bool IsEmpty()
		{
			return Count == 0;
		}

		public void AppendNode(int index)
		{
			ListNode CurNode = new ListNode();
			CurNode.Index = index;
			if (IsEmpty())
			{
				Head = CurNode;
				Tail = Head;
				CurNode.Next = CurNode;
				CurNode.Prev = CurNode;
			}
			else
			{
				CurNode.Next = Tail.Next;
				Tail.Next = CurNode;
				CurNode.Prev = Tail;
				Head.Prev = CurNode;
				Tail = CurNode;
			}
			Count++;
		}

		public void RemoveNode(ListNode inNode)
		{
			if (inNode == Head)
			{
				Head = Head.Next;
			}
			else if (inNode == Tail)
			{
				Tail = Tail.Next;
			}

			if (inNode == Head)
			{
				Head = null;
				Tail = null;
			}
			else
			{
				inNode.Prev.Next = inNode.Next;
				inNode.Next.Prev = inNode.Prev;
			}
			Count--;
		}

		public void Traver(Func<int, bool> inFunc)
		{
			ListNode Current = Head;
			for (int i = 0; i < Count; i++)
			{
				if (!inFunc(Current.Index))
				{
					break;
				}
				Current = Current.Next;
			}
		}
	}
}
