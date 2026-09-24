# Mój pierwszy agent — karta obserwacji

- Narzędzie i model: Gemini 3.8 FLASH
- Moje zadanie (2–3 zdania): Stworz pierwsza desktopowa aplikacje
- Pierwsza wiadomość (wklejona co do znaku): 
Build an application that works like "SNAKE"
- C# on .NET 8, WPF, Windows.
- Create the project in the current directory (it is already created and
  empty). The project name is SNAKE.
- No external NuGet packages unless you decide one is truly necessary. If you
  add one, record why in DECISIONS.md.

REQUIREMENTS:
1. The app works like snake found in google. 
2. You can select from multiple modes:
	a. You can select the tile size,
	b. You can adjust the speed
	c. There is a wall mode, and many others (you can choose)
3. The goal is to collect fruits, the max number of fruits is the max number of tiles
4. There is a menu, the entire games is in a foresty vibe. 
5. A simple point system

Rules:
- Do not ask questions. When something is unspecified, decide yourself and
  record the decision in DECISIONS.md in the project directory — one line per
  decision, each naming the alternative you rejected.
- At the end, write README.md in Polish: how to run the application, and what
  you did not manage to do.

- Co agent zrobił najpierw: Najpierw agent, zrobił plan działania i kazał mi go zatwierdzic.
- O co pytał — i co zostało zatwierdzone bez czytania: Pytał czy może korzystac z komend dotnet oraz zapytał o przejrzenie planu działania. Zgodziłam się na obydwa.
- Pierwszy błąd i co agent z nim zrobił: Nie zrobił folderu na wszystkie pliki związane ze snake. Oprócz tego gra nie jest płynna
- Stan po 25 minutach: działa / częściowo / nie działa / nie wiem: Aplikacja działa
- Skąd wiem, że aplikacja działa (co zostało sprawdzone): zostalo wykonane dotnet run -- --test i wszystko jest ok. 
- Rzeczy, które agent zrobił, a których nie rozumiem: --
- Jak mi się wydawało, że poszło (jedno zdanie): Myśle, że zadanie było dośc proste, jedyny problem mógłbyć z połączeniem githuba