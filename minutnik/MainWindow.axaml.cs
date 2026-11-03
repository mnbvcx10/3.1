using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly List<string> _history = new();
    private readonly DispatcherTimer _timer;
    private int _cycles = 0;
    private int _remainingSeconds;
    private bool _isRunning = false;
    private bool _isWorkSession = true;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        ResetTimer();

        WorkMinutesInput.TextChanged += OnDurationInputChanged;
        BreakMinutesInput.TextChanged += OnDurationInputChanged;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            UpdateTimerDisplay();
        }

        if (_remainingSeconds <= 0)
        {
            if (_isWorkSession)
            {
                _cycles++;
                _history.Add($"{DateTime.Now:HH:mm} — cykl {_cycles}");
                HistoryList.ItemsSource = _history.ToArray();
                CyclesDisplay.Text = _cycles.ToString();

                // Automatyczne przejście do przerwy
                _isWorkSession = false;
                SessionTypeDisplay.Text = "Przerwa";
                StatusDisplay.Text = "Przerwa w toku";
                _remainingSeconds = GetBreakMinutes() * 60;
                UpdateTimerDisplay();
            }
            else
            {
                // Powrót do pracy
                _isWorkSession = true;
                SessionTypeDisplay.Text = "Praca";
                StatusDisplay.Text = "Praca w toku";
                _remainingSeconds = GetWorkMinutes() * 60;
                UpdateTimerDisplay();
            }
        }
    }

    private void StartPauseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
            StartPauseButton.Content = "Start";
            StatusDisplay.Text = "Wstrzymano";
        }
        else
        {
            if (_remainingSeconds <= 0)
            {
                _remainingSeconds = (_isWorkSession ? GetWorkMinutes() : GetBreakMinutes()) * 60;
                UpdateTimerDisplay();
            }

            _timer.Start();
            _isRunning = true;
            StartPauseButton.Content = "Pauza";
            StatusDisplay.Text = _isWorkSession ? "Praca w toku" : "Przerwa w toku";
        }
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        ResetTimer();
    }

    private void ResetTimer()
    {
        _timer.Stop();
        _isRunning = false;
        StartPauseButton.Content = "Start";
        _isWorkSession = true;
        SessionTypeDisplay.Text = "Praca";
        StatusDisplay.Text = "Gotowy";
        _remainingSeconds = GetWorkMinutes() * 60;
        UpdateTimerDisplay();
    }

    private void OnDurationInputChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isRunning)
        {
            _remainingSeconds = (_isWorkSession ? GetWorkMinutes() : GetBreakMinutes()) * 60;
            UpdateTimerDisplay();
        }
    }

    private int GetWorkMinutes()
    {
        if (int.TryParse(WorkMinutesInput.Text, out int minutes) && minutes > 0)
        {
            return minutes;
        }
        return 25;
    }

    private int GetBreakMinutes()
    {
        if (int.TryParse(BreakMinutesInput.Text, out int minutes) && minutes > 0)
        {
            return minutes;
        }
        return 5;
    }

    private void UpdateTimerDisplay()
    {
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        TimerDisplay.Text = $"{minutes:D2}:{seconds:D2}";
    }
}
