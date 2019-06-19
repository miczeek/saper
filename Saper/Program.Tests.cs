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
    class ProgramTests
    {

        Game game = null;

        int x = 5;
        int y = 5;

        //! Tablica z ustawionymi bombami
        int[] bombs = {
            0,0,0,0,0,
            0,1,0,0,0,
            0,0,0,0,1,
            0,0,0,0,1,
            0,0,1,0,1,
        };

        //! Przed każdym testem tworzymy nową grę
        [Test]
        public void GameCreate() {
            game = new Game();
            game.NewGame(x, y, 10, bombs);

            //! Sprawdzenie czy gra została utworzona
            Assert.AreEqual(game.created, true);
        }

        [Test]
        public void GameSize () {
            GameCreate();
            //! Sprawdzenie czy rozmiar gry się zgadza
            Assert.AreEqual(game.game_size, x * y);
            Assert.AreEqual(game.game_width, x);
            Assert.AreEqual(game.game_height, y);
        }

        [Test]
        public void GameNotOver () {
            GameCreate();
            //! Sprawdzenie czy gra nie jest zakończona po utworzeniu
            Assert.AreEqual(game.game_over, false);
        }

        [Test]
        public void GameBombs () {
            GameCreate();

            int bombs_amount = 0;
            foreach (int field in bombs) {
                bombs_amount += field;
            }

            //! Sprawdzenie czy ilość bomb się zgadza
            int amount = 5;
            Assert.AreEqual(amount, bombs_amount);

            //! Sprawdzenie czy ilość pustych pól się zgadza
            Assert.AreEqual(game.tiles_left, x * y - bombs_amount);
        }

        [Test]
        public void GameSetGetFields () {
            GameCreate();

            //! Ustawienie pola 2,2 na 1 (bombę)
            game.setField(2, 2, 1);
            //! Sprawdzenie czy na polu 3,3 jest 1
            Assert.AreEqual(game.getField(2, 2), 1);
            Assert.AreNotEqual(game.getField(2, 2), 0);

            //! Przywrócenie stanu pola
            game.setField(2, 2, 0);
        }

        [Test]
        public void GameBombsAround () {
            GameCreate();

            int x = -1;
            //! Ręczne utworzena tablica z zaznaczonymi bombami wokół
            //! Pola z bombami ustawiamy na -1 i pomijamy przy sprawdzaniu
            int[] bombs_around = {
                1,1,1,0,0,
                1,x,1,1,1,
                1,1,1,2,x,
                0,1,1,4,x,
                0,1,x,3,x,
            };

            //! Pętle po wszystkich polach
            for (int i = 0; i < x; i++) {
                for (int j = 0; j < y; j++) {
                    Assert.AreEqual(game.TilesAround(i, j), bombs_around[i * x + j]);
                }
            }
        }

        [Test]
        public void GameLose () {
            GameCreate();
            //! Sprawdzenie czy gra zostanie przegrana po naciśnięciu pola z bombą
            game.TileClick(game.GetTile(1, 1), true);

            Assert.AreEqual(game.game_over, true);
            Assert.AreEqual(game.game_win, false);
        }

        [Test]
        public void GameWin () {
            GameCreate();
            //! Sprawdzenie czy gra zostanie wygrana po naciśnięciu wszystkich pustych pól
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
