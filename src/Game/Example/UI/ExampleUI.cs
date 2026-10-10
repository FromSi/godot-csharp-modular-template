using Game.Game.Example.Domain.Observer;
using Game.Game.Example.Service;
using Game.Game.Example.UI.Observer;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game.Game.Example.UI;

/// <summary>
/// Example screen. Demonstrates three things: the save system, the observer pattern
/// (state → UI), and UI → Level notifications.
///
/// Type a number, press <c>Random</c> to fill it and <c>Save</c> to persist. Feature actions
/// call the services directly; saving and navigation (<c>Save</c>, <c>Menu</c>) are raised to the
/// level via <see cref="IExampleUIObserver"/>, which reports the result back through
/// <see cref="ShowSaveResult"/> — the UI holds no navigation or save logic. The field and the
/// "Current value" label (with the time of the last change) are driven by
/// <see cref="INoteStateObserver.OnTextChanged"/>.
///
/// Load / New Game live in the main menu: the screen hosting this UI is created after them, so
/// <see cref="_Ready"/> subscribes to the current state once — no re-subscribing on load.
/// </summary>
public partial class ExampleUI : Control, INoteStateObserver
{
    private static readonly Color ErrorColor = new(1, 0.45f, 0.45f);

    private readonly INoteService _noteService;

    // UI is the publisher here; the level subscribes. It's a Node, so it can't extend
    // Common's ObservableState<T> — a manual list, like GameLevel's ILevelObserver.
    private readonly List<IExampleUIObserver> _observers = [];

    private Label _valueLabel = null!;
    private LineEdit _input = null!;
    private Label _status = null!;

    public ExampleUI(INoteService noteService)
    {
        _noteService = noteService;
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
        buttons.AddChild(MakeButton("Menu", OnMenuPressed));

        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        box.AddChild(_status);

        _noteService.Subscribe(this);
        Render(_noteService.Get(), _noteService.GetChangedAt());
    }

    // The screen is freed on the way back to the menu; don't leave a dead observer on the state.
    public override void _ExitTree()
    {
        _noteService.Unsubscribe(this);
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

        foreach (var observer in _observers)
        {
            observer.OnSaveRequested();
        }
    }

    // Called by the level after every save (button, Esc): "Saved", or a warning that stays until
    // the next good save.
    public void ShowSaveResult(bool isSaved)
    {
        _status.Text = isSaved ? "Saved" : "Could not save the game";
        _status.Modulate = isSaved ? Colors.White : ErrorColor;
    }

    private void OnMenuPressed()
    {
        // Navigation is the level's job: report the intent, let it decide.
        foreach (var observer in _observers)
        {
            observer.OnMenuRequested();
        }
    }

    // INoteStateObserver — called by the state whenever its text changes.
    public void OnTextChanged(string text, DateTime changedAt)
    {
        Render(text, changedAt);
    }

    private void Render(string text, DateTime? changedAt)
    {
        var changed = changedAt is { } time ? $"changed at {time:HH:mm:ss}" : "never changed";
        _valueLabel.Text = $"Current value: {text} ({changed})";
        _input.Text = text; // reflect loaded/changed state in the field
    }
}
