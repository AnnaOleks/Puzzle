using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puzzle
{
    public static class GameSettings
    {
        // Текущий размер доски (значения по умолчанию — первый пункт 6×4)
        public static int Rows { get; private set; } = 6;
        public static int Cols { get; private set; } = 4;

        // Запоминаем выбранный пункт пикера
        public static int SelectedSizeIndex { get; private set; } = 0;

        // Событие: подписчики узнают о смене размера (передаём новые rows/cols)
        public static event Action<int, int>? lauaSuurus;

        // Устанавливаем размер и уведомляем подписчиков
        public static void SeaLauaSuurus(int rows, int cols, int selectedIndex)
        {
            Rows = rows;
            Cols = cols;
            SelectedSizeIndex = selectedIndex;
            lauaSuurus?.Invoke(rows, cols);
        }
    }

}
