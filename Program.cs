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
            .AddOption("Şifremi Unuttum", ForgotPassword)
            .AddOption("Mevcut Kullanıcıyı Gör", ShowCurrentUser)
            .AddOption("Çıkış Yap", Logout)
            .AddOption("Programdan Çık", () =>
            {
                Console.WriteLine("Programdan çıkılıyor...");
                Thread.Sleep(1000);
            });

        mainMenu.Show();
    }
    
    static void LoggedInUserMenu()
    {
        var userMenu = new ConsoleMenu("Kullanıcı Menüsü");

        userMenu
            .AddOption("Rumuz belirle", () => Console.WriteLine("Rumuzun nedir?"))
            .AddOption("Oda ara", () => Console.WriteLine("Oda ara"))
            .AddOption("Oda oluştur", () => Console.WriteLine("Oda oluştur"));

        userMenu.Show();
    }
    
    static void LoginUser()
    {
        var inputUsername = Helper.Ask("Kullanıcı adı", true);
        var inputPassword = Helper.AskPassword("Şifre");

        var loginStatus = Auth.Login(inputUsername, inputPassword, out var user);

        switch (loginStatus)
        {
            case Auth.LoginStatus.LoggedIn:
                _loggedInUser = user;
                LoggedInUserMenu();
                break;

            case Auth.LoginStatus.UserNotFound:
                Helper.ShowErrorMsg("Kullanıcın bulunamadı!");
                Thread.Sleep(1000);
                break;

            case Auth.LoginStatus.WrongCredentials:
                Helper.ShowErrorMsg("Eksik veya hatalı giriş yaptın!");
                Thread.Sleep(1000);
                break;
        }
    }
    
    static void ForgotPassword()
    {
        using var dbContext = new AppDbContext();

        var username = Helper.Ask("Kullanıcı adınızı girin", true);
        var user = dbContext.Users.FirstOrDefault(u => u.Username == username);

        if (user == null)
        {
            Helper.ShowErrorMsg("Bu kullanıcı bulunamadı.");
            return;
        }

        var nameCheck = Helper.Ask("Adınızı girin (doğrulama için)", true);
        if (!string.Equals(user.Name, nameCheck))
        {
            Helper.ShowErrorMsg("Doğrulama başarısız. Adınız uyuşmuyor.");
            return;
        }

        var newPassword = Helper.AskPassword("Yeni şifrenizi girin");
        var hashed = Hash(newPassword);

        user.Password = hashed;
        dbContext.Users.Update(user);
        dbContext.SaveChanges();

        Helper.ShowSuccessMsg("Şifreniz başarıyla güncellendi.");
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
        Console.WriteLine("\nMenüye dönmek için bir tuşa basın...");
        Console.ReadKey(true);
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
