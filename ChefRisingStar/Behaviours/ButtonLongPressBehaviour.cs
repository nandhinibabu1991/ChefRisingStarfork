﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace ChefRisingStar.Behaviours
{
    public class ButtonLongPressBehaviour : Behavior<Button>
    {
        private readonly object _syncObject = new object();
        private const int Duration = 1000;

        //timer to track long press
        private Timer _timer;
        //the timeout value for long press
        private readonly int _duration;
        //whether the button was released after press
        private volatile bool _isReleased;

        /// <summary>
        /// Occurs when the associated button is long pressed.
        /// </summary>
        public event EventHandler LongPressed;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command),
            typeof(ICommand), typeof(LongPressBehaviour), default(ICommand));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(LongPressBehaviour));

        /// <summary>
        /// Gets or sets the command parameter.
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttachedTo(Button button)
        {
            base.OnAttachedTo(button);
            this.BindingContext = button.BindingContext;
            button.Pressed += Button_Pressed;
            button.Released += Button_Released;
        }

        protected override void OnDetachingFrom(Button button)
        {
            base.OnDetachingFrom(button);
            this.BindingContext = null;
            button.Pressed -= Button_Pressed;
            button.Released -= Button_Released;
        }

        /// <summary>
        /// DeInitializes and disposes the timer.
        /// </summary>
        private void DeInitializeTimer()
        {
            lock (_syncObject)
            {
                if (_timer == null)
                {
                    return;
                }
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
                Debug.WriteLine("Timer disposed...");
            }
        }

        /// <summary>
        /// Initializes the timer.
        /// </summary>
        private void InitializeTimer()
        {
            lock (_syncObject)
            {
                _timer = new Timer(Timer_Elapsed, null, _duration, Timeout.Infinite);
            }
        }

        private void Button_Pressed(object sender, EventArgs e)
        {
            _isReleased = false;
            InitializeTimer();
        }

        private void Button_Released(object sender, EventArgs e)
        {
            _isReleased = true;
            DeInitializeTimer();
        }

        protected virtual void OnLongPressed()
        {
            var handler = LongPressed;
            handler?.Invoke(this, EventArgs.Empty);
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        public ButtonLongPressBehaviour()
        {
            _isReleased = true;
            _duration = Duration;
        }

        public ButtonLongPressBehaviour(int duration) : this()
        {
            _duration = duration;
        }

        private void Timer_Elapsed(object state)
        {
            DeInitializeTimer();
            if (_isReleased)
            {
                return;
            }
            Device.BeginInvokeOnMainThread(OnLongPressed);
        }
    }
}




