using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using System.Collections.Generic;

namespace Elite.Silverlight
{
    //thanks toAndrea Boschin http://weblogs.asp.net/aboschin/archive/2008/03/17/silverlight-2-0-a-double-click-manager.aspx
    //i have made some modifications around thread synchronization, and longclick event
    public class MouseClickManager : IDisposable
    {
        EventWaitHandle _LongClickEvent;
        Thread _longclickThread;
        bool _terminated;
        KeyValuePair<object, MouseButtonEventArgs> _longClickState;

        public event MouseButtonEventHandler Click;
        public event MouseButtonEventHandler LongClick;
        public event MouseButtonEventHandler DoubleClick;

        public int LongClicked { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MouseClickManager"/> is clicked.
        /// </summary>
        /// <value><c>true</c> if clicked; otherwise, <c>false</c>.</value>
        private bool Clicked { get; set; }
 
        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        /// <value>The control.</value>
        public Control Control { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public int Timeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseClickManager"/> class.
        /// </summary>
        /// <param name="control">The control.</param>
        public MouseClickManager(Control control, int timeout)
        {
            _terminated = false;
            this.Clicked = false;
            this.Control = control;
            this.Timeout = timeout;
            _LongClickEvent = new ManualResetEvent(false);
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(LongClickThread);
            _longclickThread = new Thread(threadStart);
            _longclickThread.Start();
        }

        public void HandleButtonDown(object sender, MouseButtonEventArgs e)
        {
            lock (this)
            {
                LongClicked++;
                _longClickState = new KeyValuePair<object, MouseButtonEventArgs>(sender, e);
                _LongClickEvent.Set();
            }
        }

        /// <summary>
        /// Resets the thread.
        /// </summary>
        /// <param name="state">The state.</param>
        private void LongClickThread(object state)
        {
            while (!_terminated)
            {
                _LongClickEvent.WaitOne();
                if (!_terminated)
                {
                    int last = LongClicked;
                    Thread.Sleep(800);
                    lock (this)
                    {
                        if (!_terminated && LongClicked == last)
                        {
                            var key = _longClickState;
                            OnLongClick(key.Key, key.Value);
                            LongClicked = 0;
                            _LongClickEvent.Reset();
                        }
                    }
                }
            }
        }

        public void HandleButtonUp(object sender, MouseButtonEventArgs e)
        {
            lock (this)
            {
                if (LongClicked > 0)
                {
                    LongClicked = 0;
                    _LongClickEvent.Reset();
                    HandleClick(sender, e);
                }
            }
        }

        /// <summary>
        /// Handles the click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        public void HandleClick(object sender, MouseButtonEventArgs e)
        {
            lock(this)
            {
                if (this.Clicked)
                {
                    this.Clicked = false;
                    OnDoubleClick(sender, e);
                }
                else
                {
                    this.Clicked = true;
                    ParameterizedThreadStart threadStart = new ParameterizedThreadStart(ResetThread);
                    Thread thread = new Thread(threadStart);
                    thread.Start(new KeyValuePair<object, MouseButtonEventArgs>(sender, e));
                }
            }
        }

        /// <summary>
        /// Resets the thread.
        /// </summary>
        /// <param name="state">The state.</param>
        private void ResetThread(object state)
        {
            Thread.Sleep(this.Timeout);

            lock (this)
            {
                if (this.Clicked)
                {
                    this.Clicked = false;
                    var key = (KeyValuePair<object, MouseButtonEventArgs>)state;
                    OnClick(key.Key, key.Value);
                }
            }
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = Click;

            if (handler != null)
                this.Control.Dispatcher.BeginInvoke(handler, sender, e);
        }

        /// <summary>
        /// Called when [double click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = DoubleClick;

            if (handler != null)
                handler(sender, e);
        }

        private void OnLongClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = LongClick;
            if (handler != null)
                this.Control.Dispatcher.BeginInvoke(handler, sender, e);
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (this)
            {
                _terminated = true;
            }
            _LongClickEvent.Set();
            _longclickThread.Join(2500);
        }

        #endregion
    }
}
