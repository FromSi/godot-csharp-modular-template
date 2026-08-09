using System.Collections.Generic;
using System.Linq;
using Game.Game.Common.Service;
using Game.Game.Example.Domain.Observer;
using Game.Game.Example.Service;
using Game.Game.Example.UI.Observer;
using Godot;

namespace Game.Game.Example.UI;

/// <summary>
/// Example screen. Demonstrates three things: the save system, the observer pattern
/// (state → UI), and UI → Level notifications.
///
/// Type a number, press <c>Random</c> to fill it, <c>Save</c> to persist, <c>Load</c>
/// to read back and <c>New</c> to reset. Feature actions call the services directly;
/// navigation intents (<c>Quit</c>) are raised to the level via
/// <see cref="IExampleUIObserver"/> — the UI holds no navigation logic. The field and
/// the "Current value" label are driven by <see cref="INoteStateObserver.OnTextChanged"/>.
/// </summary>
public partial class ExampleUI : Control, INoteStateObserver
{
    private readonly INoteService _noteService;
    private readonly SaveService _saveService;
    private readonly NewGameService _newGameService;

    // UI is the publisher here; the level subscribes. It's a Node, so it can't extend
    // Common's ObservableState<T> — a manual list, like GameLevel's ILevelObserver.
    private readonly List<IExampleUIObserver> _observers = [];

    private Label _valueLabel = null!;
    private LineEdit _input = null!;
    private Label _status = null!;

    public ExampleUI(INoteService noteService, SaveService saveService, NewGameService newGameService)
    {
        _noteService = noteService;
        _saveService = saveService;
        _newGameService = newGameService;
    }

    public void AddObserver(IExampleUIObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // CenterContainer keeps the whole column centered on screen at any resolution.
        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var box = new VBoxContainer { CustomMinimumSize = new Vector2(360, 0) };
        box.AddThemeConstantOverride("separation", 12);
        center.AddChild(box);

        box.AddChild(new Label
        {
            Text = "Save + observer demo",
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        // Reflects the state; updated via OnTextChanged (the observer callback).
        _valueLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        box.AddChild(_valueLabel);

        _input = new LineEdit
        {
            PlaceholderText = "Digits only…",
            VirtualKeyboardType = LineEdit.VirtualKeyboardTypeEnum.Number,
        };
        _input.TextChanged += OnInputChanged; // keep the field numeric
        box.AddChild(_input);

        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 8);
        box.AddChild(buttons);

        buttons.AddChild(MakeButton("Random", OnRandomPressed));
        buttons.AddChild(MakeButton("Save", OnSavePressed));
        buttons.AddChild(MakeButton("Load", OnLoadPressed));
        buttons.AddChild(MakeButton("New", OnNewPressed));
        buttons.AddChild(MakeButton("Quit", OnQuitPressed));

        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        box.AddChild(_status);

        SubscribeAndRender();
    }

    private static Button MakeButton(string text, System.Action onPressed)
    {
        var button = new Button { Text = text, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        button.Pressed += onPressed;

        return button;
    }

    // LineEdit has no built-in numeric-only mode, so strip non-digits as the user types.
    private void OnInputChanged(string newText)
    {
        var digits = new string(newText.Where(char.IsDigit).ToArray());

        if (digits == newText)
        {
            return;
        }

        var caret = _input.CaretColumn - (newText.Length - digits.Length);
        _input.Text = digits;
        _input.CaretColumn = Mathf.Clamp(caret, 0, digits.Length);
    }

    private void OnRandomPressed()
    {
        _input.Text = _noteService.RandomNumber().ToString();
        _status.Text = "Random number generated (press Save to store it)";
    }

    private void OnSavePressed()
    {
        _noteService.SetText(_input.Text);
        _saveService.Save();
        _status.Text = "Saved";
    }

    private void OnLoadPressed()
    {
        if (_saveService.Load())
        {
            // Load replaces the state object, so re-subscribe to the fresh one and re-render.
            SubscribeAndRender();
            _status.Text = "Loaded";
        }
        else
        {
            _status.Text = "No save found";
        }
    }

    private void OnNewPressed()
    {
        _newGameService.Create();
        // New game also replaces the state object — re-subscribe and re-render.
        SubscribeAndRender();
        _status.Text = "New game";
    }

    private void OnQuitPressed()
    {
        // Navigation is the level's job: report the intent, let it decide.
        foreach (var observer in _observers)
        {
            observer.OnQuitRequested();
        }
    }

    // INoteStateObserver — called by the state whenever its text changes.
    public void OnTextChanged(string text)
    {
        _valueLabel.Text = $"Current value: {text}";
        _input.Text = text; // reflect loaded/changed state in the field
    }

    private void SubscribeAndRender()
    {
        _noteService.Subscribe(this);
        OnTextChanged(_noteService.Get());
    }
}
