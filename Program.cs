using System;
using Gtk;
using MyNamespace;
using System.Data;
using System.Data.SqlClient;

namespace Project {

  public struct Client {
    public int id;
    public string name;

    public Client(int id, string name){
      this.id = id;
      this.name = name;
    }
  }

  public class CustomCurencyManager{
    private CircularDoublyLinkedList list;
    private int items;

    public CustomCurencyManager(Client client){
      this.list = new CircularDoublyLinkedList(client);
      this.items++;
    }

    public MyNamespace.Node GetAllNode(){
      return this.list.GetRootNode();
    }

    public void Clear(){
      for(int i = 0; i < this.items; i++){
        this.list.DeleteFront();
      }
    }

    public MyNamespace.Node Current(){
      return this.list.GetCurrentNode();
    }

    public MyNamespace.Node Next(){
      return this.list.CurrentNodePrev();
    }

    public MyNamespace.Node Prev(){
      return this.list.CurrentNodeNext();
    }

    public MyNamespace.Node Insert(Client client){
      return this.list.InsertFront(client);
    }

    public void Traverse(){
      MyNamespace.Node root = this.GetAllNode();
      MyNamespace.Node temp = root;

      do {
        Console.WriteLine("Id: {0}, Name: {1}", temp.data.id, temp.data.name);
        temp = temp.prev;
      } while(temp != root);
    }

  }

  public class Program: Window {

    private Fixed container;

    private Label insertClientNameLabel,
                  insertClientIdLabel,
                  clientIdLabel,
                  clientIdText,
                  clientNameLabel,
                  clientNameText;

    private Entry insertClientNameEntry,
                  insertClientIdEntry;

    private Button insertButton,
                   deleteButton,
                   prevButton,
                   nextButton;

    private CustomCurencyManager currencyManager;

    private SqlConnection conn;

    public Program(): base("Project"){
      this.SetDefaultSize(650, 500);
      this.DeleteEvent += new DeleteEventHandler(this.Exit);

      this.DatabaseConnection();
      this.LoadData();

      this.container = new Fixed();
      this.Add(this.container);

      this.insertClientNameLabel = new Label();
      this.insertClientNameLabel.Text = "Client name";
      this.container.Put(this.insertClientNameLabel, 100, 30);

      this.insertClientNameEntry = new Entry();
      this.container.Put(this.insertClientNameEntry, 200, 30);

      this.insertButton = new Button("Insert");
      this.insertButton.Clicked += new EventHandler(this.Insert);
      this.container.Put(this.insertButton, 400, 30);

      this.insertClientIdLabel = new Label();
      this.insertClientIdLabel.Text = "Client id";
      this.container.Put(this.insertClientIdLabel, 100, 80);

      this.insertClientIdEntry = new Entry();
      this.container.Put(this.insertClientIdEntry, 200, 80);

      this.deleteButton = new Button("Delete by id");
      this.deleteButton.Clicked += new EventHandler(this.Delete);
      this.container.Put(this.deleteButton, 400, 80);

      this.clientIdLabel = new Label();
      this.clientIdLabel.Text = "Id:";
      this.container.Put(this.clientIdLabel, 150, 130);

      this.clientIdText = new Label();
      this.container.Put(this.clientIdText, 280, 130);

      this.clientNameLabel = new Label();
      this.clientNameLabel.Text = "Client name:";
      this.container.Put(this.clientNameLabel, 150, 180);

      this.clientNameText = new Label();
      this.container.Put(this.clientNameText, 280, 180);

      this.prevButton = new Button("Previous");
      this.prevButton.Clicked += new EventHandler(this.Prev);
      this.container.Put(this.prevButton, 50, 230);

      this.nextButton = new Button("Next");
      this.nextButton.Clicked += new EventHandler(this.Next);
      this.container.Put(this.nextButton, 380, 230);

      this.StartCurrencyManager();

      this.ShowAll();
    }

    private void Delete(object obj, EventArgs args){
      try {
        this.conn.Open();

        string sql = "deleteById";
        SqlCommand command = new SqlCommand(sql, this.conn);
        command.CommandType = CommandType.StoredProcedure;

        SqlParameter id = new SqlParameter();
        id = command.Parameters.Add("@id", SqlDbType.Int);
        id.Direction = ParameterDirection.Input;
        id.Value = this.insertClientIdEntry.Text;

        command.ExecuteNonQuery();

        command.Dispose();
        this.conn.Close();

        this.Restart();

      }catch(Exception e){
        Console.WriteLine(e.Message);
      }
    }

    private void Restart(){

      this.currencyManager.Clear();
      this.LoadData();
      this.StartCurrencyManager();
      this.insertClientNameEntry.Text = "";
      this.insertClientIdEntry.Text = "";
    }

    private void Insert(object obj, EventArgs args){
      try {
        this.conn.Open();

        string sql = "InsertClient";
        SqlCommand command = new SqlCommand(sql, this.conn);
        command.CommandType = CommandType.StoredProcedure;

        SqlParameter name = new SqlParameter();
        name = command.Parameters.Add("@name", SqlDbType.VarChar, 100);
        name.Direction = ParameterDirection.Input;
        name.Value = this.insertClientNameEntry.Text;

        command.ExecuteNonQuery();

        command.Dispose();
        this.conn.Close();

        this.Restart();

      } catch(Exception e){
        Console.WriteLine(e.Message);
      }
    }

    private void LoadData(){
      try {
        this.conn.Open();

        string sql = "getClients";

        SqlCommand command = new SqlCommand(sql, this.conn);
        command.CommandType = CommandType.StoredProcedure;

        SqlDataReader reader = command.ExecuteReader();

        reader.Read();

        this.currencyManager = new CustomCurencyManager(new Client(
          int.Parse((reader.GetValue(0)).ToString()),
          (reader.GetValue(1)).ToString()
        ));

        while(reader.Read()){
          this.currencyManager.Insert(new Client(
            int.Parse((reader.GetValue(0)).ToString()),
            (reader.GetValue(1)).ToString()
          ));
        }

        command.Dispose();
        this.conn.Close();
      } catch(Exception e){
        Console.WriteLine(e.Message);
      }
    }

    private void DatabaseConnection(){
      try {
        string connString = "Data source=localhost; Initial catalog=clients; User id=sa; Password=password@Q";
        this.conn = new SqlConnection(connString);
        this.conn.Open();

        Console.WriteLine("Connect to database successfully");
        this.conn.Close();
      } catch(Exception e){
        Console.WriteLine(e.Message);
      }
    }

    private void Next(object obj, EventArgs args){
      MyNamespace.Node current = this.currencyManager.Next();

      this.SetData(current.data);
    }

    private void Prev(object obj, EventArgs args){
      MyNamespace.Node current = this.currencyManager.Prev();

      this.SetData(current.data);
    }

    private void StartCurrencyManager(){
      MyNamespace.Node current = this.currencyManager.Current();
      this.SetData(current.data);
    }

    private void SetData(Client client){
      this.clientIdText.Text = client.id.ToString();
      this.clientNameText.Text = client.name;
    }

    private void Exit(object obj, DeleteEventArgs args){
      Application.Quit();
    }
  }
}
