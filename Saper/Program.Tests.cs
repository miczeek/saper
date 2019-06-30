using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using System.IO;

namespace Saper.Tests
{
    //! Klasa dla testów jednostkowych
    class ProgramTests
    {

        //! Obiekt gry klasy Game
        Game game = null;

        //! Szerokość gry
        int x = 5;
        //! Wysokość gry
        int y = 5;

        //! Tablica z ustawionymi bombami
        int[] bombs = {
            0,0,0,0,0,
            0,1,0,0,0,
            0,0,0,0,1,
            0,0,0,0,1,
            0,0,1,0,1,
        };

        //! Utworzenie nowej gry - wywoływane przed każdym testem
        [Test]
        public void GameCreate() {
            game = new Game();
            game.NewGame(x, y, 10, bombs);

            // Sprawdzenie czy gra została utworzona
            Assert.AreEqual(game.created, true);
        }

        //! Sprawdzenie czy rozmiar gry jest poprawny
        [Test]
        public void GameSize () {
            GameCreate();
            Assert.AreEqual(game.game_size, x * y);
            Assert.AreEqual(game.game_width, x);
            Assert.AreEqual(game.game_height, y);
        }

        //! Sprawdzenie czy gra nie jest zakończona po utworzeniu
        [Test]
        public void GameNotOver () {
            GameCreate();
            Assert.AreEqual(game.game_over, false);
        }

        //! Sprawdzenie czy ilość bomb się zgadza oraz czy ilość pustych pól się zgadza
        [Test]
        public void GameBombs () {
            GameCreate();

            int bombs_amount = 0;
            foreach (int field in bombs) {
                bombs_amount += field;
            }

            
            int amount = 5;
            Assert.AreEqual(amount, bombs_amount);
            Assert.AreEqual(game.tiles_left, x * y - bombs_amount);
        }

        //! Sprawdzanie czy ustawienia stanu działa poprawnie
        [Test]
        public void GameSetGetFields () {
            GameCreate();

            // Ustawienie pola 2,2 na 1 (bombę)
            game.setField(2, 2, 1);
            // Sprawdzenie czy na polu 3,3 jest 1
            Assert.AreEqual(game.getField(2, 2), 1);
            Assert.AreNotEqual(game.getField(2, 2), 0);

            // Przywrócenie stanu pola
            game.setField(2, 2, 0);
        }

        //! Sprawdzanie czy ilość bomb w pobliżu jest liczona prawidłowo
        [Test]
        public void GameBombsAround () {
            GameCreate();

            int x = -1;
            // Ręczne utworzena tablica z zaznaczonymi bombami wokół
            // Pola z bombami ustawiamy na -1 i pomijamy przy sprawdzaniu
            int[] bombs_around = {
                1,1,1,0,0,
                1,x,1,1,1,
                1,1,1,2,x,
                0,1,1,4,x,
                0,1,x,3,x,
            };

            // Pętle po wszystkich polach
            for (int i = 0; i < x; i++) {
                for (int j = 0; j < y; j++) {
                    Assert.AreEqual(game.TilesAround(i, j), bombs_around[i * x + j]);
                }
            }
        }

        //! Sprawdzenie czy gra zostanie przegrana po naciśnięciu pola z bombą
        [Test]
        public void GameLose () {
            GameCreate();
        
            game.TileClick(game.GetTile(1, 1), true);

            Assert.AreEqual(game.game_over, true);
            Assert.AreEqual(game.game_win, false);
        }

        //! Sprawdzenie czy gra zostanie wygrana po naciśnięciu wszystkich pustych pól
        [Test]
        public void GameWin () {
            GameCreate();
            
            for (int i = 0; i < x; i++) {
                for (int j = 0; j < y; j++) {
                    if (bombs[i * x + j] == 0) {
                        game.TileClick(game.GetTile(i, j), true);
                    }
                }
            }
            Assert.AreEqual(game.game_over, true);
            Assert.AreEqual(game.game_win, true);
        }
    }

}
