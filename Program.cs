using System.Security.Cryptography;
using System.Text;
using ConsoleProject.Data;
using ConsoleProject.Helpers;
using ConsoleProject.Models;

namespace ConsoleProject;

static class Program
{
    static Users? _loggedInUser = null;

    static void Main(string[] args)
    {
        var mainMenu = new ConsoleMenu("Console Chat Uygulamasına Hoş Geldin!", true);

        mainMenu
            .AddMenu("Giriş Yap", LoginUser)
            .AddMenu("Kayıt Ol", RegisterUser)
            .AddOption("Mevcut Kullanıcıyı Gör", ShowCurrentUser)
            .AddOption("Çıkış Yap", Logout)
            .AddOption("Programdan Çık", () =>
            {
                Console.WriteLine("Programdan çıkılıyor...");
                Thread.Sleep(1000);
            });

        mainMenu.Show();
    }

    static void LoginUser()
    {
        using var dbContext = new AppDbContext();

        while (true)
        {
            var username = Helper.Ask("Kullanıcı adı", true);
            var password = Helper.AskPassword("Şifre");
            var hashed = Hash(password);

            var user = dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || user.Password != hashed)
            {
                Helper.ShowErrorMsg("Bu kullanıcının kaydı olmayabilir ya da eksik ve hatalı giriş yaptınız.");
                Console.WriteLine(); // boş satırla ayır
                continue; // yeniden sorsun
            }

            _loggedInUser = user;
            Helper.ShowSuccessMsg($"Giriş başarılı! Hoş geldin, {_loggedInUser.Name}.");
            Thread.Sleep(1000); // menüye dönmeden önce bekleme
            break;
        }
    }
    static void RegisterUser()
    {
        using var dbContext = new AppDbContext();

        var name = Helper.Ask("Ad", true);
        while (dbContext.Users.Any(u => u.Name == name))
        {
            Helper.ShowErrorMsg("Bu isimde bir kullanıcı zaten kayıtlı. Lütfen başka bir isim girin.");
            name = Helper.Ask("Ad", true);
        }

        var username = Helper.Ask("Kullanıcı adı", true);
        while (dbContext.Users.Any(u => u.Username == username))
        {
            Helper.ShowErrorMsg("Bu kullanıcı adı zaten alınmış. Lütfen farklı bir kullanıcı adı girin.");
            username = Helper.Ask("Kullanıcı adı", true);
        }

        var password = Helper.AskPassword("Şifre");
        var hashed = Hash(password);

        var newUser = new Users
        {
            Name = name,
            Username = username,
            Password = hashed
        };

        dbContext.Users.Add(newUser);
        dbContext.SaveChanges();

        Helper.ShowSuccessMsg("Kayıt başarılı.");
    }

    static void ShowCurrentUser()
    {
        if (_loggedInUser != null)
        {
            Console.WriteLine($"Giriş yapan kullanıcı: {_loggedInUser.Name} ({_loggedInUser.Username})");
        }
        else
        {
            Helper.ShowInfoMsg("Şu anda giriş yapan kullanıcı yok.");
        }
    }

    static void Logout()
    {
        if (_loggedInUser != null)
        {
            Console.WriteLine($"{_loggedInUser.Name} çıkış yaptı.");
            _loggedInUser = null;
        }
        else
        {
            Helper.ShowInfoMsg("Zaten giriş yapılmamış.");
        }
    }

    static string Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
