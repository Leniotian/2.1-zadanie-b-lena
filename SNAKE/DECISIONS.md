1. Wybrano platformę .NET 8 WPF (net8.0-windows) bez zewnętrznych pakietów NuGet, odrzucając biblioteki gier trzecich (MonoGame, Raylib-CSharp, Avalonia) dla zapewnienia czystej, natywnej kompilacji na Windows.
2. Zdecydowano się na proceduralną syntezę efektów dźwiękowych w pamięci (WAV PCM) odtwarzanych przez System.Media.SoundPlayer, odrzucając pakiety zewnętrzne (np. NAudio) oraz statyczne pliki dźwiękowe z dysku.
3. Wybrano wektorowe renderowanie planszy, węża i owoców wewnątrz dedykowanego GameCanvas (OnRender DrawingContext), odrzucając budowanie planszy z setek kontrolek WPF (Borders/Images w Gridzie), które obniżałyby płynność animacji.
4. Zaimplementowano wybór rozmiaru planszy jako liczbę płytek (Mała 10x10, Klasyczna 16x16, Przestronna 22x22) z automatycznym skalowaniem pikseli do okna, odrzucając sztywny rozmiar płytki w pikselach powodujący obcinanie widoku.
5. Wprowadzono 5 trybów prędkości (Leniwiec 165ms, Jeleń 115ms, Sokół 75ms, Żmija 48ms, Dynamiczna przyspieszająca z owocami), odrzucając pojedynczy suwak prędkości bez predefiniowanych poziomów.
6. Tryb ścian zdefiniowano jako wybór pomiędzy „Gęsty Las” (śmierć przy krawędzi) a „Magiczny Portal” (przenikanie na przeciwległą stronę), odrzucając natychmiastowe zatrzymywanie węża lub odbijanie od ściany.
7. Dodano 4 dodatkowe warianty rozgrywki (Dwa Jabłka, Złoty Żołądź, Omszałe Przeszkody, Spokojny Spacer), odrzucając tryb kurczącej się areny oraz tryb z zatrutymi owocami.
8. Zaprojektowano 6 unikalnych leśnych skórek węża z indywidualnymi wzorami (cętkowane, pasiaste, diamentowe) i paletami, odrzucając jednolite jednokolorowe wypełnienia.
9. Wprowadzono animowane oczy węża w stylu Google Snake (kierujące wzrok w stronę ruchu i zmieniające się w krzyżyki po śmierci), odrzucając bezoką głowę lub statyczne oczy.
10. Dodano animację wysuwającego się języka węża podczas ruchu, odrzucając nieruchomą grafikę głowy.
11. Stworzono aktywny podgląd skórki (SkinPreviewCanvas) z wijącym się wężem w menu, odrzucając statyczne ikony lub kafelki z próbkami kolorów.
12. Zaimplementowano dwupoziomowy bufor kolejki wejścia klawiatury (input queue), odrzucając pojedynczą zmienną kierunku, która powodowała kolizję ze sobą przy szybkim wciśnięciu dwóch klawiszy.
13. Zdefiniowano cel gry jako zebranie owoców do zapełnienia planszy (maksimum = liczba płytek - długość startowa - przeszkody) z dedykowanym ekranem Zwycięstwa, odrzucając grę bezwarunkowo nieskończoną.
14. Wprowadzono system mnożnika combo (x2, x3) za szybkie zbieranie owoców z unoszącymi się napisami punktów, odrzucając płaskie, niezmienne punkty bez bonusu za płynną grę.
15. Statystyki i rekordy zapisywane są lokalnie w pliku JSON (highscore.json) za pomocą System.Text.Json, odrzucając Rejestr Windows oraz bazę SQLite.
16. Zastosowano leśną estetykę z animowanymi świetlikami (FireflyCanvas), odrzucając surowy, standardowy interfejs okienkowy Windows.
17. Wbudowano zautomatyzowane testy jednostkowe pod flagą wiersza poleceń (--test), odrzucając tworzenie osobnego, ciężkiego projektu testowego.
18. Zorganizowano projekt w dedykowanym folderze SNAKE, odrzucając pozostawienie wszystkich plików źródłowych bezpośrednio w katalogu nadrzędnym.
19. Zaimplementowano płynną interpolację ruchu węża (sub-tile smooth interpolation) z pętlą CompositionTarget.Rendering i akumulatorem czasu, odrzucając skokowy ruch z płytki na płytkę sterowany wolnym DispatcherTimerem.
