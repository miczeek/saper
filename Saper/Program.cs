/*! \mainpage Saper
 * Autor: Filip Mika \n
 * Opis projektu: Gra Saper znana z Windowsa wykonana w C# jako projekt na uczelnię
 */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;

namespace Saper {

    //! Klasa Tile dla pola/kafelka, dziedziczone z klasy PictureBox
    public class Tile : PictureBox
    {
        //! Wielkość grafiki 32x32
        public int tileSize = 32;
        //! Pozycja od lewej, liczone od zera
        public int x;
        //! Pozycja od góry, liczone od zera
        public int y;
        //! Ilość bomb w pobliżu tego pola, domyślnie -1, będzie zaktualizowane później
        public int around = -1;
        //! Statusy dla pola: Puste lub z Bombą
        public enum States { Empty, Bomb }
        //! Status pola odpowiadający enum States
        public int state;
        //! Czy pole zostało kliknięte
        public bool clicked = false;
        //! Czy pole zostało oznaczone flagą
        public bool flag = false;

        //! Klasa główna Game do której będziemy mogli się odwoływać
        Game gra;

        //! Konstruktor klasy (wywoływany przy jej tworzeniu)
        //! @param _x, _y Pozycje od lewej, góry
        //! @param _gra Obiekt z grą klasy Game
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

        //! Funkcja odpowiadająca za odświeżenie grafiki
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

    //! Klasa Game odpowiadająca za działanie gry, dziedzicząca z klasy Form
    public partial class Game : Form {

        //! Tablica przechowująca tylko stan danego pola (bomba/puste)
        int[] fields;
        //! Tablica przechowująca pola
        Tile[] tiles;
        //! Ilość kolumn z polami
        public int game_width;
        //! Ilość wierszy z polami
        public int game_height;
        //! Wielkość gry (ilość kafelków)
        public int game_size;
        //! Ile pól pozostało do kliknięcia
        public int tiles_left;
        //! Szansa na wystąpienie bomby (1/10)
        int bomb_chance = 10;
        //! Czy gra została zakończona
        public bool game_over;
        //!Generator liczb pseudolosowych
        Random rnd = new Random();
        //! Czy gra została utworzona
        public bool created = false;
        //! Czy gra została wygrana
        public bool game_win; 

        //! Funkcja zwracająca ilość bomb
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

        //! Funkcja losująca bomby
        void RandomBombs () {
            // Pętle po wszystkich polach
            for (int i = 0; i < game_width; i++) {
                for (int j = 0; j < game_height; j++) {
                    int state;
                    if (rnd.Next(0, bomb_chance - 1) > 0) { // Losowanie czy na danym polu jest bomba, 
                        state = (int)Tile.States.Empty;  // jeżeli wylosowano liczbę większą od 0 - nie ma
                        tiles_left++; // Zwiększenie stanu licznika
                    } else {
                        state = (int)Tile.States.Bomb; // Ustawienie statusu bomby
                    }
                    setField(i, j, state); // Ustawienie statusu pola w tablicy
                }
            }
        }

        //! Funkcja ustawiająca bomby
        //! @param bombs Tablica z bombami
        void SetBombs(int[] bombs) {
            for (int i = 0; i < game_width; i++) {
                for (int j = 0; j < game_height; j++) {
                    int state;
                    if (bombs[GetIndex(i, j)] == 0) { // Przypisanie wartości z pola
                        state = (int)Tile.States.Empty; 
                        tiles_left++; // Zwiększenie stanu licznika
                    } else {
                        state = (int)Tile.States.Bomb; // Ustawienie statusu bomby
                    }
                    setField(i, j, state); // Ustawienie statusu pola w tablicy
                }
            }
        }


        //! Funkcja otwierająca okno zapytania z 3 mozliwościami
        public static DialogResult DialogBox(string title, string promptText, ref string value, string button1 = "OK", string button2 = "Cancel", string button3 = null) {
            // Funkcja z: https://stackoverflow.com/questions/4264664/how-to-change-the-button-text-for-yes-and-no-buttons-in-the-messagebox-show
            // Tworzy okienko z zapytaniem i 3 przyciskami
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button button_1 = new Button();
            Button button_2 = new Button();
            Button button_3 = new Button();

            int buttonStartPos = 228; //Standard two button position

            if (button3 != null)
                buttonStartPos = 228 - 81;
            else {
                button_3.Visible = false;
                button_3.Enabled = false;
            }

            form.Text = title;

            // Label
            label.Text = promptText;
            label.SetBounds(9, 20, 372, 13);
            label.Font = new Font("Microsoft Tai Le", 10, FontStyle.Regular);

            // TextBox
            if (value == null) {
            } else {
                textBox.Text = value;
                textBox.SetBounds(12, 36, 372, 20);
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            }

            button_1.Text = button1;
            button_2.Text = button2;
            button_3.Text = button3 ?? string.Empty;
            button_1.DialogResult = DialogResult.OK;
            button_2.DialogResult = DialogResult.Cancel;
            button_3.DialogResult = DialogResult.Yes;


            button_1.SetBounds(buttonStartPos, 72, 75, 23);
            button_2.SetBounds(buttonStartPos + 81, 72, 75, 23);
            button_3.SetBounds(buttonStartPos + (2 * 81), 72, 75, 23);

            label.AutoSize = true;
            button_1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, button_1, button_2 });
            if (button3 != null)
                form.Controls.Add(button_3);
            if (value != null)
                form.Controls.Add(textBox);

            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = button_1;
            form.CancelButton = button_2;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        //! Funkcja do wyboru poziomu trudnośći dla nowej gry
        public void NewGameType () {
            string message = "Wybierz poziom trudności, od niego będzie zależeć wielkośc gry oraz częstotliwość pojawiania się bomb";
            string caption = "Poziom trudności";

            string _result = null;
            DialogResult result = DialogBox(caption, message, ref _result, "Łatwy", "Średni", "Trudny");

            if (result == DialogResult.OK) {
                NewGame(8, 8, 10);
            }
            if (result == DialogResult.Cancel) {
                NewGame(15, 15, 9);
            }
            if (result == DialogResult.Yes) {
                NewGame(20, 20, 8);
            }
        }

        //! Funkcja odpowiadająca za zmianę rozmiaru okna i ustawienie przycisków
        public void DrawLayout () {  

            // 13px marginesów po obu stronach + ilość kolumn * 32 (rozmiar) + 6 (marginesy po prawej i lewej)
            int width = 13 * 2 + game_width * (32 + 6);
            // 13px marginesów góra/dół + ilość wierszy * 32 (rozmiar) + 6 (marginesy po prawej i lewej) 
            // + margines górny na przycisk nowej gry
            int height = 13 * 2 + game_height * (32 + 6) + 30;

            this.ClientSize = new System.Drawing.Size(width, height);

            // Ustawienie lokalizacji przycisku nowej gry
            this.button_newgame.Location = new System.Drawing.Point(width / 2 - 75 / 2, 12);

            // Ustawienie lokalizacji tekstu o stanie gry
            this.stan.Location = new System.Drawing.Point(width / 3 * 2 - 35 / 2, 17);
        }

        //! Funkcja tworząca nową grę (standardowo o rozmiarze 10x10)
        //! @param width, height Szerokość i wysokość planszy
        //! @param chance_bomb  Szansa na wystąpienie bomby (1/x)
        //! @param bombs Opcjonalna tablica z ustawionymi bombami
        public void NewGame(int width = 14, int height = 14, int chance_bomb = 10, int[] bombs = null) { 
            stan.Text = "Powodzenia!"; // Zmiana tekstu na powodzenia
            game_over = false; // Gra nie jest zakończona
            game_width = width; // Przypisanie szerokości gry z argumentu (ilości pól w poziomie)
            game_height = height; // Przypisanie szerokości gry z wysokości (ilości pól w pionie)
            game_size = width * height; // Ilość pól w całej grze (poziom * pion)
            fields = new int[game_size]; // Utworzenie tablicy na statusy pól
            tiles_left = 0; // Licznik pozostałych pól 
            game_win = false;
            bomb_chance = chance_bomb;  // Szansa na wystąpienie bomby (domyślnie 1/10)

            DrawLayout();

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
                    tiles[GetIndex(i, j)] = tile; // Przypisanie pola na odpowiednie miejsce w tablicy z polami
                }
            }

            // Oznaczenie, że gra została utworzona
            created = true;
        }

        //! Pobranie zawartości pola
        //! @param i, j Pozycje od lewej oraz góry
        public int getField(int i, int j) { 
            // Jeżeli podano niestniejące pole (wychodzące poza ramy gry) zwróć 0
            if (i < 0 || i >= game_width || j < 0 || j >= game_height) return 0;
            return fields[GetIndex(i, j)]; //! Zwróć wartość pola
        }

        //! Ustawienie statusu pola
        //! @param i, j Pozycje od lewej oraz góry
        //! @param state Nowy status na polu (bomba/puste)
        public void setField(int i, int j, int state) { 
            fields[GetIndex(i, j)] = state;
        }

        //! Funckja zwracająca dane pole
        //! @param i, j Pozycje od lewej oraz góry
        public Tile GetTile (int i, int j) { 
            return tiles[GetIndex(i,j)];
        }

        //! Funkcja zwracająca index w tablicy
        //! @param i, j Pozycje od lewej oraz góry
        public int GetIndex(int i, int j) { 
            return i * game_height + j;
        }

        //! Funkcja wykonywana po kliknięciu na pole
        void TileOnClick(object sender, EventArgs e) { 
            if (game_over) return; // Jeżeli gra została już zakończona to przerwnij wykonywanie funkcji
            Tile tile = (sender as Tile); // Pobranie klikniętego pola

            // Przekonwertowanie klasy EventArgs na MouseEventArgs, aby można było sprawdzić użycie prawego przycisku myszy
            MouseEventArgs me = (MouseEventArgs)e; 
            if (me.Button == System.Windows.Forms.MouseButtons.Right) { // Sprawdzenie czy użyto prawy przycisk myszy
                tile.flag = !tile.flag; // Zmiana statusu flagi na odwrotny
                tile.Img(); // Odświeżenie grafiki pola
            } else {
                TileClick(tile, true); //! Funkcja pomocnicza po kliknięciu na pole
            }
        }
        //! Funkcja pomocnicza po kliknięciu na pole
        //! @param tile Obiekt pola klasy Tile
        //! @param user_click Czy funkcja została wywołana przez nacisnięcie przycisku czy inna funkcję
        public void TileClick(Tile tile, bool user_click) { 
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

        //! Funkcja naciskająca pola wokół
        //! @param x, y Pozycje od lewej oraz góry
        void TilesAroundClick(int x, int y) { 
            ClickField(x, y - 1); // Pole na górze
            ClickField(x - 1, y); // Pole na lewo
            ClickField(x + 1, y); // Pole na dole
            ClickField(x, y + 1); // Pole na prawo
        }

        //! Funkcja naciskająca dane pole
        //! @param i, j Pozycje od lewej oraz góry
        public void ClickField(int i, int j) { 
            // Jeżeli podano niestniejące pole (wychodzące poza ramy gry) przerwij wykonywanie funkcji
            if (i < 0 || i >= game_width || j < 0 || j >= game_height) return;
            Tile tile = tiles[GetIndex(i, j)]; // Pobierz dane pole do zmiennej
            if (tile.state == (int)Tile.States.Empty) { //! Jeżeli pole jest puste
                if (tile.around == 0) { // Jeżeli naciśnięto pole które nie ma bomb wokół,
                    TileClick(tile, false); // Wtedy naciśnij to pole
                } else if (!tile.clicked) { // Jeżeli pole nie zostało wcześniej naciśnięte
                    TileMinus();// Odejmij puste pole od licznika
                    tile.clicked = true; // Oznacz pole jako naciśnięte
                    tile.Img(); // Odśwież grafikę pola
                }
            }
        }

        //! Funkcja odemująca pozostałe pola
        void TileMinus() { 
            tiles_left--; // Odjęcie jednego pola
            stan.Text = tiles_left + " pozostało"; // Wyświetlenie ile pól pozostało
            if (tiles_left == 0) { // Jeżeli pozostało 0 pól oznacza, że gra została wygrana
                GameWin(); // więc wywołanie funkcji odpowiadającej za wygraną
            }
        }

        //! Funkcja sprawdzająca ile pól wokół posiada bomby
        //! @param x, y Pozycje od lewej oraz góry
        public int TilesAround(int x, int y) { 
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

        //! Funkcja kończąca grę
        //! @param tekst Tekst ukazywany po zakończeniu gry
        //! @param win  Czy gra została wygrana
        void GameEnd(string tekst, bool win) { 
            stan.Text = tekst; // Zmiana tekstu o statusie gry
            game_over = true; // Ustawiamy stan gry jako zakończony
            game_win = win; // Ustawienei stanu gry 

            // Pętla po wszystkich polach
            for (int n = 0; n < game_size; n++) {
                var tile = tiles[n]; // Pobranie n-tego pola
                tile.Img(); // Odświeżenie grafiki każdego pola po zakończniu gry
            }
        }

        //! Funkcja wywoływana kiedy przegramy grę
        void GameWin() { 
            GameEnd("Wygrałeś! B)", true);
        }

        //! Funkcja wywoływana kiedy przegramy grę
        void GameOver() { 
            GameEnd("Przegrałeś! :(", false);
        }

        //!Funkcja wywoływana po naciśnięciu przycisku nowej gry
        void ButtonNewGame(object sender, EventArgs e) { 
            NewGameType(); // Wywołanie funkcji utworzenia nowej gry
        }

        //! Konstruktor klasy Game, wywoływany przy tworzeniu
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

    // Ta część kodu odpowiada za uruchomienie aplikacji 
    static class Program {
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Game()); //Poprzez wywołanie funkcji podająć klasę formularza Game
        }
    }
}
