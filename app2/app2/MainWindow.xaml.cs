using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace app2
{
    public partial class MainWindow : Window
    {
        private RSAKeys currentKeys;
        private readonly int targetDigits = 51;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Генерация ключей
        private void BtnGenerateKeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                currentKeys = GenerateRSAKeys(targetDigits);
                sw.Stop();

                tbPublicKey.Text = $"e={currentKeys.PublicKey.E}, N={currentKeys.PublicKey.N.ToString().Length} цифр";
                tbPrivateKey.Text = $"d={currentKeys.PrivateKey.D}, N={currentKeys.PublicKey.N.ToString().Length} цифр";
                Log($"Ключи сгенерированы за {sw.ElapsedMilliseconds} мс. N: {currentKeys.PublicKey.N.ToString().Length} цифр.");
            }
            catch (Exception ex)
            {
                Log($"Ошибка генерации ключей: {ex.Message}");
            }
        }

        // Шифрование
        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentKeys == null) { Log("Сначала сгенерируйте ключи!"); return; }
                var text = tbInputText.Text;
                if (string.IsNullOrEmpty(text)) { Log("Введите текст!"); return; }

                var sw = Stopwatch.StartNew();
                var blocks = TextToBlocks(text, currentKeys.PublicKey.N);
                var encryptedBlocks = EncryptBlocks(blocks, currentKeys.PublicKey);
                var encryptedText = string.Join(" ", encryptedBlocks.Select(b => b.ToString()));
                sw.Stop();

                tbEncrypted.Text = encryptedText;
                lblEncryptTime.Text = $"Время шифрования: {sw.ElapsedMilliseconds} мс ({blocks.Count} блоков)";
                Log($"Шифрование: {text.Length} символов → {encryptedBlocks.Count} блоков за {sw.ElapsedMilliseconds} мс.");
            }
            catch (Exception ex)
            {
                Log($"Ошибка шифрования: {ex.Message}");
            }
        }

        // Расшифровка
        private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentKeys == null) { Log("Сначала сгенерируйте ключи!"); return; }
                if (string.IsNullOrEmpty(tbEncrypted.Text)) { Log("Нет шифротекста!"); return; }

                var sw = Stopwatch.StartNew();
                var blocks = tbEncrypted.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => BigInteger.Parse(s)).ToList();
                var decryptedBlocks = DecryptBlocks(blocks, currentKeys.PrivateKey);
                var decryptedText = BlocksToText(decryptedBlocks);
                sw.Stop();

                tbDecrypted.Text = decryptedText;
                lblDecryptTime.Text = $"Время расшифровки: {sw.ElapsedMilliseconds} мс";
                Log($"Расшифровка успешна: {decryptedText}");
            }
            catch (Exception ex)
            {
                Log($"Ошибка расшифровки: {ex.Message}");
            }
        }

        private void BtnClearInput_Click(object sender, RoutedEventArgs e) => tbInputText.Clear();

        // Генерация ключей RSA
        private RSAKeys GenerateRSAKeys(int digits)
        {
            int halfDigits = digits / 2;
            int remainingDigits = digits - halfDigits;

            BigInteger p = GenerateLargePrime(halfDigits);
            BigInteger q = GenerateLargePrime(remainingDigits);

            while (p == q)
                q = GenerateLargePrime(remainingDigits);

            var n = p * q;

            while (n.ToString().Length != digits)
            {
                if (n.ToString().Length < digits)
                {
                    p = GenerateLargePrime(halfDigits + 1);
                    q = GenerateLargePrime(remainingDigits);
                }
                else
                {
                    p = GenerateLargePrime(halfDigits - 1);
                    q = GenerateLargePrime(remainingDigits);
                }
                n = p * q;
            }

            var phi = (p - 1) * (q - 1);
            var e = new BigInteger(65537);

            while (BigInteger.GreatestCommonDivisor(e, phi) != 1)
                e += 2;

            var d = ModInverse(e, phi);

            return new RSAKeys
            {
                PublicKey = new RSAKey { E = e, N = n },
                PrivateKey = new RSAKey { D = d, N = n }
            };
        }

        // Генерация большого простого числа
        private BigInteger GenerateLargePrime(int digits)
        {
            BigInteger minValue = BigInteger.Pow(10, digits - 1);
            BigInteger maxValue = BigInteger.Pow(10, digits) - 1;

            while (true)
            {
                BigInteger candidate = GenerateRandomBigInteger(minValue, maxValue);
                if (candidate % 2 == 0) candidate++;

                if (IsProbablePrime(candidate, 10))
                    return candidate;
            }
        }

        // Генерация случайного BigInteger в диапазоне
        private BigInteger GenerateRandomBigInteger(BigInteger min, BigInteger max)
        {
            if (min > max) throw new ArgumentException("min must be less than or equal to max");

            byte[] bytes = max.ToByteArray();
            BigInteger result;

            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F;
                result = new BigInteger(bytes);
            } while (result < min || result > max);

            return result;
        }

        // Тест Миллера-Рабина
        private bool IsProbablePrime(BigInteger n, int k)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            BigInteger d = n - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            for (int i = 0; i < k; i++)
            {
                BigInteger a;
                do
                {
                    a = GenerateRandomBigInteger(2, n - 2);
                } while (a < 2);

                BigInteger x = ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                bool composite = true;
                for (int j = 0; j < r - 1; j++)
                {
                    x = ModPow(x, 2, n);
                    if (x == n - 1)
                    {
                        composite = false;
                        break;
                    }
                }

                if (composite) return false;
            }
            return true;
        }

        // Модульная инверсия (расширенный алгоритм Евклида)
        private BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;

            if (m == 1) return 0;

            while (a > 1)
            {
                BigInteger q = a / m;
                BigInteger t = m;

                m = a % m;
                a = t;
                t = y;

                y = x - q * y;
                x = t;
            }

            if (x < 0) x += m0;
            return x;
        }

        // Модульное возведение в степень
        private BigInteger ModPow(BigInteger baseValue, BigInteger exponent, BigInteger modulus)
        {
            if (modulus == 1) return 0;

            BigInteger result = 1;
            baseValue = baseValue % modulus;

            while (exponent > 0)
            {
                if (exponent % 2 == 1)
                    result = (result * baseValue) % modulus;

                baseValue = (baseValue * baseValue) % modulus;
                exponent = exponent >> 1;
            }

            return result;
        }

        // Текст → блоки (< N)
        private List<BigInteger> TextToBlocks(string text, BigInteger N)
        {
            var blocks = new List<BigInteger>();
            var bytes = Encoding.ASCII.GetBytes(text);
            string currentBlock = "";

            foreach (byte b in bytes)
            {
                string byteStr = b.ToString("D3");
                string testBlock = currentBlock + byteStr;

                if (BigInteger.Parse(testBlock) < N)
                {
                    currentBlock = testBlock;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentBlock))
                    {
                        blocks.Add(BigInteger.Parse(currentBlock));
                        currentBlock = byteStr;
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentBlock))
                blocks.Add(BigInteger.Parse(currentBlock));

            return blocks;
        }

        // Шифрование блоков
        private List<BigInteger> EncryptBlocks(List<BigInteger> blocks, RSAKey key)
            => blocks.Select(b => ModPow(b, key.E, key.N)).ToList();

        // Расшифровка блоков
        private List<BigInteger> DecryptBlocks(List<BigInteger> blocks, RSAKey key)
            => blocks.Select(b => ModPow(b, key.D, key.N)).ToList();

        // Блоки → текст
        private string BlocksToText(List<BigInteger> blocks)
        {
            var bytes = new List<byte>();

            foreach (var block in blocks)
            {
                string blockStr = block.ToString();

                if (blockStr.Length % 3 != 0)
                    blockStr = blockStr.PadLeft(((blockStr.Length / 3) + 1) * 3, '0');

                for (int i = 0; i < blockStr.Length; i += 3)
                {
                    string byteStr = blockStr.Substring(i, 3);
                    if (byte.TryParse(byteStr, out byte b))
                        bytes.Add(b);
                }
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private void Log(string msg)
        {
            tbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            tbLog.ScrollToEnd();
        }
    }

    public class RSAKeys
    {
        public RSAKey PublicKey { get; set; }
        public RSAKey PrivateKey { get; set; }
    }

    public class RSAKey
    {
        public BigInteger E { get; set; }
        public BigInteger D { get; set; }
        public BigInteger N { get; set; }
    }
}