using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto
{
    public class ThreadSafeQueue
    {
        private readonly List<Command> _queue = new List<Command>();
        private readonly CCriticalSection _queueLock = new CCriticalSection();

        public string Name { get; private set; }

        public ThreadSafeQueue (string name)
        {
            Name = name;
        }

        //This should only be called inside the lock in the Add method
        private bool SearchCommandsForCommand(Command newCommand, List<Command> commands)
        {
            bool exists = false;

            foreach (Command command in commands)
            {
                if (command.Name == newCommand.Name)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }

        public int Count()
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.Count");
            try
            {
                _queueLock.Enter();
                return _queue.Count;
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.Count Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
                return -1;
            }
            finally
            {
                _queueLock.Leave();
            }
        }

        public void Clear()
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.Clear");
            try
            {
                _queueLock.Enter();
                _queue.Clear();
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.Clear Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                _queueLock.Leave();
            }
        }

        public void Add(Command item)
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.Add");
            try
            {
                _queueLock.Enter();
                if (item is Command)
                {
                    if (!SearchCommandsForCommand(item, _queue))
                    {
                        PanoptoLogger.Notice("Adding {0} command to queue {1}", item.Name, Name);
                        _queue.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.Add Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                _queueLock.Leave();
            }
        }

        public void RemoveItem(Command item)
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.RemoveItem");
            try
            {
                _queueLock.Enter();
                _queue.Remove(item);
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.RemoveItem Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                _queueLock.Leave();
            }
        }

        public void RemoveAt(int index)
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.RemoveAt");
            try
            {
                _queueLock.Enter();
                _queue.RemoveAt(index);
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.RemoveAt Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                _queueLock.Leave();
            }
        }

        public Command GetAt(int index)
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.GetAt");
            Command command = null;
            try
            {
                _queueLock.Enter();
                if (_queue.Count > 0)
                {
                    command = _queue[index];
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.GetAt Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
            finally
            {
                _queueLock.Leave();
            }
            return command;
        }

        public List<Command> GetItems()
        {
            PanoptoLogger.Notice("Panopto.ThreadSafeQueue.GetItems");
            try
            {
                _queueLock.Enter();
                return _queue.ToList();
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.ThreadSafeQueue.GetItems Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
                return null;
            }
            finally
            {
                _queueLock.Leave();
            }
        }
    }
}