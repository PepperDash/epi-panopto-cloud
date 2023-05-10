// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Events
{
    /// <summary>
    /// Arguments for events that refer to a list changing.
    /// </summary>
    public class ListChangedEventArgs<T> : EventArgs
    {
        public ListChangedEventArgs(
           ListChangedAction changedAction,
           T oldItem,
           T newItem,
           int index)
        {
            ChangedAction = changedAction;
            OldItem = oldItem;
            NewItem = newItem;
            Index = index;
        }

        /// <summary>
        /// The action that caused this event.
        /// </summary>
        public ListChangedAction ChangedAction { get; protected set; }

        /// <summary>
        /// Item that was removed or replaced.
        /// </summary>
        public T OldItem { get; private set; }

        /// <summary>
        /// Item that was added.
        /// </summary>
        public T NewItem { get; private set; }

        /// <summary>
        /// Index of the <see cref="OldItem"/> or <see cref="NewItem"/>.
        /// <para>
        /// For added item events, this is then current index of the item.
        /// For removed item events, this is the index from which the item was removed.
        /// For replace events, this is the index of the item being replaced (and therefore the incoming item, too).
        /// For reset events, this will be -1.     
        /// </para>
        /// </summary>
        public int Index { get; private set; }
    }


    /// <summary>
    /// Describes the action that caused an applicable list changed event.
    /// </summary>
    public enum ListChangedAction
    {
        /// <summary>
        /// An item was added to a list.
        /// </summary>
        Added,

        /// <summary>
        /// An item was removed from a list.
        /// <para>
        /// NOTE: During this "Removed" changed action, the item being removed may not be fully functional.
        /// However, it will be identifiable.
        /// For example, if it has an ID or a Name property, they will still be valid,
        /// but a method such as SetValue() or a property such as CurrentValue
        /// will not be valid and may even cause an error if called/accessed during
        /// this "Removed" action.
        /// </para>
        /// </summary>
        Removed,

        /// <summary>
        /// An item in a list was replaced with another item.
        /// <para>
        /// If the old and new items are the same reference,
        /// it means one or more parts of the object changed and
        /// all data based on that object should be refreshed.
        /// </para>
        /// </summary>
        Replaced,

        /// <summary>
        /// The content of the list has been cleared or has changed significantly.
        /// Subscribers to event being raised should request a new copy of the list
        /// from the object that raised the event.
        /// </summary>
        Reset
    }

}