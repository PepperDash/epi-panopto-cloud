// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;

namespace Crestron.Panopto.Common
{
    public class MinHeap<T>
    {
        public int Count { get; private set; }

        private class Node
        {
            public Node(T obj, int priority, ulong id)
            {
                Object = obj;
                Priority = priority;
                Id = id;
            }

            public T Object { get; set; }
            public int Priority { get; set; }
            public ulong Id { get; set; }
        }

        private Node[] Collection { get; set; }

        public T[] Values
        {
            get
            {
                var temp = new T[Count];
                for (var i = 0; i < Count; i++)
                {
                    temp[i] = Collection[i].Object;
                }
                return temp;
            }
        }

        public MinHeap(int maxSize)
        {
            Count = 0;
            Collection = new Node[maxSize];
        }

        public void Add(T obj, int priority, ulong id)
        {
            if (Count < Collection.Length)
            {
                Collection[Count] = new Node(obj, priority, id);
                Count++;
                SlideNodeUp(Count - 1);
            }
        }

        public T Extract()
        {
            if (Count == 0) throw new Exception("Minheap extract failure");
            Count--;
            Swap(0, Count);
            var min = Collection[Count].Object;
            Collection[Count] = null;
            SlideNodeDown2(0);
            return min;
        }

        // Should consider not using a Minheap since this method might be called a lot
        public void Remove(int index)
        {
            if (Count > 0)
            {
                if (index == 0)
                {
                    Extract();
                }
                else if (index < Count)
                {
                    // This is a Minheap, so just assign the value to the lowest possible value making 
                    // it go to the top of the list, then extract it                
                    Collection[index].Priority = int.MinValue;
                    SlideNodeUp(index);
                    Extract();
                }
            }
        }

        public void Modify(int index, T obj)
        {
            if (index < Count)
            {
                Collection[index].Object = obj;
            }
        }

        public void Clear()
        {
            for (var i = 0; i < Count; i++)
            {
                Collection[i] = null;
            }
            Count = 0;
        }



        private void SlideNodeDown2(int position)
        {
            int leftChild = 2 * position + 1;
            int rightChild = (2 * position) + 2;
            int newPosition = position;
            int prevPosition = position;
            int parent = (int)Math.Floor((double)(position - 1) / 2);
            int rightSib = position + 1;
            int rightSibParent = (int)Math.Floor((double)(rightSib - 1) / 2);

            //Determine if can go down a level on left child side
            if ((leftChild < Count) && (Collection[leftChild].Priority < Collection[position].Priority))
            {
                Swap(position, leftChild);
                position = leftChild;
                SlideNodeDown2(position);
                //Now check the the right sibling of the position that left child was moved up to.
                CheckRightSib2(prevPosition);

            }
            else if ((leftChild < Count) && (Collection[leftChild].Priority == Collection[position].Priority) &&
                (Collection[leftChild].Id < Collection[position].Id))
            {
                Swap(position, leftChild);
                position = leftChild;
                SlideNodeDown2(position);
                //Now check the the right sibling of the position that left child was moved up to.
                CheckRightSib2(prevPosition);

            }
            //Else determine if can go down a level on right child side
            else if ((rightChild < Count) && (Collection[rightChild].Priority < Collection[position].Priority))
            {
                Swap(position, rightChild);
                position = rightChild;
                SlideNodeDown2(position);
                //Now check the the right sibling of the position that left child was moved up to.
                CheckRightSib2(prevPosition);
            }
            else if ((rightChild < Count) && (Collection[rightChild].Priority == Collection[position].Priority) &&
                (Collection[rightChild].Id < Collection[position].Id))
            {
                Swap(position, rightChild);
                position = rightChild;
                SlideNodeDown2(position);
                //Now check the the right sibling of the position that left child was moved up to.
                CheckRightSib2(prevPosition);
            }
            //Else determine if the current position's right siblling (if it exists), should be swapped with this position
            //"If it exists" also means check that current positon is not already the right child of parent
            else if (((position + 1) < Count) && (position != (2 * parent) + 2) &&
                      (parent == rightSibParent) &&
                      (Collection[position + 1].Priority < Collection[position].Priority))
            {
                Swap(position, (position + 1));
                position++;
                SlideNodeDown2(position);
            }
            else if (((position + 1) < Count) && (position != (2 * parent) + 2) &&
                      (parent == rightSibParent) &&
                      (Collection[position + 1].Priority == Collection[position].Priority) &&
                      (Collection[position + 1].Id < Collection[position].Id))
            {
                Swap(position, (position + 1));
                position++;
                SlideNodeDown2(position);
            }

            return;
        }



        private void CheckRightSib2(int position)
        {
            if (position == 0)
                return;

            int parent = (int)Math.Floor((double)(position - 1) / 2);
            int rightSib = position + 1;
            int parentRightSib = (int)Math.Floor((double)(rightSib - 1) / 2);

            //Check that the right Sib is valid by checking that its
            //parent is the same as the parent of the original position
            if ((parent == parentRightSib) && (rightSib < Count) &&
                (position != (2 * parent) + 2) &&
                (Collection[rightSib].Priority < Collection[position].Priority))
            {
                Swap(position, rightSib);
                position = rightSib;
                SlideNodeDown2(position);
            }
            else if ((parent == parentRightSib) && (rightSib < Count) &&
                     (position != (2 * parent) + 2) &&
                     (Collection[rightSib].Priority == Collection[position].Priority) &&
                     (Collection[rightSib].Id < Collection[position].Id))
            {
                Swap(position, rightSib);
                position = rightSib;
                SlideNodeDown2(position);
            }

            return;
        }



        private void SlideNodeUp(int position)
        {
            var parent = (int)Math.Floor((double)(position - 1) / 2);
            if ((position == 0) || (parent >= position))
            {
                return;
            }

            // 
            //Go as far up on left side of tree without recursion
            //
            while ((position > 0) && (Collection[position].Priority < Collection[parent].Priority))
            {
                Swap(position, parent);

                // If positon is a right child 
                // For parent going down check if there is a left sib it should swap with
                // The parent would have been there before the left sib so check <=
                if (position == (parent * 2 + 2))
                {
                    int leftSibb = (parent * 2 + 1);
                    //Parent now in location "position"
                    if (Collection[position].Priority <= Collection[leftSibb].Priority)
                    {
                        Swap(leftSibb, position);
                    }
                }

                position = parent;
                parent = (int)Math.Floor((double)(position - 1) / 2);
            }

            if ((position == 0) || (parent >= position))
            {
                return;
            }


            //
            //If parent is a left child check with parents right sibling if it exists 
            //
            int grandParent = (int)Math.Floor((double)(parent - 1) / 2);
            int rightSib = parent * 2 + 2;
            int leftSib = parent * 2 + 1;


            if ((parent != 0) && (parent == (grandParent * 2 + 1)))
            {
                //Being added as child of parent that is itself a left child
                //That is the start of a new node leaf
                //Also there will be no sub-tree rooted at the parents sibling or else this new node
                // being added would have been added to that path so just ...
                //Check parents sibling which must exist for this position to be added as a child of a parent
                //that is itself a left child.

                int uncle = parent + 1;
                if ((uncle < Count) && (Collection[position].Priority < Collection[uncle].Priority))
                {
                    Swap(position, uncle);
                    SlideNodeDown2(uncle);
                }
            }

            if ((rightSib < Count) && (position == rightSib) &&
                (Collection[position].Priority < Collection[leftSib].Priority))
            {
                Swap(position, leftSib);
                SlideNodeDown2(leftSib);
            }


            return;
        }


        private void Swap(int p1, int p2)
        {
            var temp = Collection[p1];
            Collection[p1] = Collection[p2];
            Collection[p2] = temp;
        }

    }
}
