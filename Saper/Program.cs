using System;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;

namespace Saper {

    // Klasa Tile dla pola/kafelka, dziedziczone z klasy PictureBox
    public class Tile : PictureBox {
        public int tileSize = 32; // Wielkość grafiki 32x32
        public int x; // Pozycja od lewej, XXX
        public int y; // Pozycja od góry, XXX
        public int around = -1; // Ilość bomb w pobliżu tego pola, domyślnie -1, będzie zaktualizowane później

        public enum States { Empty, Bomb } // Statusy dla pola: Puste lub z Bombą
        public int state; // Status pola odpowiadający enum States
        public bool clicked = false; // Czy pole zostało kliknięte
        public bool flag = false; // Czy pole zostało oznaczone flagą

        Game gra; // Klasa główna Game do której będziemy mogli się odwoływać

        // Konstruktor klasy (wywoływany przy jej tworzeniu) przyjmujący argumenty: lewo, góra, stan (bomba/puste), oraz klasę Game
        public Tile(int _x, int _y, int _state, Game _gra) {
            // Przypisanie wartości zmiennym wartośćiami z argumentów funkcji
            x = _x;
            y = _y;
            state = _state;
            gra = _gra;

            Name = "Tile_" + _x + "," + y; // Nadanie identyfikatora dla pola
            Size = new Size(tileSize, tileSize); // Utworzenie kwadratowego obrazka o podanej wcześniej wielkości w zmiennej tileSize
            Cursor = Cursors.Hand; // Zmiana wyglądu kursora po najchaniu na pole
            Img(); // Ustawienie grafiki dla pola
        }

        public void Img() {
            var path = Properties.Resources.tile; // Standardowa grafika pustego kafelka

            if (!clicked && !gra.game_over && flag) { // Wyświetlanie flagi gdy: Gra trwa, nie kliknięto, oznaczono flagą
                path = Properties.Resources.flag;
            } else if (state == (int)States.Empty) { // Dla pustego pola
                if (clicked) { // Gdy został kliknięty - wyświetlenie ilości bomb w pobliżu
                    if (around == 0) path = Properties.Resources.tile_empty_0;
                    if (around == 1) path = Properties.Resources.tile_empty_1;
                    if (around == 2) path = Properties.Resources.tile_empty_2;
                    if (around == 3) path = Properties.Resources.tile_empty_3;
                    if (around == 4) path = Properties.Resources.tile_empty_4;
                    if (around == 5) path = Properties.Resources.tile_empty_5;
                    if (around == 6) path = Properties.Resources.tile_empty_6;
                    if (around == 7) path = Properties.Resources.tile_empty_7;
                    if (around == 8) path = Properties.Resources.tile_empty_8;
                } else if (flag) { // Gdy oznaczono go flagą
                    path = Properties.Resources.flag;
                }
            } else if (state == (int)States.Bomb) { // Dla pola z bombą
                if (clicked) { // Gdy kliknięto bombę - wybuch
                    path = Properties.Resources.bomb_clicked;
                } else if (flag) { // Gdy bombę oznaczono flagą
                    path = Properties.Resources.bomb_flag;
                } else if (gra.game_over) { // Po zakończeniu gry pokaż bomby
                    path = Properties.Resources.bomb;
                }
            } else { // W przypadku błędu wyświetl grafikę z pytajnikiem
                path = Properties.Resources.tile_undefined;
            }

            Image = path;
        }
    }

    // Klasa Game odpowiadająca za działanie gry, dziedzicząca z klasy Form
    public partial class Game : Form {

        int[] fields; // Tablica przechowująca tylko stan danego pola (bomba/puste)
        Tile[] tiles; // Tablica przechowująca pola
        public int game_width; // Ilość kolumn z polami
        public int game_height; // Ilość wierszy z polami
        public int game_size; // Wielkość gry (ilość kafelków)
        public int tiles_left; // Ile pól pozostało do kliknięcia
        int bomb_chance = 10; // Szansa na wystąpienie bomby (1/10)
        public bool game_over; // Czy gra została zakończona
        Random rnd = new Random(); //Generator liczb pseudolosowych
        public bool created = false;
        public bool game_win;

        public int BombsAmount () {
            int bombs_amount = 0;
            // Pętle po wszystkich polach
            for (int i = 0; i < game_width; i++) {
                for (int j = 0; j < game_height; j++) {
                    bombs_amount += getField(i, j);
                }
            }
            return bombs_amount;
        }

        void RandomBombs () {
            // Pętle po wszystkich polach
            for (int i = 0; i < game_width; i++) {
                for (int j = 0; j < game_height; j++) {
                    int state;
                    if (rnd.Next(0, bomb_chance - 1) > 0) { // Losowanie czy na danym polu jest bomba, 
                        state = (int)Tile.States.Empty;  //jeżeli wylosowano liczbę większą od 0 - nie ma
                        tiles_left++; // Zwiększenie stanu licznika
                    } else {
                        state = (int)Tile.States.Bomb; // Ustawienie statusu bomby
                    }
                    setField(i, j, state); // Ustawienie statusu pola w tablicy
                }
            }
        }

        void SetBombs(int[] bombs) {
            for (int i = 0; i < game_width; i++) {
                for (int j = 0; j < game_height; j++) {
                    int state;
                    if (bombs[i * game_width + j] == 0) { // Przypisanie wartości z pola
                        state = (int)Tile.States.Empty; 
                        tiles_left++; // Zwiększenie stanu licznika
                    } else {
                        state = (int)Tile.States.Bomb; // Ustawienie statusu bomby
                    }
                    setField(i, j, state); // Ustawienie statusu pola w tablicy
                }
            }
        }

        // Funkcja tworząca nową grę (standardowo o rozmiarze 10x10)
        public void NewGame(int width = 10, int height = 10, int[] bombs = null) {
            stan.Text = "Powodzenia!"; // Zmiana tekstu na powodzenia
            game_over = false; // Gra nie jest zakończona
            game_width = width; // Przypisanie szerokości gry z argumentu (ilości pól w poziomie)
            game_height = height; // Przypisanie szerokości gry z wysokości (ilości pól w pionie)
            game_size = width * height; // Ilość pól w całej grze (poziom * pion)
            fields = new int[game_size]; // Utworzenie tablicy na statusy pól
            tiles_left = 0; // Licznik pozostałych pól 
            game_win = false;

            // Wylosowanie bomb
            if (bombs == null) {
                RandomBombs();
            } else {
                SetBombs(bombs);
            }

            // Wyczyszczenie tabeli z polami i jej stylów
            mines.Controls.Clear();
            mines.RowStyles.Clear();
            mines.ColumnStyles.Clear();

            // Ustawienie wielkośći tabeli
            mines.ColumnCount = game_width;
            mines.RowCount = game_height;

            // Utworzenie tablicy z polami
            tiles = new Tile[game_size];

            // Pętle po wszystkich polach
            for (int i = 0; i < game_width; i++) {
                for (int j = 0; j < game_height; j++) {
                    int value = getField(i, j); // Pobranie wylosowanego wcześniej statusu pola (bomba/puste)
                    Tile tile = new Tile(i, j, value, this); // Utworzenie pola z klasy Tile
                    tile.Click += new System.EventHandler(this.TileOnClick); // Przypisanie funkcji do kliknięcia na pole
                    mines.Controls.Add(tile); // Dodanie pola do tabeli
                    tile.around = TilesAround(tile.x, tile.y); // Przeliczenie ile pól wokół ma bomby
                    tiles[i * game_width + j] = tile; // Przypisanie pola na odpowiednie miejsce w tablicy z polami
                }
            }

            // Oznaczenie, że gra została utworzona
            created = true;
        }

        public int getField(int i, int j) { // Pobranie zawartości pola
            // Jeżeli podano niestniejące pole (wychodzące poza ramy gry) zwróć 0
            if (i < 0 || i >= game_width || j < 0 || j >= game_height) return 0;
            return fields[i * game_width + j]; // Zwróć wartość pola
        }

        public void setField(int i, int j, int state) { // Ustawienie statusu pola
            fields[i * game_width + j] = state;
        }

        public Tile GetTile (int i, int j) {
            return tiles[i * game_width + j];
        }


        void TileOnClick(object sender, EventArgs e) { // Funkcja wykonywana po kliknięciu na pole
            if (game_over) return; // Jeżeli gra została już zakończona to przerwnij wykonywanie funkcji
            Tile tile = (sender as Tile); // Pobranie klikniętego pola

            // Przekonwertowanie klasy EventArgs na MouseEventArgs, aby można było sprawdzić użycie prawego przycisku myszy
            MouseEventArgs me = (MouseEventArgs)e; 
            if (me.Button == System.Windows.Forms.MouseButtons.Right) { // Sprawdzenie czy użyto prawy przycisk myszy
                tile.flag = !tile.flag; // Zmiana statusu flagi na odwrotny
                tile.Img(); // Odświeżenie grafiki pola
            } else {
                TileClick(tile, true); // Funkcja pomocnicza po kliknięciu na pole
            }
        }
        public void TileClick(Tile tile, bool user_click) { // Funkcja pomocnicza po kliknięciu na pole
            if (tile.clicked || game_over) return; // Jeżeli pole zostało już kliknięte lub gra została zakończna - nic nie rób
            if (user_click && tile.flag) return; // Jeżeli na polu jest flaga nic nie rób
            tile.clicked = true; // Oznacz pole jako naciśnięte
            tile.Img(); // Odśwież grafikę pola

            if (tile.state == (int)Tile.States.Bomb) { // Jeżeli na polu znajdowała się bomba to zakończ grę
                GameOver();
                return;
            }

            TileMinus(); // Odejmij puste pole od licznika

            if (tile.around == 0) { // Jeżeli naciśnięto pole które nie ma bomb wokół,
                TilesAroundClick(tile.x, tile.y); // wtedy nacisnij pola wokół
            }
        }

        void TilesAroundClick(int x, int y) { // Funkcja naciskająca pola wokół
            ClickField(x, y - 1); // Pole na górze
            ClickField(x - 1, y); // Pole na lewo
            ClickField(x + 1, y); // Pole na dole
            ClickField(x, y + 1); // Pole na prawo
        }

        public void ClickField(int i, int j) { // Funkcja naciskająca dane pole
            // Jeżeli podano niestniejące pole (wychodzące poza ramy gry) przerwij wykonywanie funkcji
            if (i < 0 || i >= game_width || j < 0 || j >= game_height) return;
            Tile tile = tiles[i * game_width + j]; // Pobierz dane pole do zmiennej
            if (tile.state == (int)Tile.States.Empty) { // Jeżeli pole jest puste
                if (tile.around == 0) { // Jeżeli naciśnięto pole które nie ma bomb wokół,
                    TileClick(tile, false); // Wtedy naciśnij to pole
                } else if (!tile.clicked) { // Jeżeli pole nie zostało wcześniej naciśnięte
                    TileMinus();// Odejmij puste pole od licznika
                    tile.clicked = true; // Oznacz pole jako naciśnięte
                    tile.Img(); // Odśwież grafikę pola
                }
            }
        }

        void TileMinus() { // Funkcja odemująca pozostałe pola
            tiles_left--; // Odjęcie jednego pola
            stan.Text = tiles_left + " pozostało"; // Wyświetlenie ile pól pozostało
            if (tiles_left == 0) { // Jeżeli pozostało 0 pól oznacza, że gra została wygrana
                GameWin(); // więc wywołanie funkcji odpowiadającej za wygraną
            }
        }

        public int TilesAround(int x, int y) { // Funkcja sprawdzająca ile pól wokół posiada bomby
            int around = 0;
            // Korzystając z faktu, że puste pole ma wartość 0, a z bombą 1 - dodajemy wartość pola do licznika around
            around += getField(x - 1, y - 1); // Pole na lewo w górę
            around += getField(x, y - 1); // Pole na lewo
            around += getField(x + 1, y - 1); // Pole na prawo w górę
            around += getField(x - 1, y); // itd.
            around += getField(x + 1, y);
            around += getField(x - 1, y + 1);
            around += getField(x, y + 1);
            around += getField(x + 1, y + 1);
            return around; // Zwrócenie licznika
        }

        void GameEnd(string tekst, bool win) { // Funkcja kończąca grę
            stan.Text = tekst; // Zmiana tekstu o statusie gry
            game_over = true; // Ustawiamy stan gry jako zakończony
            game_win = win; // Ustawienei stanu gry 

            //Pętla po wszystkich polach
            for (int n = 0; n < game_size; n++) {
                var tile = tiles[n]; // Pobranie n-tego pola
                tile.Img(); // Odświeżenie grafiki każdego pola po zakończniu gry
            }
        }
        void GameWin() { // Funkcja wywoływana kiedy przegramy grę
            GameEnd("Wygrałeś! B)", true);
        }
        void GameOver() { // Funkcja wywoływana kiedy przegramy grę
            GameEnd("Przegrałeś! :(", false);
        }

        void ButtonNewGame(object sender, EventArgs e) { //Funkcja wywoływana po naciśnięciu przycisku nowej gry
            NewGame(); // Wywołanie funkcji utworzenia nowej gry
        }

        //Konstruktor klasy Game, wywoływany przy tworzeniu
        public Game() {
            InitializeComponent(); // Wywołanie funkcji z projektanta formularzy
            NewGame(); // Wywołanie funkcji utworzenia nowej gry
        }
    }

    // Ta część kodu została wygenerowana przez Projektanta formularzy systemu Windows i odpowiada za rysowanie okna
    partial class Game {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent() {
            this.button_newgame = new System.Windows.Forms.Button();
            this.stan = new System.Windows.Forms.Label();
            this.serviceController1 = new System.ServiceProcess.ServiceController();
            this.mines = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // Przycisk nowej gry
            this.button_newgame.Location = new System.Drawing.Point(182, 12);
            this.button_newgame.Name = "button_newgame";
            this.button_newgame.Size = new System.Drawing.Size(75, 23);
            this.button_newgame.TabIndex = 0;
            this.button_newgame.Text = "Nowa gra";
            this.button_newgame.UseVisualStyleBackColor = true;
            this.button_newgame.Click += new System.EventHandler(this.ButtonNewGame);
            // Tekstowy stan gry
            this.stan.AutoSize = true;
            this.stan.Location = new System.Drawing.Point(333, 17);
            this.stan.Name = "stan";
            this.stan.Size = new System.Drawing.Size(35, 13);
            this.stan.TabIndex = 3;
            this.stan.Text = "";
            // Tabela z polami
            this.mines.AutoSize = true;
            this.mines.ColumnCount = 1;
            this.mines.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mines.Location = new System.Drawing.Point(12, 41);
            this.mines.Name = "mines";
            this.mines.RowCount = 1;
            this.mines.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mines.Size = new System.Drawing.Size(418, 397);
            this.mines.TabIndex = 4;
            // Okienko formularza
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 440);
            this.Controls.Add(this.mines);
            this.Controls.Add(this.stan);
            this.Controls.Add(this.button_newgame);
            this.Name = "SaperForm";
            this.Text = "Saper";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button button_newgame;
        private System.Windows.Forms.Label stan;
        private System.ServiceProcess.ServiceController serviceController1;
        private System.Windows.Forms.TableLayoutPanel mines;
    }

    //Ta część kodu odpowiada za uruchomienie aplikacji 
    static class Program {
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Game()); //Poprzez wywołanie funkcji podająć klasę formularza Game
        }
    }
}
