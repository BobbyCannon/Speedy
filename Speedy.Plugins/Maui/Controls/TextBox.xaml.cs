namespace Speedy.Plugins.Maui.Controls;

public partial class TextBox
{
	#region Fields

	public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder),
		typeof(string), typeof(TextBox), string.Empty, propertyChanged: PlaceholderChanged);

	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text),
		typeof(string), typeof(TextBox), string.Empty, BindingMode.TwoWay, propertyChanged: TextChanged);

	public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword),
		typeof(bool), typeof(TextBox), false, BindingMode.TwoWay);

	public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard),
		typeof(Keyboard), typeof(TextBox), Keyboard.Default);

	#endregion

	#region Constructors

	public TextBox()
	{
		InitializeComponent();
	}

	#endregion

	#region Properties

	public bool IsPassword
	{
		get => (bool) GetValue(IsPasswordProperty);
		set => SetValue(IsPasswordProperty, value);
	}

	public Keyboard Keyboard
	{
		get => (Keyboard) GetValue(KeyboardProperty);
		set => SetValue(KeyboardProperty, value);
	}

	public string Placeholder
	{
		get => (string) GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}

	public string Text
	{
		get => (string) GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	#endregion

	#region Methods

	private void InputOnFocused(object sender, FocusEventArgs e)
	{
		UpdatePlaceholderLabel(e.IsFocused);
	}

	private void InputOnUnfocused(object sender, FocusEventArgs e)
	{
		UpdatePlaceholderLabel(e.IsFocused);
	}

	private static void PlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is TextBox textbox)
		{
			textbox.UpdatePlaceholderLabel(textbox.IsFocused);
		}
	}

	private static void TextChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is TextBox textbox)
		{
			textbox.UpdatePlaceholderLabel(textbox.IsFocused);
		}
	}

	private void UpdatePlaceholderLabel(bool isFocused)
	{
		var hasText = !string.IsNullOrEmpty(Text);

		if (isFocused || hasText)
		{
			PlaceHolderLabel.FontSize = 11;
			PlaceHolderLabel.TranslateTo(0, -25, 80, Easing.Linear);
			PlaceHolderLabel.BackgroundColor = MainGrid.BackgroundColor;
		}
		else
		{
			PlaceHolderLabel.FontSize = 15;
			PlaceHolderLabel.TranslateTo(0, 0, 80, Easing.Linear);
			PlaceHolderLabel.BackgroundColor = null;
		}
	}

	#endregion
}