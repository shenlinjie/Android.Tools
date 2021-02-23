using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace Android.Commom
{
    public class EventToCommand
	{
        public static readonly BindableProperty ToCommandProperty =BindableProperty.CreateAttached(
            "ToCommand",typeof(ICommand),
            typeof(EventToCommand),
            null,BindingMode.OneWay,
			null,new BindableProperty.BindingPropertyChangedDelegate((bindable, value, newValue) => OnCommandChanged(bindable, value, newValue)),
			new BindableProperty.BindingPropertyChangingDelegate(((bindable, value, newValue) => {})),
			null,null);

			
		public static readonly BindableProperty FromEventProperty = BindableProperty.CreateAttached("FromEvent", typeof(string),typeof(EventToCommand),null,BindingMode.OneWay);

		private static readonly ICollection<IDisposable> Subscriptions = new Collection<IDisposable>();

		public static ICommand GetToCommand(BindableObject obj)
		{
			return (ICommand)obj.GetValue(ToCommandProperty);
		}

		public static void SetToCommand(BindableObject obj, ICommand value)
		{
			obj.SetValue(ToCommandProperty, value);
		}

		public static string GetFromEvent(BindableObject obj)
		{
			return (string)obj.GetValue(FromEventProperty);
		}

		public static void SetFromEvent(BindableObject obj, string value)
		{
			obj.SetValue(FromEventProperty, value);
		}

		private static void OnCommandChanged(BindableObject obj, object oldValue, object newValue)
		{
			var eventName = GetFromEvent(obj);

			if (string.IsNullOrEmpty(eventName))
			{
				throw new InvalidOperationException("FromEvent property is null or empty");
			}

			var subscription = Observable.FromEventPattern(obj, eventName).Subscribe(args => {
				var command = GetToCommand(obj);

				if (command != null &&
					command.CanExecute(args))
				{
					command.Execute(args);
				}
			});

			Subscriptions.Add(subscription);
		}

		/// <summary>
		/// Unsubscribes all event subscription specifically
		/// </summary>
		internal static void UnsubscribeAll()
		{
			foreach (var subscription in Subscriptions)
			{
				subscription.Dispose();
			}

			Subscriptions.Clear();
		}
	}
}