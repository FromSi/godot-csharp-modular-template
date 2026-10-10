using Game.Game.Common.Service;
using Game.Game.MainMenu.UI.Observer;
using System;
using System.Collections.Generic;
using Godot;

namespace Game.Game.MainMenu.UI;

/// <summary>
/// Start menu: a centered column with the title and Continue / New Game / Settings / Quit.
/// Continue is inactive without a save that loads; a failed save is reported under the buttons.
/// Choices are raised to the level via <see cref="IMainMenuUIObserver"/>; the UI itself neither
/// loads nor navigates.
/// </summary>
public partial class MainMenuUI : Control
{
    private readonly SaveService _saveService;
    private readonly List<IMainMenuUIObserver> _observers = [];

    private Button _continueButton = null!;
    private Label _saveFailed = null!;

    public MainMenuUI(SaveService saveService)
    {
        _saveService = saveService;
    }

    public void AddObserver(IMainMenuUIObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var column = new VBoxContainer { CustomMinimumSize = new Vector2(320, 0) };
        column.AddThemeConstantOverride("separation", 12);
        center.AddChild(column);

        var title = new Label { Text = "Game", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 64);
        column.AddChild(title);

        _continueButton = CreateButton("Continue", observer => observer.OnContinuePressed());
        column.AddChild(_continueButton);
        column.AddChild(CreateButton("New Game", observer => observer.OnNewGamePressed()));
        column.AddChild(CreateButton("Settings", observer => observer.OnSettingsPressed()));
        column.AddChild(CreateButton("Quit", observer => observer.OnQuitPressed()));

        _saveFailed = new Label
        {
            Text = "Could not save the game",
            Modulate = new Color(1, 0.45f, 0.45f),
            HorizontalAlignment = HorizontalAlignment.Center,
            Visible = false,
        };
        column.AddChild(_saveFailed);

        RefreshContinue();
    }

    // Continue is available once there is a save that loads (e.g. after returning from the game).
    public void RefreshContinue()
    {
        _continueButton.Disabled = !_saveService.CanLoad();
    }

    // Shown after the player left the game without saving; hidden by the next good save.
    public void ShowSaveFailed(bool isFailed)
    {
        _saveFailed.Visible = isFailed;
    }

    private Button CreateButton(string text, Action<IMainMenuUIObserver> notify)
    {
        var button = new Button { Text = text };
        button.AddThemeFontSizeOverride("font_size", 28);

        button.Pressed += () =>
        {
            foreach (var observer in _observers)
            {
                notify(observer);
            }
        };

        return button;
    }
}
