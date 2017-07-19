using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using AppKit;
using Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace ReactivePropertySample
{
    public partial class ViewController : NSViewController
    {
        private Model model;

        private CompositeDisposable compositeDisposable;

        public ViewController(IntPtr handle) : base(handle)
        {
            model = new Model() { Name = "HogeName" };
            compositeDisposable = new CompositeDisposable();
            Name = model.ToReactivePropertyAsSynchronized(m => m.Name).ToReactiveProperty();
            Name.Subscribe(s => BeginInvokeOnMainThread(() => 
            {
                Label.StringValue = s ?? string.Empty; 
            })).AddTo(compositeDisposable);
        }

        public ReactiveProperty<string> Name { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TextField.Delegate = new TextFieldDelegate(s => model.Name = TextField.StringValue);
        }
    }

    public class Model : INotifyPropertyChanged
    {

        private string name;
        public string Name { get => name; set => SetProperty(ref name,value, namesPropertyChangedEventArgs); }

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly PropertyChangedEventArgs namesPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Name));

		private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
		{
			if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
				PropertyChanged?.Invoke(this, ev);
			}
		}
    }

    public class TextFieldDelegate : NSTextFieldDelegate
    {
		private readonly Action<string> callback;

        public TextFieldDelegate(Action<string> callback)
        {
            this.callback = callback;
        }

        public override void Changed(NSNotification notification)
        {
			var field = notification.Object as NSTextField;
			if (field != null)
			{
				callback?.Invoke(field.StringValue);
			}
        }
    }
}
