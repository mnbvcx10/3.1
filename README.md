# Minutnik – Zadanie 3.1

Aplikacja minutnika (Pomodoro) napisana w technologii **Avalonia .NET (C#)** zgodnie z wytycznymi z lekcji *Minutnik: siedem zasad w jednej wiadomości*.

---

## 1. Spełnienie siedmiu zasad

1. **Czas pracy (25 min) i przerwy (5 min) do zmiany w oknie**:
   - W oknie znajdują się dwa pola tekstowe (`WorkMinutesInput` oraz `BreakMinutesInput`), pozwalające użytkownikowi na wpisanie dowolnej liczby minut pracy i przerwy.
2. **Jeden przycisk start/pauza, drugi reset**:
   - Przycisk `StartPauseButton` dynamicznie przełącza stan odliczania (Start ↔ Pauza).
   - Przycisk `ResetButton` zatrzymuje odliczanie i przywraca czas początkowy.
3. **Widoczny licznik skończonych cykli**:
   - Etykieta `CyclesDisplay` na bieżąco prezentuje liczbę ukończonych cykli pracy.
4. **Po skończonej pracy przerwa startuje sama**:
   - Po upływie czasu sesji pracy (`_remainingSeconds <= 0`), licznik cykli jest zwiększany o 1, tryb automatycznie przełącza się na przerwę (`SessionTypeDisplay.Text = "Przerwa"`), ładowany jest czas przerwy i odliczanie trwa nadal samoczynnie bez konieczności ponownego klikania Start.
5. **Czas pokazywany jako mm:ss**:
   - Wartość wyświetlana na głównym ekranie jest formatowana jako `{minutes:D2}:{seconds:D2}` (np. `25:00`, `04:59`).
6. **Interfejs po polsku, nazwy w kodzie po angielsku**:
   - Etykiety i komunikaty w oknie są całkowicie w języku polskim (*Minutnik Pomodoro*, *Czas pracy*, *Czas przerwy*, *Start*, *Pauza*, *Reset*, *Ukończone cykle*, *Praca*, *Przerwa*, *Gotowy*).
   - Wszystkie identyfikatory, metody i pola w kodzie C# (`MainWindow.axaml.cs`) są w języku angielskim (`_timer`, `_cycles`, `_history`, `Timer_Tick`, `StartPauseButton_Click`, itd.).
7. **Brak dodatkowych bibliotek i cała logika w MainWindow.axaml.cs**:
   - Użyto wyłącznie wbudowanego `Avalonia.Threading.DispatcherTimer` (co eliminuje błąd wielowątkowości `Call from invalid thread`), bez zewnętrznych bibliotek i bez osobnych klas pomocniczych.
8. **Własną ręką: historia cykli (`ListBox`)**:
   - Zgodnie z blokiem ćwiczenia dodano kontrolkę `<ListBox Name="HistoryList" />` oraz pole `private readonly List<string> _history = new();`. Po każdym ukończonym cyklu pracy dodawany jest wpis z aktualną godziną (np. `15:14 — cykl 1`) i przekazywany do widoku.

---

## 2. Cztery liczby z lekcji

- **Data pomiaru**: 3 listopada 2026 r.
- **Narzędzie / Model**: Antigravity (Google DeepMind) / model **Gemini 3.8 Flash (High)**

| Lp. | Metryka / Pytanie | Wartość | Źródło / Link |
|:---:|:---|:---:|:---|
| 1 | **Liczba tokenów pierwszej wiadomości po polsku** | **138 tokenów** (dla promptu z 7 zasadami / 466 znaków) <br> *(cała wklejona treść lekcji: 1 685 tokenów / 6 820 znaków)* | Pomiar Tokenizer Playground / OpenAI tokenizer (cl100k_base) |
| 2 | **Liczba tokenów pierwszej wiadomości po angielsku** | **68 tokenów** (dla przetłumaczonego promptu / 396 znaków) <br> *(cała przetłumaczona lekcja: 875 tokenów)* | Pomiar Tokenizer Playground / OpenAI tokenizer (cl100k_base) |
| 3 | **Okno kontekstu modelu (Context Window)** | **1 048 576 tokenów** (~1 000 000 tokenów) | [Google Gemini Models Documentation](https://ai.google.dev/gemini-api/docs/models/gemini) |
| 4 | **Maksymalna długość odpowiedzi (Max Output Tokens)** | **65 536 tokenów** (~64 000 tokenów) | [Google Gemini Models Documentation](https://ai.google.dev/gemini-api/docs/models/gemini) |

> **Wniosek dotyczący tokenizacji:**  
> Ta sama instrukcja w języku polskim zajmuje ponad **dwukrotnie więcej tokenów** niż po angielsku (138 tokenów vs 68 tokenów). Wynika to ze specyfiki tokenizerów opartych na podziałach BPE trenowanych głównie na tekstach anglojęzycznych – polskie słowa z polskimi znakami diakrytycznymi (ą, ć, ę, ł, ń, ó, ś, ź, ż) oraz bogatą fleksją są dzielone na mniejsze fragmenty (często 2–3 tokeny na jedno słowo).

---

## 3. Wynik próby wpisania wartości ułamkowych (`25,5` oraz `25.5`)

W aplikacji czas pracy i czas przerwy są odczytywane z pól `TextBox` za pomocą metody:
```csharp
private int GetWorkMinutes()
{
    if (int.TryParse(WorkMinutesInput.Text, out int minutes) && minutes > 0)
    {
        return minutes;
    }
    return 25;
}
```

### Próba 1: Wpisanie `25,5` w pole długości
- **Co się stało:**  
  `int.TryParse("25,5", out int minutes)` zwróciło `false`, ponieważ znak przecinka `,` nie jest poprawnym znakiem liczby całkowitej w formacie `int`.
- **Skutek w aplikacji:**  
  Aplikacja nie uległa awarii (brak wyjątku `System.FormatException`), a metoda bezpiecznie przyjęła domyślną wartość **25 minut**. Na wyświetlaczu minutnika pozostało `25:00`. Gdyby użyto naiwnego `int.Parse("25,5")`, program natychmiast zgłosiłby nieobsłużony wyjątek `FormatException` i zamknąłby okno.

### Próba 2: Wpisanie `25.5` w pole długości
- **Co się stało:**  
  `int.TryParse("25.5", out int minutes)` również zwróciło `false`, ponieważ kropka `.` nie jest częścią zapisu liczby całkowitej typu `int`.
- **Skutek w aplikacji:**  
  Aplikacja nie zgłosiła błędu, zachowała stabilność i powróciła do domyślnej wartości **25 minut** (`25:00`).

### Uwaga o kulturze językowej (`pl-PL` vs `en-US`):
W przypadku implementacji opartej na liczbach zmiennoprzecinkowych (`double.Parse`):
- W polskim systemie Windows (kultura `pl-PL`) separatorem dziesiętnym jest przecinek `,`. Zapis `25,5` zostałby sparsowany poprawnie jako 25.5 minuty (25 minut i 30 sekund), natomiast `25.5` rzuciłby wyjątek `FormatException`.
- Zastosowanie bezpiecznego parsowania `int.TryParse` / `double.TryParse` chroni użytkownika przed awarią aplikacji niezależnie od wpisanego separatora.

---

## 4. Uruchomienie projektu

W katalogu projektu:
```bash
cd minutnik
dotnet run
```
