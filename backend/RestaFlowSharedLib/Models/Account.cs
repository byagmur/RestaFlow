using Dapper;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RestaurantSharedLib.Models
{ 
public class Account
{

    private string name;
    private string surname;
    private string username;
    private string password;
    private string email;
    private int id;
    private bool isAuthorized;
    private string role;
    private string gsm;


        public Account()
    {

    }

    public static void Assign(Account source, Account  target)
    {
        target.Id = source.Id;
        target.Password = source.Password;
        target.Username = source.Username;
        target.IsAuthorized = source.IsAuthorized;
        target.email = source.email;

    }


        public Account(int id,string Name,string Surname, string username, string password, bool isAuthorized, string role)
        {
            this.id = id;
            this.Name = Name;
            this.Surname = Surname;
            this.username = username;
            this.password = password;
            this.isAuthorized = isAuthorized;
            this.role = role;
            if (LoggedUser == null)
            { LoggedUser = new Account(); }
            Assign(this, LoggedUser);
        }

    public Account(string mail, string username, string password, string v, string v1, bool isAuthorized)
    {
        this.email = mail ;
        this.username = username;
        this.password = password;
        this.isAuthorized = isAuthorized;
        if (LoggedUser == null)
        { LoggedUser = new Account(); }
        Assign(this, LoggedUser);
    }

    public String Username { get { return username; } set { username = value; } }
    public String Password { get { return password; } set { password = value; } }
    public int Id { get { return id; } set { id = value; } }
    public bool IsAuthorized { get { return isAuthorized ; } set { isAuthorized = value; } }
    public string Role { get { return role; } set { role = value; } }
    public string Name { get { return name; } set { name = value; } }
    public string Surname { get { return surname; } set { surname = value; } }
    public string FullName { get { return $"{Name} {Surname}"; } }
    public string Gsm { get { return gsm; } set { gsm = value; } }
    public string Mail { get; set; }

    public static Account LoggedUser = null;

      

    }

}
