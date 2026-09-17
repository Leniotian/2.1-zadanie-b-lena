# SNAKE - Leśna Przygoda (Google Snake Forest Edition)

Gra wzorowana na klasycznej grze **Google Snake**, osadzona w przytulnym, leśnym klimacie leśnej puszczy. Aplikacja została zbudowana w języku **C#** na platformie **.NET 8** z wykorzystaniem **WPF (Windows Presentation Foundation)** dla systemu Windows.

Zgodnie z wymaganiami projekt **nie korzysta z żadnych zewnętrznych pakietów NuGet** – cała grafika, animacje, zapisywanie stanu oraz proceduralna synteza dźwięków opierają się wyłącznie na natywnych bibliotekach platformy .NET.

---

## Jak uruchomić aplikację

### Wymagania wstępne
- System operacyjny: **Windows 10 / 11**
- Zainstalowane środowisko **.NET 8 SDK** (lub nowsze, np. .NET 9 SDK ze wsparciem dla aplikacji WPF na Windows Desktop).

### Uruchomienie krok po kroku

1. Otwórz terminal (PowerShell, CMD lub wbudowany terminal IDE) w katalogu projektu `SNAKE`:
   ```powershell
   cd c:\Users\bobryk.lena\Desktop\2.1-zadanie-b-lena\SNAKE
   ```

2. Zbuduj aplikację:
   ```powershell
   dotnet build
   ```

3. Uruchom grę:
   ```powershell
   dotnet run
   ```

4. *(Opcjonalnie)* Uruchomienie wbudowanych zautomatyzowanych testów silnika:
   ```powershell
   dotnet run -- --test
   ```

---

## Sterowanie w grze

| Klawisz | Akcja |
| :--- | :--- |
| **Strzałki (↑ ↓ ← →)** / **W, A, S, D** | Zmiana kierunku ruchu węża |
| **Spacja (Space)** | Pauza / Wznowienie rozgrywki / Start |
| **R** | Błyskawiczny restart z aktualnymi ustawieniami |
| **M** | Wyciszenie / Włączenie dźwięków |
| **Esc** | Powrót do menu głównego z konfiguracją |

---

## Zaimplementowane funkcje i tryby

### 1. Rozgrywka w stylu Google Snake
- **Szachownica łąkowa**: Naprzemienne kafelki w odcieniach leśnej zieleni i mchu.
- **Ekspresyjne oczy**: Oczy węża dynamicznie spoglądają w stronę, w którą aktualnie się porusza; po przegranej zmieniają się w krzyżyki.
- **Animowany język**: Wąż okresowo wysuwa rozwidlony języczek podczas pełzania.
- **Podwójne buforowanie klawiszy**: Kolejka wejścia uniemożliwia przypadkową śmierć przy szybkich zakrętach (np. góra + prawo w jednym ticku).

### 2. Wybór rozmiaru planszy / wielkości płytek
- **Duże Płytki (10x10)** – mała plansza, duże kafelki, bardzo dynamiczna rozgrywka.
- **Klasyczna (16x16)** – zbalansowana standardowa plansza znana z Google Snake.
- **Przestronna (22x22)** – wielka leśna polana z mniejszymi kafelkami do długich partii.
- *Rozmiar płytek skaluje się w pełni automatycznie do rozmiaru okna.*

### 3. Regulacja prędkości
- **Leniwiec (Wolno)** – 165 ms na krok.
- **Jeleń (Normalnie)** – 115 ms na krok (domyślny).
- **Sokół (Szybko)** – 75 ms na krok.
- **Żmija (Ekstremalnie)** – 48 ms na krok dla mistrzów refleksu.
- **Dynamiczna** – wąż automatycznie przyspiesza co kilka zjedzonych owoców.

### 4. Tryby ścian i warianty specjalne
- **Ściany**:
  - **Gęsty Las (Solid)**: Kolizja ze skrajem planszy kończy grę.
  - **Magiczny Portal (Portal)**: Wąż swobodnie przenika przez krawędzie na przeciwległą stronę.
- **Warianty gry**:
  - **Klasyczny**: 1 czerwone jabłko na planszy.
  - **Dwa Jabłka**: Dwa owoce aktywne równocześnie na planszy.
  - **Złoty Żołądź**: Bonusowy żołądź z pulsującą poświatą, pojawiający się na określony czas za 35 punktów.
  - **Głazy i Pnie**: Omszałe przeszkody leśne rozsiane po planszy, w które nie wolno uderzyć.
  - **Spokojny Spacer**: Tryb relaksacyjny – wąż nie ginie od ścian ani od własnego ciała.

### 5. Cel gry i maksymalna liczba owoców
- Cel to zjadanie owoców – maksymalna liczba owoców do zebrania to liczba wszystkich pól na planszy pomniejszona o startowe ciało węża i ewentualne przeszkody (`Szerokość * Wysokość - 3 - Przeszkody`).
- Wskaźnik na górnym pasku stale prezentuje postęp: `OWOCE / MAX`.
- Zebranie maksymalnej liczby owoców lub wypełnienie planszy nagradzane jest specjalnym ekranem: **KRÓL LEŚNEJ PUSZCZY! ZWYCIĘSTWO!** wraz z uroczystą fanfarą.

### 6. System punktowy i statystyki
- **Jabłko**: 10 pkt.
- **Leśna jagoda**: 20 pkt.
- **Złoty żołądź**: 35 pkt.
- **System Combo**: Zjedzenie owocu w czasie poniżej 5 sekund od poprzedniego aktywuje mnożnik punktów (`x2`, `x3`) z unoszącymi się animowanymi napisami (np. `+20 (x2!)`).
- Zapisywanie rekordu punktowego, łącznej liczby owoców, rozegranych partii i zwycięstw w pliku `highscore.json`.

### 7. Unikalne leśne skórki węża (Skins)
W menu dostępny jest interaktywny, wijący się na żywo podgląd węża (`SkinPreviewCanvas`). Dostępne skórki:
1. **Leśny Zaskroniec** – soczysta leśna zieleń z cętkami i jasnym brzuchem.
2. **Jesienny Szelest** – ogniste barwy klonu i dębu z diamentowym wzorem łusek.
3. **Nocny Błysk** – głęboki granat z bioluminescencyjnymi pasami i neonowymi ślepiami.
4. **Złota Żmija** – lśniące złote łuski i szlachetne rubinowe oczy.
5. **Leśny Muchomor** – intensywna czerwień usiana białymi zarodnikami.
6. **Gęsty Mech** – kamuflaż z mchu i kory dębowej.

### 8. Efekty dźwiękowe (Procedural Audio)
Dźwięki są generowane proceduralnie w pamięci jako strumienie bajtów WAV PCM:
- Chrupnięcie owocu (częstotliwość wznosząca z obwiednią)
- Magiczny dwutonowy dzwonek przy złotym żołędziu
- Delikatny stukot gałązki przy skręcie
- Basowy dźwięk końca gry
- Zwycięska 4-tonowa fanfara arpeggio
- Kliknięcie przycisków w menu

---

## Czego nie udało się zrealizować / Ograniczenia i przyszłe usprawnienia

1. **Ciągła muzyka tła (Ambient Forest OST)**:
   - Zgodnie z wytycznymi nie dodano zewnętrznych bibliotek NuGet (np. NAudio czy BASS). Standardowy odtwarzacz systemu Windows (`System.Media.SoundPlayer`) potrafi odtwarzać w danym momencie tylko jeden strumień audio. Odtwarzanie ciągłej muzyki w tle blokowałoby lub przerywało krótkie efekty dźwiękowe zjadania jabłek i skrętów. Wybrano zachowanie responsywnych, proceduralnych efektów dźwiękowych kosztem ciągłego podkładu muzycznego.
2. **Zaawansowana fizyka kręgosłupa węża (Inverse Kinematics / Spine Curves)**:
   - Zastosowano styl geometryczny Google Snake (zaokrąglone segmenty łączone prostokątnymi mostkami na zakrętach), zamiast w pełni płynnej, bezkratkowej fizyki ugięcia mięśniowego.
3. **Tryb dla dwóch graczy na jednym ekranie (Local Co-op / Versus)**:
   - Gra skupia się na dopracowanym trybie dla jednego gracza z 5 wariantami rozgrywki; nie zaimplementowano drugiego węża sterowanego np. klawiszami IJKL.
