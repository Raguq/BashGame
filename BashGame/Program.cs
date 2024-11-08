using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Установка значений по умолчанию
        int defaultN = 21;
        int defaultM = 3;

        Console.WriteLine($"Введите количество предметов (N) [по умолчанию: {defaultN}]: ");
        string inputN = Console.ReadLine();
        int N = string.IsNullOrEmpty(inputN) ? defaultN : int.Parse(inputN);

        Console.WriteLine($"Введите максимальное количество, которое можно взять за ход (M) [по умолчанию: {defaultM}]: ");
        string inputM = Console.ReadLine();
        int M = string.IsNullOrEmpty(inputM) ? defaultM : int.Parse(inputM);

        Console.WriteLine("Выберите стратегию игрока 1 (1 - Агрессивная, 2 - Защитная, 3 - Монте-Карло, 4 - Случайная):");
        int player1Strategy = int.Parse(Console.ReadLine());

        Console.WriteLine("Выберите стратегию игрока 2 (1 - Агрессивная, 2 - Защитная, 3 - Монте-Карло, 4 - Случайная):");
        int player2Strategy = int.Parse(Console.ReadLine());

        int player1Wins = 0;
        int player2Wins = 0;
        int rounds = 100; // количество итераций, чтобы оценить стратегию

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Parallel.For(0, rounds, i =>
        {
            var result = PlayGame(N, M, player1Strategy, player2Strategy);
            if (result == 1)
                player1Wins++;
            else if (result == 2)
                player2Wins++;
        });
        stopwatch.Stop();
        Console.WriteLine($"Игрок 1 победил: {player1Wins} раз ({(double)player1Wins / rounds * 100}%)");
        Console.WriteLine($"Игрок 2 победил: {player2Wins} раз ({(double)player2Wins / rounds * 100}%)");
        Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} миллисекунд.");
    }

    static int PlayGame(int N, int M, int strategy1, int strategy2)
    {
        int currentPlayer = 1; // 1 - Игрок 1, 2 - Игрок 2
        while (N > 0)
        {
            int move = 0;
            if (currentPlayer == 1)
            {
                move = GetMove(N, M, strategy1);
                Console.WriteLine($"Игрок 1 выбирает: {move}");
            }
            else
            {
                move = GetMove(N, M, strategy2);
                Console.WriteLine($"Игрок 2 выбирает: {move}");
            }

            N -= move;
            currentPlayer = currentPlayer == 1 ? 2 : 1; // Переключение игрока
        }
        return currentPlayer; // Возвращаем номер проигравшего игрока
    }

    static int GetMove(int N, int M, int strategy)
    {
        switch (strategy)
        {
            case 1: // Агрессивная стратегия
                return Math.Min(M, N); // Берёт максимальное количество
            case 2: // Защитная стратегия
                return (N - 1) % (M + 1) + 1; // Старается оставить сопернику проигрышную позицию
            case 3: // Тактика Монте-Карло
                return MonteCarloMove(N, M);
            case 4: // Случайная стратегия
                return new Random(Guid.NewGuid().GetHashCode()).Next(1, Math.Min(M, N) + 1); // Случайный выбор от 1 до Min(M, N)
            default:
                return 1; // По умолчанию берём 1
        }
    }

    static int MonteCarloMove(int N, int M)
    {
        const int simulations = 1000000; // количество симуляций
        var winRates = new ConcurrentDictionary<int, int>();

        for (int take = 1; take <= Math.Min(M, N); take++)
        {
            int wins = 0;
            for (int sim = 0; sim < simulations; sim++)
            {
                int simulatedN = N - take;
                int currentPlayer = 2; // Начинаем с игрока 2
                while (simulatedN > 0)
                {
                    // Игрок 2 принимает самое выгодное решение, всё же это упрощенный вариант
                    int move = Math.Min(M, simulatedN);
                    simulatedN -= move;
                    currentPlayer = currentPlayer == 1 ? 2 : 1; // Переключение игрока
                }
                if (currentPlayer == 1) wins++; // Если выиграл игрок 1
            }
            winRates[take] = wins;
        }

        // Выбор хода с самым высоким win rate
        return winRates.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    }
}
