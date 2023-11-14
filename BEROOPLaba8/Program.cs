using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;

namespace BEROOPLaba8
{
    public enum AccountType
    {
        Savings,
        Checking,
        Credit
    }

    public interface IFormattableInterface
    {
        void Format();
    }

    public class BankTransaction
    {
        public readonly decimal Amount;
        public readonly DateTime TransactionDateTime;

        public BankTransaction(decimal amount)
        {
            Amount = amount;
            TransactionDateTime = DateTime.Now;
        }
    }

    public class BankAccount : IFormattableInterface, IDisposable
    {
        private static int accountNumberSeed = 1234567890;
        private readonly Queue<BankTransaction> transactions = new Queue<BankTransaction>();

        public int AccountNumber { get; }
        public decimal Balance { get; private set; }
        public AccountType Type { get; }

        public BankAccount(AccountType type) : this(type, 0) { }

        public BankAccount(AccountType type, decimal initialBalance)
        {
            AccountNumber = accountNumberSeed++;
            Type = type;
            Balance = initialBalance;
        }

        public void Deposit(decimal amount)
        {
            Balance += amount;
            transactions.Enqueue(new BankTransaction(amount));
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= Balance)
            {
                Balance -= amount;
                transactions.Enqueue(new BankTransaction(-amount));
                return true;
            }
            return false;
        }

        public void Transfer(BankAccount destinationAccount, decimal amount)
        {
            if (Withdraw(amount))
            {
                destinationAccount.Deposit(amount);
                Console.WriteLine("Перевод выполнен успешно.");
            }
            else
            {
                Console.WriteLine("Недостаточно средств для перевода.");
            }
        }

        public void Format()
        {
            Console.WriteLine($"Номер счета: {AccountNumber}, Тип счета: {Type}, Баланс: {Balance}");
        }

        public void Dispose()
        {
            string directoryPath = @"C:\Users\Coolsinee\source\repos\BEROOPLaba8\BEROOPLaba8"; // Замените на нужный вам путь
            string filePath = Path.Combine(directoryPath, $"transactions_{AccountNumber}.txt");

            using (FileStream fileStream = File.Create(filePath))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    foreach (var transaction in transactions)
                    {
                        writer.WriteLine($"Дата: {transaction.TransactionDateTime}, Сумма: {transaction.Amount}");
                    }
                }
            }

            GC.SuppressFinalize(this);
        }
    }

    public class FormatChecker
    {
        public void CheckIFormattable(object obj)
        {
            if (obj is IFormattableInterface)
            {
                Console.WriteLine("Объект реализует интерфейс IFormattable.");
                ((IFormattableInterface)obj).Format();
            }
            else
            {
                Console.WriteLine("Объект не реализует интерфейс IFormattable.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            FormatChecker formatChecker = new FormatChecker();
            BankAccount[] accounts = new BankAccount[10]; // Предполагаем, что у вас есть 10 счетов
            int accountCount = 0;

            while (true)
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1. Создать новый банковский счет");
                Console.WriteLine("2. Показать информацию о счете");
                Console.WriteLine("3. Внести деньги на счет");
                Console.WriteLine("4. Снять деньги со счета");
                Console.WriteLine("5. Перевести деньги на другой счет");
                Console.WriteLine("6. Проверить интерфейс IFormattable");
                Console.WriteLine("7. Выйти из программы");

                int choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        if (accountCount < 10)
                        {
                            Console.Write("Введите начальный баланс: ");
                            decimal initialBalance = Convert.ToDecimal(Console.ReadLine());
                            Console.WriteLine("Выберите тип счета (0 - Savings, 1 - Checking, 2 - Credit): ");
                            AccountType accountType = (AccountType)Enum.Parse(typeof(AccountType), Console.ReadLine());
                            accounts[accountCount] = new BankAccount(accountType, initialBalance);
                            Console.WriteLine($"Создан новый счет. Номер счета: {accounts[accountCount].AccountNumber}, Тип счета: {accounts[accountCount].Type}, Баланс: {accounts[accountCount].Balance}");
                            accountCount++;
                        }
                        else
                        {
                            Console.WriteLine("Нельзя создать больше счетов.");
                        }
                        break;

                    case 2:
                        Console.Write("Введите номер счета: ");
                        int accountNumber = Convert.ToInt32(Console.ReadLine());
                        BankAccount selectedAccount = null;

                        for (int i = 0; i < accountCount; i++)
                        {
                            if (accounts[i].AccountNumber == accountNumber)
                            {
                                selectedAccount = accounts[i];
                                break;
                            }
                        }

                        if (selectedAccount != null)
                        {
                            Console.WriteLine($"Номер счета: {selectedAccount.AccountNumber}, Тип счета: {selectedAccount.Type}, Баланс: {selectedAccount.Balance}");
                        }
                        else
                        {
                            Console.WriteLine("Счет не найден.");
                        }
                        break;

                    case 3:
                        Console.Write("Введите номер счета для внесения денег: ");
                        int depositAccountNumber = Convert.ToInt32(Console.ReadLine());
                        BankAccount depositAccount = null;

                        for (int i = 0; i < accountCount; i++)
                        {
                            if (accounts[i].AccountNumber == depositAccountNumber)
                            {
                                depositAccount = accounts[i];
                                break;
                            }
                        }

                        if (depositAccount != null)
                        {
                            Console.Write("Введите сумму для внесения: ");
                            decimal depositAmount = Convert.ToDecimal(Console.ReadLine());
                            depositAccount.Deposit(depositAmount);
                            Console.WriteLine($"Баланс после внесения: {depositAccount.Balance}");
                        }
                        else
                        {
                            Console.WriteLine("Счет не найден.");
                        }
                        break;

                    case 4:
                        Console.Write("Введите номер счета для снятия денег: ");
                        int withdrawAccountNumber = Convert.ToInt32(Console.ReadLine());
                        BankAccount withdrawAccount = null;

                        for (int i = 0; i < accountCount; i++)
                        {
                            if (accounts[i].AccountNumber == withdrawAccountNumber)
                            {
                                withdrawAccount = accounts[i];
                                break;
                            }
                        }

                        if (withdrawAccount != null)
                        {
                            Console.Write("Введите сумму для снятия: ");
                            decimal withdrawAmount = Convert.ToDecimal(Console.ReadLine());
                            if (withdrawAccount.Withdraw(withdrawAmount))
                            {
                                Console.WriteLine($"Баланс после снятия: {withdrawAccount.Balance}");
                            }
                            else
                            {
                                Console.WriteLine("Недостаточно средств на счете.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Счет не найден.");
                        }
                        break;

                    case 5:
                        Console.Write("Введите номер счета, с которого хотите перевести деньги: ");
                        int sourceAccountNumber = Convert.ToInt32(Console.ReadLine());
                        Console.Write("Введите номер счета, на который хотите перевести деньги: ");
                        int targetAccountNumber = Convert.ToInt32(Console.ReadLine());

                        BankAccount sourceAccount = null;
                        BankAccount targetAccount = null;

                        for (int i = 0; i < accountCount; i++)
                        {
                            if (accounts[i].AccountNumber == sourceAccountNumber)
                            {
                                sourceAccount = accounts[i];
                            }
                            if (accounts[i].AccountNumber == targetAccountNumber)
                            {
                                targetAccount = accounts[i];
                            }

                            if (sourceAccount != null && targetAccount != null)
                            {
                                break;
                            }
                        }

                        if (sourceAccount != null && targetAccount != null)
                        {
                            Console.Write("Введите сумму для перевода: ");
                            decimal transferAmount = Convert.ToDecimal(Console.ReadLine());
                            sourceAccount.Transfer(targetAccount, transferAmount);
                        }
                        else
                        {
                            Console.WriteLine("Один из счетов не найден.");
                        }
                        break;

                    case 6:
                        Console.Write("Введите номер счета для проверки интерфейса IFormattable: ");
                        int accountNumberForInterfaceCheck = Convert.ToInt32(Console.ReadLine());
                        BankAccount accountForInterfaceCheck = null;

                        for (int i = 0; i < accountCount; i++)
                        {
                            if (accounts[i].AccountNumber == accountNumberForInterfaceCheck)
                            {
                                accountForInterfaceCheck = accounts[i];
                                break;
                            }
                        }

                        if (accountForInterfaceCheck != null)
                        {
                            formatChecker.CheckIFormattable(accountForInterfaceCheck);
                        }
                        else
                        {
                            Console.WriteLine("Счет не найден.");
                        }
                        break;

                    case 7:
                        Console.WriteLine("До свидания!");// Вызываем Dispose для каждого объекта BankAccount перед выходом
                        for (int i = 0; i < accountCount; i++)
                        {
                            accounts[i].Dispose();
                        }
                        return;

                    default:
                        Console.WriteLine("Неправильный выбор.");
                        break;
                }
            }
        }
    }
}
