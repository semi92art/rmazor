using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Common.Helpers.Thread_Ninja
{
    /// <summary>
    /// Represents an async task.
    /// </summary>
    public class TaskNinja : IEnumerator
    {
        private object locker = new object();
        
        // implements IEnumerator to make it usable by StartCoroutine;
        #region IEnumerator Interface
        /// <summary>
        /// The current iterator yield return value.
        /// </summary>
        public object Current { get; private set; }

        /// <summary>
        /// Runs next iteration.
        /// </summary>
        /// <returns><code>true</code> for continue, otherwise <code>false</code>.</returns>
        public bool MoveNext()
        {
            return OnMoveNext();
        }

        public void Reset()
        {
            // Reset method not supported by iterator;
            throw new NotSupportedException(
                "Not support calling Reset() on iterator.");
        }
        #endregion

        // inner running state used by state machine;
        private enum RunningState
        {
            Init,
            RunningAsync,
            PendingYield,
            ToBackground,
            RunningSync,
            CancellationRequested,
            Done,
            Error
        }

        // routine user want to run;
        private readonly IEnumerator m_InnerRoutine;

        // current running state;
        private RunningState m_State;
        // last running state;
        private RunningState m_PreviousState;
        // temporary stores current yield return value
        // until we think Unity coroutine engine is OK to get it;
        private object m_PendingCurrent;

        /// <summary>
        /// Gets state of the task.
        /// </summary>
        public TaskState State
        {
            get
            {
                switch (m_State)
                {
                    case RunningState.CancellationRequested:
                        return TaskState.Cancelled;
                    case RunningState.Done:
                        return TaskState.Done;
                    case RunningState.Error:
                        return TaskState.Error;
                    case RunningState.Init:
                        return TaskState.Init;
                    default:
                        return TaskState.Running;
                }
            }
        }

        public TaskNinja(IEnumerator _Routine)
        {
            m_InnerRoutine = _Routine;
            // runs into background first;
            m_State = RunningState.Init;
        }

        /// <summary>
        /// Cancel the task till next iteration;
        /// </summary>
        public void Cancel()
        {
            if (State == TaskState.Running)
            {
                GotoState(RunningState.CancellationRequested);
            }
        }

        /// <summary>
        /// A co-routine that waits the task.
        /// </summary>
        public IEnumerator Wait()
        {
            while (State == TaskState.Running)
                yield return null;
        }

        // thread safely switch running state;
        private void GotoState(RunningState _State)
        {
            if (m_State == _State) return;

            lock (locker)
            {
                // maintainance the previous state;
                m_PreviousState = m_State;
                m_State = _State;
            }
        }

        // thread safely save yield returned value;
        private void SetPendingCurrentObject(object _Current)
        {
            lock (locker)
            {
                m_PendingCurrent = _Current;
            }
        }

        // actual MoveNext method, controls running state;
        private bool OnMoveNext()
        {
            // no running for null;
            if (m_InnerRoutine == null)
                return false;

            // set current to null so that Unity not get same yield value twice;
            Current = null;

            // loops until the inner routine yield something to Unity;
            while (true)
            {
                // a simple state machine;
                switch (m_State)
                {
                    // first, goto background;
                    case RunningState.Init:
                        GotoState(RunningState.ToBackground);
                        break;
                    // running in background, wait a frame;
                    case RunningState.RunningAsync:
                        return true;

                    // runs on main thread;
                    case RunningState.RunningSync:
                        MoveNextUnity();
                        break;

                    // need switch to background;
                    case RunningState.ToBackground:
                        GotoState(RunningState.RunningAsync);
                        // call the thread launcher;
                        MoveNextAsync();
                        return true;
                    // something was yield returned;
                    case RunningState.PendingYield:
                        if (m_PendingCurrent == Ninja.JumpBack)
                        {
                            // do not break the loop, switch to background;
                            GotoState(RunningState.ToBackground);
                        }
                        else if (m_PendingCurrent == Ninja.JumpToUnity)
                        {
                            // do not break the loop, switch to main thread;
                            GotoState(RunningState.RunningSync);
                        }
                        else
                        {
                            // not from the Ninja, then Unity should get noticed,
                            // Set to Current property to achieve this;
                            Current = m_PendingCurrent;

                            // yield from background thread, or main thread?
                            if (m_PreviousState == RunningState.RunningAsync)
                            {
                                // if from background thread, 
                                // go back into background in the next loop;
                                m_PendingCurrent = Ninja.JumpBack;
                            }
                            else
                            {
                                // otherwise go back to main thread the next loop;
                                m_PendingCurrent = Ninja.JumpToUnity;
                            }

                            // end this iteration and Unity get noticed;
                            return true;
                        }
                        break;
                    // done running, pass false to Unity;
                    default:
                        return false;
                }
            }
        }

        // background thread launcher;
        private void MoveNextAsync()
        {
            ThreadPool.QueueUserWorkItem(BackgroundRunner);
        }

        // background thread function;
        private void BackgroundRunner(object _State)
        {
            // just run the sync version on background thread;
            MoveNextUnity();
        }

        // run next iteration on main thread;
        private void MoveNextUnity()
        {
            try
            {
                // run next part of the user routine;
                var result = m_InnerRoutine.MoveNext();

                if (result)
                {
                    // something has been yield returned, handle it;
                    SetPendingCurrentObject(m_InnerRoutine.Current);
                    GotoState(RunningState.PendingYield);
                }
                else
                {
                    // user routine simple done;
                    GotoState(RunningState.Done);
                }
            }
            catch (Exception ex)
            {
                // exception handling, save & log it;
                Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
                // then terminates the task;
                GotoState(RunningState.Error);
            }
        }
    }
}